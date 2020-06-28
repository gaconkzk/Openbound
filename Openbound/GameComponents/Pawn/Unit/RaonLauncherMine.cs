using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenBound.Common;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Level.Scene;
using OpenBound.GameComponents.Pawn.UnitProjectiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBound.GameComponents.Pawn.Unit
{
    public enum MineState
    {
        Dormant,
        Active,
        Moving,
    }

    public class RaonLauncherMine : Actor
    {
        protected static Dictionary<MineState, AnimationInstance> mineAnimationPresets = new Dictionary<MineState, AnimationInstance>()
        {
            { MineState.Dormant, new AnimationInstance(){ StartingFrame = 00, EndingFrame = 19, TimePerFrame = 1/20f, AnimationType = AnimationType.Foward } },
            { MineState.Active,  new AnimationInstance(){ StartingFrame = 20, EndingFrame = 36, TimePerFrame = 1/20f, AnimationType = AnimationType.Foward } },
            { MineState.Moving,  new AnimationInstance(){ StartingFrame = 37, EndingFrame = 48, TimePerFrame = 1/20f, AnimationType = AnimationType.Foward } },
        };

        readonly Mobile mobile;
        readonly Flipbook flipbook;

        Mobile target;

        MineState mineState;

        public new virtual Vector2 Position => flipbook.Position;

        public RaonLauncherMine(Mobile mobile, Vector2 position) : base()
        {
            Owner = mobile.Owner;
            this.mobile = mobile;

            mineState = MineState.Dormant;

            LayerDepth = DepthParameter.Mobile;
            
            flipbook = Flipbook.CreateFlipbook(position -  5 * Vector2.UnitY, new Vector2(16, 25), 31, 29,
                "Graphics/Tank/RaonLauncher/MineS2", mineAnimationPresets[mineState],
                false, DepthParameter.Mobile);
        }

        public override void Update(GameTime gameTime)
        {
            UpdateProximity();
        }

        //public void Update

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

            if (target.CollisionBox.CheckCollision(Position))
            {
                RaonProjectile2Explosion p = new RaonProjectile2Explosion((RaonLauncher)mobile);
                p.Position = Position;
                p.UpdateCollider(Position);
                LevelScene.ToBeRemovedMineList.Add(this);
            }

            if (minDist < 1000 && mineState == MineState.Dormant)
            {
                flipbook.AppendAnimationIntoCycle(mineAnimationPresets[MineState.Active], true);
            }
            else if (mineState == MineState.Active)
            {
                flipbook.AppendAnimationIntoCycle(mineAnimationPresets[MineState.Dormant], true);
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            flipbook.Draw(gameTime, spriteBatch);
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
