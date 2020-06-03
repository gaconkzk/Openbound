using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenBound.Common;
using OpenBound.Extension;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Collision;
using OpenBound.GameComponents.Debug;
using OpenBound.GameComponents.Level;
using OpenBound.GameComponents.PawnAction;
using Openbound_Network_Object_Library.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBound.GameComponents.WeatherEffect
{
    public enum WeatherAnimationType
    {
        FixedAnimationFrame,
        VariableAnimationFrame,
    }

    public abstract class Weather
    {
        private List<Flipbook> flipbookList;
        private float randomRotationElapsedTime;
        private Vector2 offset;
        
        protected List<Projectile> modifiedProjectileList;
        protected Rectangle collisionRectangle;

        private DebugRectangle debugRectangle;

        public Weather()
        {
            flipbookList = new List<Flipbook>();
            modifiedProjectileList = new List<Projectile>();
            randomRotationElapsedTime = 0;
        }

        public void Initialize(string texturePath, Vector2 startingPosition, WeatherAnimationType animationType)
        {
            Vector2 endingPosition = new Vector2(Topography.MapWidth, Topography.MapHeight) / 2;
            Vector2 currentOffset = startingPosition;

            int startingFrame = 0;

            while (currentOffset.Y - 64 * 2 <= endingPosition.Y)
            {
                //FixedAnimationsFrames is used on mirror
                //VariableAnimationFrame is used on tornado, weakness and force
                AnimationInstance animation = new AnimationInstance() { TimePerFrame = 1/15f };

                switch (animationType)
                {
                    case WeatherAnimationType.FixedAnimationFrame:
                        animation.StartingFrame = animation.EndingFrame = startingFrame % 8;
                        break;
                    case WeatherAnimationType.VariableAnimationFrame:
                        animation.EndingFrame = 7;
                        break;
                }

                startingFrame++;

                Flipbook fb = Flipbook.CreateFlipbook(currentOffset, new Vector2(64, 32),
                    128, 64, texturePath, animation, true, DepthParameter.WeatherEffect);
                flipbookList.Add(fb);

                fb.SetCurrentFrame(startingFrame % 8);

                currentOffset += Vector2.Transform(new Vector2(0, fb.SpriteHeight), Matrix.CreateRotationZ(flipbookList[0].Rotation));
            }

            collisionRectangle = new Rectangle((int)startingPosition.X - 35, (int)startingPosition.Y, (35 * 2), (int)(endingPosition.Y - startingPosition.Y));
            debugRectangle = new DebugRectangle(Color.Blue);
            debugRectangle.Update(collisionRectangle);
            DebugHandler.Instance.Add(debugRectangle);
        }

        public abstract void Update(GameTime gameTime);

        /// <summary>
        /// Updates the entire weather animation to a random animation frame, should be used on the tornado and electricity
        /// </summary>
        /// <param name="gameTime"></param>
        public void RandomFlipbookUpdate(GameTime gameTime)
        {
            if ((randomRotationElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds) >= Parameter.WeaterEffectRandomFlipbookUpdateTimer)
            {
                flipbookList.ForEach((x) => x.JumpToRandomAnimationFrame());
                randomRotationElapsedTime = 0;
            }
        }

        /// <summary>
        /// Makes the weatherEffect to move down and loop on screen
        /// </summary>
        /// <param name="gameTime"></param>
        public void VerticalScrollingUpdate(GameTime gameTime)
        {
            offset += new Vector2(0, Parameter.WeatherEffectVerticalScrollingUpdateSpeed) * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Update position
            if (offset.Y >= 1)
            {
                Vector2 roundOffset = new Vector2((float)Math.Round(offset.X), (float)Math.Round(offset.Y));
                flipbookList.ForEach((x) => x.Position += roundOffset);
                offset = Vector2.Zero;
            }

            //Spawn new flipbooks if necessary
            Flipbook lE = flipbookList.Last();
            if (lE.Position.Y - lE.SourceRectangle.Height >= Topography.MapHeight / 2)
            {
                lE.Position = flipbookList[0].Position - new Vector2(0, lE.Pivot.Y * 2);
                int newFrame = flipbookList[0].GetCurrentFrame() - 1;
                lE.SetCurrentFrame(newFrame < 0 ? 7 : newFrame);
                flipbookList.Remove(lE);
                flipbookList.Insert(0, lE);
            }
        }

        public void CheckProjectileInteraction(Projectile projectile)
        {
            if (collisionRectangle.Intersects(projectile.Position) && !modifiedProjectileList.Contains(projectile))
            {
                modifiedProjectileList.Add(projectile);
                OnInteract(projectile);
            }
        }

        public bool IsInteracting(Projectile projectile) => modifiedProjectileList.Contains(projectile);

        public abstract void OnInteract(Projectile projectile);
        public abstract void OnStopInteracting(Projectile projectile);

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            flipbookList.ForEach((x) => x.Draw(gameTime, spriteBatch));
        }
    }
}


