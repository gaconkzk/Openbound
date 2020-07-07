/* 
 * Copyright (C) 2020, Carlos H.M.S. <carlos_judo@hotmail.com>
 * This file is part of OpenBound.
 * OpenBound is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or(at your option) any later version.
 * 
 * OpenBound is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with OpenBound. If not, see http://www.gnu.org/licenses/.
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenBound.Common;
using OpenBound.Extension;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Animation.InGame;
using OpenBound.GameComponents.Audio;
using OpenBound.GameComponents.Debug;
using OpenBound.GameComponents.Input;
using OpenBound.GameComponents.Interface;
using OpenBound.GameComponents.Level;
using OpenBound.GameComponents.Level.Scene;
using OpenBound.GameComponents.MobileAction;
using OpenBound.GameComponents.Asset;
using OpenBound.ServerCommunication.Service;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Entity.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using Openbound_Network_Object_Library.Models;
using OpenBound.GameComponents.Collision;
using OpenBound.GameComponents.MobileAction.Motion;

namespace OpenBound.GameComponents.Pawn
{
    public abstract class Mobile : Actor
    {
        public Rider Rider;

        public MobileFlipbook MobileFlipbook;

        // Mobile Name
        public MobileType MobileType;

        //Crosshair Wrapper
        public Crosshair Crosshair;

        //Mobile Information
        public MobileMetadata MobileMetadata;

        //Collision
        public CollisionBox CollisionBox;

        //Projectiles
        public List<Projectile> ProjectileList;

        public List<Projectile> LastCreatedProjectileList;

        public List<Projectile> UnusedProjectile;

        public ShotType SelectedShotType;

        public bool IsHealthCritical => MobileMetadata != null && (MobileMetadata.CurrentHealth / MobileMetadata.BaseHealth < Parameter.HealthBarLowHealthThreshold);
        public bool IsEnemy => Owner.PlayerTeam != GameInformation.Instance.PlayerInformation.PlayerTeam;

        public SyncMobile SyncMobile;
        public bool ForceSynchronize;
        //Positioning sync variable
        public Vector2 SyncPosition;

        public bool IsPlayable;
        public bool IsAbleToShoot;
        public bool IsAlive;
        public bool IsSummon;

        //IsActionsLocked
        public bool IsActionsLocked;
        bool hasShotSequenceStarted;

        //Sound Effect
        public SoundEffect movingSE, unableToMoveSE;

#if DEBUG
        //Debug Variables
        DebugCrosshair debugCrosshair = new DebugCrosshair(Color.Red);
        DebugCrosshair debugCrosshair2 = new DebugCrosshair(Color.HotPink);
#endif

        public Mobile(Player player, Vector2 position, MobileType mobileType, Vector2 riderPositionOffset = default, bool IsSummon = false) : base()
        {
            ProjectileList = new List<Projectile>();
            UnusedProjectile = new List<Projectile>();
            LastCreatedProjectileList = new List<Projectile>();

            MobileType = mobileType;
            Owner = player;

            movingSE = AssetHandler.Instance.RequestSoundEffect(SoundEffectParameter.MobileMovement(mobileType));
            unableToMoveSE = AssetHandler.Instance.RequestSoundEffect(SoundEffectParameter.MobileUnableToMove(mobileType));

            this.IsSummon = IsSummon;

            IsPlayable = GameInformation.Instance.IsOwnedByPlayer(this) && !IsSummon;

            if (IsPlayable)
                Movement = new LocalMovement(this);
            else if (IsSummon)
                Movement = new AutomatedMovement(this);
            else
                Movement = new RemoteMovement(this);

            MobileMetadata = MobileMetadata.BuildMobileMetadata(mobileType);

            Position = position;
            MobileFlipbook = MobileFlipbook.CreateMobileFlipbook(MobileType, position);

            if (!IsSummon)
            {
                Rider = new Rider(this, riderPositionOffset);
                Crosshair = new Crosshair(this);
            }

            //Sync
            SyncMobile = new SyncMobile();
            SyncMobile.Owner = player;
            SyncMobile.Position = Position.ToArray<int>();
            SyncMobile.MobileMetadata = MobileMetadata.BuildMobileMetadata(player.PrimaryMobile);
            SyncPosition = Position;

            IsAlive = true;
            IsActionsLocked = false;
            hasShotSequenceStarted = false;

#if DEBUG
            DebugHandler.Instance.Add(debugCrosshair);
            DebugHandler.Instance.Add(debugCrosshair2);
#endif
        }

        public override void Update(GameTime gameTime)
        {
            //Must be called before Movement.Update()
            UpdateFlipbookRotation();

            SyncPosition = Position;

            Movement.Update();

            if (IsPlayable && IsAbleToShoot)
            {
                Shoot(gameTime);
            }
            else
            {
                SyncShootHandler();
            }

            if (IsAlive && Crosshair != null) Crosshair.Update(gameTime);

            ProjectileList.ForEach((x) => x.Update());
            UnusedProjectile.ForEach((x) => ProjectileList.Remove(x));
            UnusedProjectile.Clear();
            LastCreatedProjectileList.ForEach((x) => ProjectileList.Add(x));
            LastCreatedProjectileList.Clear();

            CollisionBox.Update();

            SendRequestToServer();

            Rider?.Update();

#if DEBUG
                //Debug
            debugCrosshair.Update(Position);
            debugCrosshair2.Update(MobileFlipbook.Position);
#endif
        }

        public new virtual void PlayMovementSE(float pitch = 0, float pan = 0)
        {
            if (IsAbleToShoot)
                AudioHandler.PlayUniqueSoundEffect(movingSE, () => IsStateMoving(MobileFlipbook.State), pitch: pitch, pan: pan);
        }

        public new virtual void PlayUnableToMoveSE(float pitch = 0, float pan = 0)
        {
            if (IsAbleToShoot)
                AudioHandler.PlayUniqueSoundEffect(unableToMoveSE, () => MobileFlipbook.State == ActorFlipbookState.UnableToMove, pitch: pitch, pan: pan);
        }

        public virtual void Die()
        {
            if (!IsAlive) return;

            if (Rider != null) Rider.Hide();

            DeathAnimation.Add(this);
            ChangeFlipbookState(ActorFlipbookState.Dead, true);
            IsAlive = false;
        }

        public void HandleSyncMobile(SyncMobile syncMobile)
        {
            if (IsPlayable) return;

            SyncMobile.Update(syncMobile);

            //Position
            ((RemoteMovement)Movement).EnqueuePosition(Topography.GetTransformedPosition(new Vector2(syncMobile.Position[0], syncMobile.Position[1])).ToVector2());

            //ShotType
            ChangeShot(SyncMobile.SelectedShotType);
        }

        public void RequestLoseTurn()
        {
            LoseTurn();
            ///ServerInformationHandler.RequestNextPlayerTurn();
        }

        public virtual void LoseTurn()
        {
            Movement.IsAbleToMove = false;
            LevelScene.HUD.LoseTurn();
            IsAbleToShoot = false;
            //MobileFlipbook.ChangeState(MobileFlipbookState.Stand, true);
            //MobileFlipbook.EnqueueAnimation(MobileFlipbookState.Stand);

            if (IsPlayable)
                Crosshair.FadeElement();
            else
                Crosshair.HideElement();
        }

        public virtual void GrantTurn()
        {
            LevelScene.MatchMetadata.CurrentTurnOwner = SyncMobile;

            Movement.RemainingStepsThisTurn = Movement.MaximumStepsPerTurn;
            Movement.IsAbleToMove = true;
            IsAbleToShoot = true;

            if (IsPlayable)
                LevelScene.HUD.GrantTurn();

            Crosshair.PlayAnimation();

            AudioHandler.PlaySoundEffect(SoundEffectParameter.InGameGameplayNewTurn);

            GameScene.Camera.TrackObject(this);
        }

        /// <summary>
        /// Flip method, must be called when the player forces mobile to turn to the opposite direction
        /// </summary>
        public new virtual void Flip()
        {
            base.Flip();
            MobileFlipbook.Flip();
            Crosshair?.Flip();
            Rider?.Flip();
        }

        public void SendRequestToServer()
        {
            if (!IsPlayable) return;

            #region Left-Right sync logic
            Vector2 syncPosition = Position;

            if (InputHandler.IsBeingPressed(Keys.Left))
            {
                ForceSynchronize = true;
                SyncMobile.AddSynchronizableAction(SynchronizableAction.LeftMovement);

                if (!Movement.IsAbleToMove && IsAbleToShoot)
                    SyncMobile.AddSynchronizableAction(SynchronizableAction.UnableToMove);
                else
                    syncPosition = SyncPosition;
            }
            else if (InputHandler.IsBeingHeldDown(Keys.Left))
            {
                if (!Movement.IsAbleToMove)
                    SyncMobile.AddSynchronizableAction(SynchronizableAction.UnableToMove);
            }
            else if (InputHandler.IsBeingReleased(Keys.Left))
            {
                ForceSynchronize = true;
                SyncMobile.RemoveSynchronizableAction(SynchronizableAction.LeftMovement);
                SyncMobile.RemoveSynchronizableAction(SynchronizableAction.UnableToMove);
            }
            else if (InputHandler.IsBeingPressed(Keys.Right))
            {
                ForceSynchronize = true;
                SyncMobile.AddSynchronizableAction(SynchronizableAction.RightMovement);

                if (!Movement.IsAbleToMove && IsAbleToShoot)
                    SyncMobile.AddSynchronizableAction(SynchronizableAction.UnableToMove);
                else
                    syncPosition = SyncPosition;
            }
            else if (InputHandler.IsBeingHeldDown(Keys.Right))
            {
                if (!Movement.IsAbleToMove)
                    SyncMobile.AddSynchronizableAction(SynchronizableAction.UnableToMove);
            }
            else if (InputHandler.IsBeingReleased(Keys.Right))
            {
                ForceSynchronize = true;
                SyncMobile.RemoveSynchronizableAction(SynchronizableAction.RightMovement);
                SyncMobile.RemoveSynchronizableAction(SynchronizableAction.UnableToMove);
            }
            #endregion

            #region Up-down sync logic
            if (InputHandler.IsBeingReleased(Keys.Up) || InputHandler.IsBeingReleased(Keys.Down))
                ForceSynchronize = true;
            SyncPosition = syncPosition;
            #endregion

            #region Spacebar sync logic
            if (InputHandler.IsBeingPressed(Keys.Space) && IsAbleToShoot)
            {
                ForceSynchronize = true;
                SyncMobile.AddSynchronizableAction(SynchronizableAction.ChargingShot);
            }
            else if ((InputHandler.IsBeingReleased(Keys.Space) || LevelScene.HUD.StrenghtBar.IsFull) && IsAbleToShoot)
            {
                ForceSynchronize = true;
                SyncMobile.RemoveSynchronizableAction(SynchronizableAction.ChargingShot);
            }
            #endregion

            if (ForceSynchronize)
            {
                ForceSynchronize = false;
                UpdateSyncMobileToServer();

#if !DEBUGSCENE
                ServerInformationHandler.SynchronizeMobileStatus(SyncMobile);
#endif
            }
        }

        public void RequestDeath()
        {
#if !DEBUGSCENE
            ServerInformationHandler.RequestDeath(SyncMobile);
#endif
        }

        public virtual void UpdateSyncMobileToServer()
        {
            SyncMobile.MobileMetadata = MobileMetadata;
            SyncMobile.Position = Topography.GetRelativePosition(SyncPosition);
            SyncMobile.SelectedShotType = SelectedShotType;
            SyncMobile.CrosshairAngle = Crosshair.ShootingAngle;
            SyncMobile.Facing = Facing;
        }

        public virtual void ChangeShot(ShotType ShotType)
        {
            if (SelectedShotType == ShotType) return;
            SelectedShotType = ShotType;
            Crosshair.ChangeShot(ShotType);

            if (IsPlayable) ForceSynchronize = true;
        }

        /// <summary>
        /// Updates the flipbook position depending on its rotation towards the normal
        /// </summary>
        public void UpdateFlipbookRotation()
        {
            //Flipbook Rotation
            Vector2 offsetVector = new Vector2(
                Movement.CollisionOffset * (float)Math.Cos(MobileFlipbook.Rotation + Math.PI / 2),
                Movement.CollisionOffset * (float)Math.Sin(MobileFlipbook.Rotation + Math.PI / 2));

            MobileFlipbook.Position = Position - offsetVector;
        }

        public void Shoot(GameTime GameTime)
        {
            if (InputHandler.IsBeingPressed(Keys.Space) && !IsActionsLocked)
            {
                LevelScene.HUD.StrenghtBar.Reset();
                hasShotSequenceStarted = true;
            }
            else if (InputHandler.IsBeingHeldDown(Keys.Space)
                && !LevelScene.HUD.StrenghtBar.IsFull && hasShotSequenceStarted && !IsActionsLocked)
            {
                LevelScene.HUD.StrenghtBar.PerformStep(GameTime);

                if (SelectedShotType == ShotType.S1)
                    ChangeFlipbookState(ActorFlipbookState.ChargingS1, true);
                else if (SelectedShotType == ShotType.S2)
                    ChangeFlipbookState(ActorFlipbookState.ChargingS2, true);
                else if (SelectedShotType == ShotType.SS)
                    ChangeFlipbookState(ActorFlipbookState.ChargingSS, true);
            }
            else if ((InputHandler.IsBeingReleased(Keys.Space) || LevelScene.HUD.StrenghtBar.IsFull) && hasShotSequenceStarted)
            {
                LevelScene.HUD.UpdatePreviousShotMarker();

                RequestShoot();
                hasShotSequenceStarted = false;
            }
        }

        public void SyncShootHandler()
        {
            if (SyncMobile != null && SyncMobile.ContainsAction(SynchronizableAction.ChargingShot))
            {
                if (SelectedShotType == ShotType.S1) ChangeFlipbookState(ActorFlipbookState.ChargingS1, true);
                else if (SelectedShotType == ShotType.S2) ChangeFlipbookState(ActorFlipbookState.ChargingS2, true);
                else if (SelectedShotType == ShotType.SS) ChangeFlipbookState(ActorFlipbookState.ChargingSS, true);
            }
        }

        protected bool IsStateMoving(ActorFlipbookState state) => state == ActorFlipbookState.MovingLowHealth || state == ActorFlipbookState.Moving;
        protected bool IsStateStand(ActorFlipbookState state) => state == ActorFlipbookState.Stand || state == ActorFlipbookState.StandLowHealth;
        protected bool IsStateShooting(ActorFlipbookState state) => state == ActorFlipbookState.ShootingS1 || state == ActorFlipbookState.ShootingS2 || state == ActorFlipbookState.ShootingSS;
        protected bool IsStateCharging(ActorFlipbookState state) => state == ActorFlipbookState.ChargingS1 || state == ActorFlipbookState.ChargingS2 || state == ActorFlipbookState.ChargingSS;
        protected bool IsStateBeingDamaged(ActorFlipbookState state) => state == ActorFlipbookState.BeingDamaged1 || state == ActorFlipbookState.BeingDamaged2 || state == ActorFlipbookState.DepletingShield || state == ActorFlipbookState.BeingFrozen || state == ActorFlipbookState.BeingShocked;
        protected bool IsStateEmoting(ActorFlipbookState state) => state == ActorFlipbookState.Emotion1 || state == ActorFlipbookState.Emotion2;
        protected bool IsStateDead(ActorFlipbookState state) => state == ActorFlipbookState.Dead;
        protected bool IsStateFalling(ActorFlipbookState state) => state == ActorFlipbookState.Falling;
        protected bool IsStateUnableToMove(ActorFlipbookState state) => state == ActorFlipbookState.UnableToMove;
        protected bool IsStateUsingItem(ActorFlipbookState state) => state == ActorFlipbookState.UsingItem;

        /// <summary>
        /// Mobile's flipbook change state machine
        /// </summary>
        public virtual void ChangeFlipbookState(ActorFlipbookState NewState, bool Force = false)
        {
            if (IsHealthCritical)
            {
                if (NewState == ActorFlipbookState.Stand)
                    NewState = ActorFlipbookState.StandLowHealth;
                else if (NewState == ActorFlipbookState.Moving)
                    NewState = ActorFlipbookState.MovingLowHealth;
            }

            if (NewState == MobileFlipbook.State) return;

            if (IsStateDead(MobileFlipbook.State)) return;

            if (IsStateUnableToMove(MobileFlipbook.State) && IsStateStand(NewState)) Force = true;

            if (IsStateMoving(MobileFlipbook.State) && IsStateStand(NewState)) Force = true;

            if (IsStateCharging(MobileFlipbook.State) && IsStateStand(NewState)) return;

            //Moving & Shooting condition
            if (IsStateCharging(MobileFlipbook.State) && IsStateMoving(NewState)) Force = true;

            if (IsStateMoving(MobileFlipbook.State) && IsStateCharging(NewState)) return;

            //Being shot and start moving
            if (IsStateBeingDamaged(MobileFlipbook.State) && IsStateStand(NewState)) return;

            if (IsStateShooting(MobileFlipbook.State) && IsStateStand(NewState)) return;

            //Stand Condition
            if (Force ||
            (IsStateStand(MobileFlipbook.State) && (IsStateMoving(NewState) || IsStateEmoting(NewState) || IsStateBeingDamaged(NewState) ||
                IsStateCharging(NewState) || IsStateUsingItem(NewState) || IsStateDead(NewState) || IsStateFalling(NewState))) ||

            //Moving || MovingLowHealth
            (IsStateMoving(MobileFlipbook.State) && (IsStateStand(NewState) || IsStateDead(NewState) || IsStateShooting(NewState) || IsStateFalling(NewState))) ||

            //Unable to move
            (IsStateUnableToMove(MobileFlipbook.State) && (IsStateStand(NewState) || IsStateMoving(NewState) || IsStateShooting(NewState))) ||

            //Falling
            (IsStateFalling(MobileFlipbook.State) && (IsStateStand(NewState) || IsStateMoving(NewState) || IsStateUnableToMove(NewState))) ||

            //Charging
            (IsStateCharging(MobileFlipbook.State) && (IsStateMoving(NewState) || IsStateUnableToMove(NewState) || IsStateEmoting(NewState) ||
            IsStateBeingDamaged(NewState) || IsStateShooting(NewState) || IsStateUsingItem(NewState))))
            {
                MobileFlipbook.ChangeState(NewState, Force);
            }
        }

        public void DepleteShield()
        {
            if (MobileMetadata.BaseShield == 0) return;

            if (MobileFlipbook.StatePresets.ContainsKey(ActorFlipbookState.DepletingShield))
            {
                ChangeFlipbookState(ActorFlipbookState.DepletingShield, true);
            }
            else
            {
                switch (Parameter.Random.Next(0, 2))
                {
                    case 0:
                        ChangeFlipbookState(ActorFlipbookState.BeingDamaged1, true);
                        break;
                    case 1:
                        ChangeFlipbookState(ActorFlipbookState.BeingDamaged2, true);
                        break;
                }
            }

            if (IsHealthCritical) MobileFlipbook.EnqueueAnimation(ActorFlipbookState.StandLowHealth);
            else MobileFlipbook.EnqueueAnimation(ActorFlipbookState.Stand);
        }

        public override void ReceiveShock(int damage)
        {
            ChangeFlipbookState(ActorFlipbookState.BeingShocked, true);

            if (IsHealthCritical) MobileFlipbook.EnqueueAnimation(ActorFlipbookState.StandLowHealth);
            else MobileFlipbook.EnqueueAnimation(ActorFlipbookState.Stand);

            //Damage Handling - The damage should deplete the shield first
            MobileMetadata.CurrentShield -= damage;

            if (MobileMetadata.CurrentShield < 0)
            {
                MobileMetadata.CurrentHealth += MobileMetadata.CurrentShield;
                MobileMetadata.CurrentShield = 0;

                if (MobileMetadata.CurrentHealth == 0)
                    MobileMetadata.CurrentHealth = 0;
            }

            LevelScene.HUD.FloatingTextHandler.AddDamage(this, -damage);
        }

        public override void ReceiveDamage(int damage)
        {
            //Play the "BeingDamaged" animation, then enqueue the stand animation
            switch (Parameter.Random.Next(0, 2))
            {
                case 0:
                    ChangeFlipbookState(ActorFlipbookState.BeingDamaged1, true);
                    break;
                case 1:
                    ChangeFlipbookState(ActorFlipbookState.BeingDamaged2, true);
                    break;
            }

            if (IsHealthCritical) MobileFlipbook.EnqueueAnimation(ActorFlipbookState.StandLowHealth);
            else MobileFlipbook.EnqueueAnimation(ActorFlipbookState.Stand);

            //Damage Handling - The damage should deplete the shield first
            MobileMetadata.CurrentShield -= damage;

            if (MobileMetadata.CurrentShield < 0)
            {
                MobileMetadata.CurrentHealth += MobileMetadata.CurrentShield;
                MobileMetadata.CurrentShield = 0;

                if (MobileMetadata.CurrentHealth == 0)
                    MobileMetadata.CurrentHealth = 0;
            }

            LevelScene.HUD.FloatingTextHandler.AddDamage(this, -damage);
        }

        public void RequestShoot()
        {
            //Prepare the function
            SyncMobile.SyncProjectile = new SyncProjectile();
            SyncMobile.SyncProjectile.ShotStrenght = LevelScene.HUD.StrenghtBar.Intensity;
            SyncMobile.SyncProjectile.ShotType = SelectedShotType;
            SyncMobile.SyncProjectile.ShotAngle = Crosshair.GetProjectileTrajetoryAngle();
            SyncMobile.SyncProjectile.CannonPosition = Crosshair.CannonPosition.ToArray<float>();
            SyncMobile.RemoveSynchronizableAction(SynchronizableAction.ChargingShot);
            SyncPosition = Position;
            UpdateSyncMobileToServer();

#if !DEBUGSCENE
            ServerInformationHandler.RequestShot(SyncMobile);
#else
            Shoot();
#endif

            IsAbleToShoot = Movement.IsAbleToMove = false;
        }

        public void ConsumeShootAction()
        {
            Shoot();

            LastCreatedProjectileList.ForEach((x) => x.OnFinalizeExecutionAction = () => 
            {
                ServerInformationHandler.RequestNextPlayerTurn(); 
            });
            GameScene.Camera.TrackObject(LastCreatedProjectileList.First());
        }

        protected virtual void Shoot()
        {
            //Animation
            if (SelectedShotType == ShotType.S1)
                ChangeFlipbookState(ActorFlipbookState.ShootingS1, true);
            else if (SelectedShotType == ShotType.S2)
                ChangeFlipbookState(ActorFlipbookState.ShootingS2, true);
            else if (SelectedShotType == ShotType.SS)
                ChangeFlipbookState(ActorFlipbookState.ShootingSS, true);

            if (IsHealthCritical) MobileFlipbook.EnqueueAnimation(ActorFlipbookState.StandLowHealth);
            else MobileFlipbook.EnqueueAnimation(ActorFlipbookState.Stand);

            //Initialize Projectiles
            LastCreatedProjectileList.ForEach((x) => x.InitializeMovement());
        }

        public new virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            MobileFlipbook.Draw(gameTime, spriteBatch);
            if (IsAlive) Crosshair?.Draw(null, spriteBatch);

            ProjectileList.ForEach((x) => x.Draw(gameTime, spriteBatch));
            Rider?.Draw(gameTime, spriteBatch);
        }
    }
}
