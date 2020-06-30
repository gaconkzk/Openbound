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
        //Necessary overrides to reuse mines as a mobile


        readonly Mobile mobile;

        Mobile target;

        public RaonLauncherMine(Mobile mobile, Vector2 position) : base(mobile.Owner, MobileType.RaonLauncherMine, true)
        {
            this.mobile = mobile;

            Position = position;

            MobileFlipbook = MobileFlipbook.CreateMobileFlipbook(MobileType.RaonLauncherMine, position);
            MobileFlipbook.ChangeState(ActorFlipbookState.Dormant, true);

            Movement.CollisionOffset = 10;
            Movement.MaximumStepsPerTurn = 90;

            CollisionBox = new CollisionBox(this, new Rectangle(0, 0, 12, 12), new Vector2(0, 0));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            UpdateProximity();
            UpdateOutOfBounds();
        }

        //Necessary overrides to reuse mines as a mobile
        public override void PlayMovementSE(float pitch = 0, float pan = 0)
        {
            AudioHandler.PlayUniqueSoundEffect(movingSE, () => Movement.IsAbleToMove, pitch: pitch, pan: pan);
        }

        public override void UpdateSyncMobileToServer() { return; }

        public void UpdateOutOfBounds()
        {
            if (Topography.IsNotInsideMapBoundaries(Position))
                Die();
        }

        private bool IsStateDormant(ActorFlipbookState state) => state == ActorFlipbookState.Dormant;
        private bool IsStateActivated(ActorFlipbookState state) => state == ActorFlipbookState.Activated;

        public override void ChangeFlipbookState(ActorFlipbookState NewState, bool Force = false)
        {
            if (NewState == MobileFlipbook.State || NewState == ActorFlipbookState.Stand || NewState == ActorFlipbookState.Falling) return;
            /*
            if (
                (IsStateDormant(MobileFlipbook.State) && IsStateActivated(NewState)) ||
                (IsStateDormant(MobileFlipbook.State) && IsStateActivated(NewState)) ||
                (IsStateActivated(MobileFlipbook.State) && IsStateActivated(NewState)) ||
                Force
                )
            {*/
            MobileFlipbook.ChangeState(NewState, Force);
            //}
        }

        public override void GrantTurn()
        {
            Movement.RemainingStepsThisTurn = Movement.MaximumStepsPerTurn;
            Movement.IsAbleToMove = true;
        }

        public void UpdateProximity()
        {
            Console.WriteLine(Position + " " + MobileFlipbook.Position);
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
                p.Position = Position;
                p.UpdateCollider(Position);
                Die();
            }

            if (minDist < 8100)
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

            LevelScene.ToBeRemovedMineList.Add(this);
        }
    }
}
