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
using Microsoft.Xna.Framework.Graphics;
using OpenBound.Common;
using OpenBound.Extension;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Audio;
using OpenBound.GameComponents.Level;
using OpenBound.GameComponents.Level.Scene;
using OpenBound.GameComponents.Pawn;
using OpenBound.GameComponents.Physics;
using OpenBound.GameComponents.WeatherEffect;
using Openbound_Network_Object_Library.Entity;
using System;
using System.Collections.Generic;

namespace OpenBound.GameComponents.PawnAction
{
    public enum ProjectileAnimationState
    {
        Spawning,
        Moving,

        Closed,
        Opening,
        Opened,
    }

    public abstract class Projectile
    {
        public List<Flipbook> FlipbookList { get; protected set; }

        protected Vector2 projectileInitialPosition;
        protected Vector2 projectileOffset, previousProjectileOffset;

        //Physics-related variables
        protected AcceleratedMovement yMovement, xMovement;
        protected float xSpeedComponent, ySpeedComponent;

        protected float mass, angle, windInfluence;
        protected float force;

        public bool IsAbleToRefreshPosition;
        public bool IsExternallyRefreshingPosition;

        //Physics-related variables - Wind
        protected float ywSpeedComponent, xwSpeedComponent;
        protected float wAngle, wForce;

        //Camera Tracking
        public Vector2 Position
        {
            get => FlipbookList[0].Position;
            set => FlipbookList[0].Position = value;
        }

        //Rotation-related variables
        protected Vector2 previousPosition;

        //Base Damage
        public int BaseDamage { get; set; }
        public int ExplosionRadius { get; set; }

        //Behaviour
        protected double SpawnTimeCounter, FreezeTimeCounter;
        protected double SpawnTime, FreezeTime;

        //Object References
        protected Mobile mobile;
        protected ShotType shotType;

        //Action handlers
        public Action OnFinalizeExecutionAction;
        public Action OnExplodeAction;
        public Action OnBeingDestroyedAction;
        public Action<int> OnDestroyGroundAction;
        public Action<int> OnDealDamageAction;

        //CanExplode
        public bool CanCollide { get; private set; }

        //Debug
#if Debug
        DebugCrosshair debugCrosshair;
#endif
        public Projectile(Mobile owner, ShotType shotType, int explosionRadius, int baseDamage, Vector2 projectileInitialPosition = default, float angleModifier = 0, float forceModifier = 0, bool canCollide = true)
        {
            this.shotType = shotType;
            CanCollide = canCollide;
            BaseDamage = baseDamage;
            ExplosionRadius = explosionRadius;

            FlipbookList = new List<Flipbook>();

            mobile = owner;

            //Physics-relaetd variables
            yMovement = new AcceleratedMovement();
            xMovement = new AcceleratedMovement();

            projectileOffset = Vector2.Zero;

            IsAbleToRefreshPosition = true;
            IsExternallyRefreshingPosition = false;

            if (projectileInitialPosition != default)
                this.projectileInitialPosition = previousPosition = projectileInitialPosition;
            else
                this.projectileInitialPosition = previousPosition = mobile.SyncMobile.SyncProjectile.CannonPosition.ToVector2();

            force = mobile.SyncMobile.SyncProjectile.ShotStrenght;
            angle = mobile.SyncMobile.SyncProjectile.ShotAngle;

            angle += angleModifier;
            force += forceModifier;

            xSpeedComponent = (float)Math.Round(Math.Cos(angle), 3);
            ySpeedComponent = (float)Math.Round(Math.Sin(angle), 3);

            //Physics-related variables - Wind
            wForce = LevelScene.MatchMetadata.WindForce;
            wAngle = MathHelper.ToRadians(LevelScene.MatchMetadata.WindAngleDegrees);
            xwSpeedComponent = (float)Math.Round(Math.Cos(wAngle), 3);
            ywSpeedComponent = (float)Math.Round(Math.Sin(wAngle), 3);
#if Debug
            debugCrosshair = new DebugCrosshair(Color.Yellow);
            DebugHandler.Instance.Add(debugCrosshair);
            Log.Write($"x: {projectileInitialPosition.X} | y: {projectileInitialPosition.Y} | f: {force} | a: {angle}");
#endif
        }

        public virtual void InitializeMovement()
        {
            yMovement.Preset(ySpeedComponent * force * Parameter.ProjectileMovementForceFactor / mass, Parameter.ProjectileMovementGravity + ywSpeedComponent * wForce * windInfluence);
            xMovement.Preset(xSpeedComponent * force * Parameter.ProjectileMovementForceFactor / mass, xwSpeedComponent * wForce * windInfluence);
        }

        public virtual void Update()
        {
            if (SpawnTimeCounter < SpawnTime || (SpawnTime == 0 && SpawnTimeCounter == 0))
            {
                SpawnTimeCounter += Parameter.GameplayTimeFrameMaximumDeltaTime;

                //If should play sfx when spawn AND has spawned
                if (SpawnTimeCounter >= SpawnTime)
                    OnSpawn();
                else
                    return;
            }

            UpdatePosition();
            UpdateRotation();
        }

        public void PlayLaunchSFX()
        {
            //Sound
            AudioHandler.PlaySoundEffect(SoundEffectParameter.MobileProjectileLaunch(mobile.MobileType, shotType));
        }

        public void PlayExplosionSFX()
        {
            ShotType st = shotType;

            //Change audio output in case the mobile projectile's parameter are changed to accomodate the SFX logic
            switch (mobile.MobileType)
            {
                case MobileType.Knight:
                    if (st == ShotType.Satellite)
                        st = ShotType.S1;
                    break;
            }

            AudioHandler.PlaySoundEffect(SoundEffectParameter.MobileProjectileExplosion(mobile.MobileType, st));
        }

        public virtual void OnSpawn()
        {
            PlayLaunchSFX();
        }

        public virtual void OnStartUpdating()
        {

        }

        /// <summary>
        /// This method updates the projectile position and interpolates it.
        /// All the motion is calculated using the parabolic throw formula.
        /// </summary>
        /// <param name="GameTime"></param>
        protected virtual void UpdatePosition()
        {
            if (FreezeTimeCounter <= FreezeTime)
            {
                FreezeTimeCounter += Parameter.GameplayTimeFrameMaximumDeltaTime;

                if (FreezeTimeCounter >= FreezeTime)
                    OnStartUpdating();
                else
                    return;
            }

            //Interpolate Movement for collision
            for (float elapsedTime = 0; elapsedTime < Parameter.ProjectileMovementTotalTimeElapsed; elapsedTime += Parameter.ProjectileMovementTimeElapsedPerInteraction)
            {
                UpdateMovementIteraction(Parameter.ProjectileMovementTimeElapsedPerInteraction);

                if (CheckOutOfBounds(Position) || UpdateCollider(Position)) break;
            }

#if Debug
            debugCrosshair.Update(FlipbookList[0].Position);
#endif
        }

        public Vector2 SpeedVector => new Vector2(xMovement.CurrentSpeed, yMovement.CurrentSpeed);
        public Vector2 InitialSpeedVector => new Vector2(xMovement.InitialSpeed, yMovement.InitialSpeed);
        public Vector2 CurrentFlipbookAngleVector => new Vector2((float)Math.Cos(FlipbookList[0].Rotation), (float)Math.Sin(FlipbookList[0].Rotation));
        public float CurrentFlipbookRotation => FlipbookList[0].Rotation;

        protected virtual void CheckCollisionWithWeather()
        {
            foreach (Weather w in LevelScene.WeatherHandler.WeatherList)
            {
                w.CheckProjectileInteraction(this);
            }
        }

        public virtual void OnBeginTornadoInteraction() { }

        public virtual void OnEndTornadoInteraction() { }

        public virtual void OnBeginForceInteraction(Force force) { }

        public virtual void OnBeginWeaknessInteraction(Weakness weakness) { }

        public void SetBasePosition(Vector2 newPosition)
        {
            projectileInitialPosition = newPosition;
            xSpeedComponent = (float)Math.Round(Math.Cos(CurrentFlipbookRotation), 3);
            ySpeedComponent = (float)Math.Round(Math.Sin(CurrentFlipbookRotation), 3);
            InitializeMovement();
        }

        protected virtual void UpdateMovementIteraction(float timeElapsedPerIteraction)
        {
            if (!IsAbleToRefreshPosition) return;

            CheckCollisionWithWeather();

            if (IsExternallyRefreshingPosition) return;

            //Normal movement positioning
            yMovement.RefreshCurrentPosition(timeElapsedPerIteraction);
            xMovement.RefreshCurrentPosition(timeElapsedPerIteraction);

            Vector2 newPosition = projectileInitialPosition + new Vector2(xMovement.CurrentPosition, yMovement.CurrentPosition);

            //Update all projectile list related to the new position
            FlipbookList.ForEach((x) => x.Position = newPosition);
        }

        public virtual bool CheckOutOfBounds(Vector2 position)
        {
            if (Topography.IsNotInsideMapBoundaries(position))
            {
                OutofboundsDestroy();
                return true;
            }

            return false;
        }

        public bool UpdateCollider(Vector2 position)
        {
            bool hasExploded = false;

            //Check collision with ground
            if (CanCollide && Topography.CheckCollision(position))
            {
                hasExploded = true;
                Explode();
#if Debug
                debugCrosshair.Update(FlipbookList[0].Position);
#endif
            }
            else
            {
                //Check collision with mobiles
                foreach (Mobile mobile in LevelScene.MobileList)
                {
                    if (CanCollide && mobile.CollisionBox.CheckCollision(position))
                    {
                        Explode();
                        hasExploded = true;
#if Debug
                        debugCrosshair.Update(FlipbookList[0].Position);
#endif
                    }
                }
            }

            return hasExploded;
        }

        protected virtual void OutofboundsDestroy()
        {
            Destroy();
        }

        /// <summary>
        /// Destroys the projectile, this is called when the project explodes
        /// or leaves the playable map area.
        /// </summary>
        protected virtual void Destroy()
        {
            OnBeingDestroyedAction?.Invoke();
            mobile.UnusedProjectile.Add(this);
        }

        protected virtual int CalculateDamage(Mobile mobile)
        {
            //Normal damaging flux
            double distance = mobile.CollisionBox.GetDistance(FlipbookList[0].Position, ExplosionRadius);

            if (distance < ExplosionRadius)
            {
                distance = Helper.EuclideanDistance(Position, mobile.CollisionBox.Center);
                //Calculate the damage
                //                           100     -  500
                //                         distance  -   x
                return (int)(BaseDamage * ExplosionRadius / distance);
            }

            return 0;
        }

        /// <summary>
        /// This method is called when the projectile collides with a collidable surface
        /// must be overwriten to perform extra actions.
        /// </summary>
        protected virtual void Explode()
        {
            OnExplodeAction?.Invoke();

            if (ExplosionRadius > 0)
            {
                int removedPixels = Topography.CreateErosion(FlipbookList[0].Position, ExplosionRadius);
                ParticleBuilder.CreateGroundCollapseParticleEffect(removedPixels / 32, FlipbookList[0].Position, FlipbookList[0].Rotation);
                PlayExplosionSFX();

                OnDestroyGroundAction?.Invoke(removedPixels / 32);
            }

            if (BaseDamage > 0)
            {
                foreach(Mobile m in LevelScene.MobileList)
                {
                    int damage = CalculateDamage(m);

                    if (damage > 0)
                    {
                        //Play the receive damage animation if is alive
                        float previousHealth = m.MobileMetadata.CurrentHealth;

                        m.ReceiveDamage(damage);

                        if (previousHealth > 0 && m.MobileMetadata.CurrentHealth <= 0)
                            m.RequestDeath();

                        //This is the right implementation, but it would bug by repeating the same floating text for
                        //all the players each turn, wich is why im just gonna print on the enemy hud
                        //LevelScene.MobileList.ForEach((y) => y.HUD.FloatingTextHandler.AddDamage(x, -damage));

                        LevelScene.HUD.FloatingTextHandler.AddDamage(m, -damage);

                        OnDealDamageAction?.Invoke(damage);
                        //MatchManager.Instance.NextPlayerTurn.HUD.FloatingTextHandler.AddDamage(x, -damage);
                    }
                }
            }

            Destroy();
        }

        /// <summary>
        /// Default rotation method. Here the rotation is calculated by the angle
        /// formed by the previous and current position
        /// </summary>
        protected virtual void UpdateRotation()
        {
            if (previousPosition == FlipbookList[0].Position) return;

            float newRotation = (float)Helper.AngleBetween(FlipbookList[0].Position, previousPosition);
            FlipbookList.ForEach((x) => x.Rotation = newRotation);
            previousPosition = FlipbookList[0].Position;
        }

        public virtual void Draw(GameTime GameTime, SpriteBatch SpriteBatch)
        {
            if (SpawnTimeCounter < SpawnTime) return;

            FlipbookList.ForEach((x) => x.Draw(GameTime, SpriteBatch));
        }
    }
}
