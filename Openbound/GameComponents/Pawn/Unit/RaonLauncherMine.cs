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

            Movement.CollisionOffset = 10;
            Movement.MaximumStepsPerTurn = 90;

            //Unstuck mine
            //Movement.MoveSideways(Facing.Left);
            //Movement.MoveSideways(Facing.Right);

            CollisionBox = new CollisionBox(this, new Rectangle(0, 0, 12, 12), new Vector2(0, 0));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            UpdateProximity();
            UpdateOutOfBounds();
        }


        //Necessary overrides to reuse mines as a mobile
        public override void UpdateSyncMobileToServer() { return; }

        public void UpdateOutOfBounds()
        {
            if (Topography.IsNotInsideMapBoundaries(Position))
                LevelScene.ToBeRemovedMineList.Add(this);
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
                p.Position = Position;
                p.UpdateCollider(Position);
                LevelScene.ToBeRemovedMineList.Add(this);
            }

            if (minDist < 1000 && MobileFlipbook.State == ActorFlipbookState.Stand)
            {
                MobileFlipbook.ChangeState(ActorFlipbookState.ChargingS1, true);
            }
            else if (MobileFlipbook.State == ActorFlipbookState.ChargingS1)
            {
                MobileFlipbook.ChangeState(ActorFlipbookState.Stand, true);
            }
        }

        public override void ReceiveDamage(int damage)
        {
            throw new NotImplementedException();
        }

        public override void ReceiveShock(int damage)
        {
            throw new NotImplementedException();
        }
    }
}
