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
        public float Scale { get; protected set; }
        public Vector2 StartingPosition { get; protected set; }
        
        public List<Projectile> ModifiedProjectileList;
        protected Rectangle collisionRectangle, outerCollisionRectangle;

        protected Vector2 flipbookPivot;
        protected int numberOfFrames;
        protected Vector2 collisionRectangleOffset, outerCollisionRectangleOffset;

        private DebugRectangle debugRectangle;
        private DebugRectangle outerDebugRectangle;

        public WeatherEffectType WeatherEffectType { get; private set; }

        public Weather(Vector2 startingPosition, Vector2 flipbookPivot, int numberOfFrames, Vector2 collisionRectangleOffset, Vector2 outerCollisionRectangleOffset, WeatherEffectType weatherEffectType, float scale)
        {
            flipbookList = new List<Flipbook>();
            ModifiedProjectileList = new List<Projectile>();
            randomRotationElapsedTime = 0;

            StartingPosition = startingPosition;
            this.flipbookPivot = flipbookPivot;
            this.numberOfFrames = numberOfFrames;
            this.collisionRectangleOffset = collisionRectangleOffset;
            this.outerCollisionRectangleOffset = outerCollisionRectangleOffset;
            WeatherEffectType = weatherEffectType;
            this.Scale = scale;
        }

        public virtual void Initialize(string texturePath, Vector2 startingPosition, WeatherAnimationType animationType)
        {
            Vector2 endingPosition = new Vector2(Topography.MapWidth, Topography.MapHeight) / 2;
            Vector2 currentOffset = startingPosition;

            int startingFrame = 0;

            while (currentOffset.Y - flipbookPivot.X * 2 * Scale <= endingPosition.Y)
            {
                //FixedAnimationsFrames is used on mirror
                //VariableAnimationFrame is used on tornado, weakness and force
                AnimationInstance animation = new AnimationInstance() { TimePerFrame = 1/15f };

                switch (animationType)
                {
                    case WeatherAnimationType.FixedAnimationFrame:
                        animation.StartingFrame = animation.EndingFrame = startingFrame % numberOfFrames;
                        break;
                    case WeatherAnimationType.VariableAnimationFrame:
                        animation.EndingFrame = (numberOfFrames - 1);
                        break;
                }

                startingFrame++;

                Flipbook fb = Flipbook.CreateFlipbook(currentOffset, flipbookPivot, (int)flipbookPivot.X * 2, (int)flipbookPivot.Y * 2, texturePath, animation, true, DepthParameter.WeatherEffect);
                fb.Scale *= Scale;
                flipbookList.Add(fb);

                fb.SetCurrentFrame(startingFrame % numberOfFrames);

                currentOffset += Vector2.Transform(new Vector2(0, fb.SpriteHeight), Matrix.CreateRotationZ(flipbookList[0].Rotation)) * Scale;
            }

            collisionRectangle = new Rectangle((int)(startingPosition.X - collisionRectangleOffset.X * Scale), (int)startingPosition.Y,
                (int)(collisionRectangleOffset.X * 2 * Scale), (int)((endingPosition.Y - startingPosition.Y)));

            debugRectangle = new DebugRectangle(Color.Blue);
            debugRectangle.Update(collisionRectangle);
            DebugHandler.Instance.Add(debugRectangle);

            outerCollisionRectangle = new Rectangle(collisionRectangle.X - (int)outerCollisionRectangleOffset.X, collisionRectangle.Y - (int)outerCollisionRectangleOffset.Y,
                collisionRectangle.Width + (int)outerCollisionRectangleOffset.X * 2, collisionRectangle.Height + 10 * 2);

            outerDebugRectangle = new DebugRectangle(Color.Red);
            outerDebugRectangle.Update(outerCollisionRectangle);
            DebugHandler.Instance.Add(outerDebugRectangle);
        }

        public abstract Weather Merge(Weather weather);

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
            if (lE.Position.Y - lE.SourceRectangle.Height * Scale >= Topography.MapHeight / 2)
            {
                lE.Position = flipbookList[0].Position - new Vector2(0, lE.Pivot.Y * 2) * 2;
                int newFrame = flipbookList[0].GetCurrentFrame() - 1;
                lE.SetCurrentFrame(newFrame < 0 ? (numberOfFrames - 1) : newFrame);
                flipbookList.Remove(lE);
                flipbookList.Insert(0, lE);
            }
        }

        public bool CheckProjectileInteraction(Projectile projectile)
        {
            if (collisionRectangle.Intersects(projectile.Position) && !ModifiedProjectileList.Contains(projectile))
            {
                ModifiedProjectileList.Add(projectile);
                OnInteract(projectile);
                return true;
            }

            return false;
        }


        public bool Intersects(Weather weather) => weather.outerCollisionRectangle.Intersects(outerCollisionRectangle);
        public bool Intersects(Projectile projectile) => collisionRectangle.Intersects(projectile.Position);

        public bool IsInteracting(Projectile projectile) => ModifiedProjectileList.Contains(projectile);

        public abstract void OnInteract(Projectile projectile);
        public abstract void OnStopInteracting(Projectile projectile);

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            flipbookList.ForEach((x) => x.Draw(gameTime, spriteBatch));
        }
    }
}


