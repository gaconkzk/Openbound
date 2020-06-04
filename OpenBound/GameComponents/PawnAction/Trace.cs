using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenBound.Common;
using OpenBound.Extension;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Debug;
using OpenBound.GameComponents.Level;
using OpenBound.GameComponents.Level.Scene;
using Openbound_Network_Object_Library.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBound.GameComponents.PawnAction
{
    public class Trace
    {
        private Projectile projectile;

        protected List<Flipbook> traceList;
        protected Flipbook leadTrace;

        protected Vector2 positionOffset, positionRotatedOffset, lastSpawn;
        public Color Color;

        //
        protected float rotationAngle;

#if DEBUG
        protected DebugCrosshair dc0, dc1;
#endif

        public Vector2 Position
        {
            get => leadTrace.Position;
            set => leadTrace.Position = value;
        }
            

        public Trace(MobileType mobileType, Color color, Projectile projectile)
        {
            this.projectile = projectile;
            traceList = new List<Flipbook>();
            Color = color;

#if DEBUG
            dc0 = new DebugCrosshair(Color.Yellow);
            dc1 = new DebugCrosshair(Color.Blue);

            DebugHandler.Instance.Add(dc0);
            DebugHandler.Instance.Add(dc1);
#endif
        }


        public Flipbook SpawnFlipbook(MobileType mobileType, ShotType shotType, AnimationInstance animationInstance = null)
        {
            if (animationInstance == null)
                animationInstance = new AnimationInstance() { StartingFrame = 0, EndingFrame = 72, TimePerFrame = 1 / 72f, AnimationType = AnimationType.FowardStop };

            switch (mobileType)
            {
                case MobileType.Mage:
                    return Flipbook.CreateFlipbook(Vector2.Zero, new Vector2(8, 8), 16, 16, $"Graphics/Tank/Mage/{shotType}T", animationInstance, false, projectile.FlipbookList[0].LayerDepth);
                case MobileType.Turtle:
                    return Flipbook.CreateFlipbook(Vector2.Zero, new Vector2(20, 8), 32, 16, $"Graphics/Tank/Turtle/S1T", animationInstance, false, projectile.FlipbookList[0].LayerDepth);
            }

            return null;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            traceList.ForEach((x) => x.Draw(gameTime, spriteBatch));
        }
    }
}
