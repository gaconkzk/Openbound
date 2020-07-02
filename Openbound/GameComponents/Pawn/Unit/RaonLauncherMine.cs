using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenBound.Common;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Level.Scene;
using OpenBound.GameComponents.Pawn.UnitProjectiles;
using Openbound_Network_Object_Library.Models;
using System;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Entity.Sync;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenBound.GameComponents.Collision;
using OpenBound.GameComponents.Level;
using OpenBound.GameComponents.Audio;

namespace OpenBound.GameComponents.Pawn.Unit
{
    public class RaonLauncherMine : Mobile
    {
        readonly Mobile mobile;

        Mobile target;
        float freezeTime;

        private List<RaonLauncherMine> extraMineList;

        public RaonLauncherMine(Mobile mobile, Vector2 position) : base(mobile.Owner, MobileType.RaonLauncherMine, true)
        {
            this.mobile = mobile;

            Position = position;

            MobileFlipbook = MobileFlipbook.CreateMobileFlipbook(MobileType.RaonLauncherMine, position);
            MobileFlipbook.ChangeState(ActorFlipbookState.Dormant, true);

            Movement.CollisionOffset = 10;
            Movement.MaximumStepsPerTurn = Parameter.ProjectileRaonLauncherS2MineMaximumStepsPerTurn;

            CollisionBox = new CollisionBox(this, new Rectangle(0, 0, 8, 8), new Vector2(0, 5));
        }

        //Necessary overrides to reuse mines as a mobile
        public override void Update(GameTime gameTime)
        {
            if (Movement.IsAbleToMove && freezeTime <= 0.5f)
            {
                freezeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                return;
            }

            base.Update(gameTime);
            UpdateProximity();
            UpdateOutOfBounds();
        }

        public void GrantTurn(List<RaonLauncherMine> extraMineList)
        {
            this.extraMineList = extraMineList;
            GrantTurn();
        }

        public override void GrantTurn()
        {
            //Force targetting re-checking
            UpdateProximity();

            if (target == null) {
                LoseTurn();
                return;
            }

            freezeTime = 0;
            Movement.RemainingStepsThisTurn = Movement.MaximumStepsPerTurn;
            Movement.IsAbleToMove = true;

            GameScene.Camera.TrackObject(this);
        }

        public override void LoseTurn()
        {
            //If it has died before its turn has reached
            if (extraMineList == null) return;

            extraMineList.Remove(this);

            if (extraMineList.Count > 0)
                extraMineList[0].GrantTurn(extraMineList);
            else
                mobile.GrantTurn();

            extraMineList = null;
        }

        public override void PlayMovementSE(float pitch = 0, float pan = 0)
        {
            AudioHandler.PlayUniqueSoundEffect(movingSE, () => Movement.IsAbleToMove && !Movement.IsFalling, pitch: pitch, pan: pan);
        }

        public override void UpdateSyncMobileToServer() { return; }

        public void UpdateOutOfBounds()
        {
            if (Topography.IsNotInsideMapBoundaries(Position)) Die();
        }

        private bool IsStateDormant(ActorFlipbookState state) => state == ActorFlipbookState.Dormant;
        private bool IsStateActivated(ActorFlipbookState state) => state == ActorFlipbookState.Activated;

        public override void ChangeFlipbookState(ActorFlipbookState NewState, bool Force = false)
        {
            if (NewState == MobileFlipbook.State || NewState == ActorFlipbookState.Stand || NewState == ActorFlipbookState.Falling) return;
            MobileFlipbook.ChangeState(NewState, Force);
        }

        public void UpdateProximity()
        {
            float minDist = float.MaxValue;

            foreach(Mobile m in LevelScene.MobileList)
            {
                float dist = (float)Helper.SquaredEuclideanDistance(m.Position, Position);

                if (dist < minDist)
                {
                    minDist = dist;
                    target = m;
                }
            }

            if (target.CollisionBox.CheckCollision(CollisionBox.Center))
            {
                RaonProjectile2Explosion p = new RaonProjectile2Explosion((RaonLauncher)mobile);
                p.Position = CollisionBox.Center;
                p.Explode();
                Die();
            }

            if (minDist < Parameter.ProjectileRaonLauncherS2MineSquaredProximityRange)
            {
                if (Position.X - target.Position.X < 0 && Facing == Facing.Left)
                    Flip();
                else if (Position.X - target.Position.X > 0 && Facing == Facing.Right)
                    Flip();

                if (IsStateDormant(MobileFlipbook.State))
                {
                    ChangeFlipbookState(ActorFlipbookState.Activated, true);
                }
            }
            else if (IsStateActivated(ActorFlipbookState.Activated))
            {
                target = null;
                ChangeFlipbookState(ActorFlipbookState.Dormant, true);
            }
        }


        public override void ReceiveDamage(int damage) => Die();

        public override void ReceiveShock(int damage) => Die();

        public override void Die()
        {
            IsAlive = false;

            //Change the state to force the SE thread to stop
            Movement.IsAbleToMove = false;

            //Grants turn to the next mine or mobile
            LevelScene.ToBeRemovedMineList.Add(this);

            LoseTurn();
        }
    }
}
