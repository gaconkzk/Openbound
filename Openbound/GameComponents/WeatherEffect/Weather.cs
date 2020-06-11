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
using OpenBound.GameComponents.Debug;
using OpenBound.GameComponents.Level;
using OpenBound.GameComponents.PawnAction;
using Openbound_Network_Object_Library.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBound.GameComponents.WeatherEffect
{
    public enum WeatherAnimationType
    {
        /// <summary>
        /// Uses a single frame for the whole animation
        /// </summary>
        FixedAnimationFrame,
        /// <summary>
        /// Uses the entirespritesheet for the animation
        /// </summary>
        VariableAnimationFrame,
    }

    /// <summary>
    /// Assisting type to store the time required to Force effect spawn a new particle
    /// </summary>
    public class WeatherProjectileParticleTimer
    {
        public Projectile Projectile;
        public float ParticleTimer;
    }

    public abstract class Weather
    {
        //Weather properties
        private List<Flipbook> flipbookList;

        protected Vector2 flipbookPivot;
        protected float rotation;
        protected int numberOfFrames;
        protected float _initialTransparency = 1;

        public float Scale { get; protected set; }
        public Vector2 StartingPosition { get; protected set; }

        public WeatherType WeatherType { get; private set; }

        //Effects

        /// <summary>
        /// List of modified/under behavior modification projectiles.
        /// </summary>
        public List<Projectile> ModifiedProjectileList;

        //Collision
        protected Rectangle collisionRectangle, outerCollisionRectangle;
        protected Vector2 collisionRectangleOffset, outerCollisionRectangleOffset;

        //Animation
        //Animation - VerticalScrolling
        private Vector2 animationVerticalScrollingOffset;

        //Animation - RandomFlipbookFrame
        private float animationRandomFlipbookElapsedTime;

        private float fadeAnimationElapsedTime;

#if DEBUG
        //Debug
        private DebugRectangle debugRectangle;
        private DebugRectangle outerDebugRectangle;
#endif

        /// <summary>
        /// Creates a new weather. Ideally it should only be used in <see cref="WeatherHandler.Add"/>.
        /// </summary>
        /// <param name="startingPosition">
        ///     Starting position of the weather element. The ending position is always the position of the first sprite located outside the screen.
        ///     This variable does not takes the pivot as consideration.
        /// </param>
        /// <param name="flipbookPivot">
        ///     Pivot must be the center of a sprite's frame.
        /// </param>
        /// <param name="numberOfFrames">
        ///     Number of the frames inside the sprite
        /// </param>
        /// <param name="collisionRectangleOffset">
        ///     This parameter has it's X and Y values subtracted from the original sprite width height in order to calculate the collision area.
        /// </param>
        /// <param name="outerCollisionRectangleOffset">
        ///     This parameter has it's X and Y values added to the <see cref="collisionRectangleOffset"/> to calculate the external bounds of the collision rectangle.
        ///     The merging occurs when this box collides with another outerCollisionRectangle of a similar weather. 
        /// </param>
        /// <param name="weatherType">
        ///     This variable is defined by each weather element that inherits Weather.
        /// </param>
        /// <param name="scale">
        ///     Current scale of each instance of Flipbooks.
        /// </param>
        /// <param name="rotation">
        ///     Defines the rotation of this element.
        /// </param>
        public Weather(Vector2 startingPosition, Vector2 flipbookPivot, int numberOfFrames, Vector2 collisionRectangleOffset, Vector2 outerCollisionRectangleOffset, WeatherType weatherType, float scale, float rotation = 0)
        {
            flipbookList = new List<Flipbook>();
            ModifiedProjectileList = new List<Projectile>();

            animationRandomFlipbookElapsedTime = 0;

            this.flipbookPivot = flipbookPivot;
            this.numberOfFrames = numberOfFrames;
            this.collisionRectangleOffset = collisionRectangleOffset;
            this.outerCollisionRectangleOffset = outerCollisionRectangleOffset;
            this.rotation = rotation;

            StartingPosition = startingPosition;
            WeatherType = weatherType;
            Scale = scale;
        }

        /// <summary>
        /// Instances all Flipbooks in position and set it's animations
        /// </summary>
        public virtual void Initialize(string texturePath, Vector2 startingPosition, WeatherAnimationType animationType, int extraSpawns = 0)
        {
            Vector2 endingPosition = new Vector2(Topography.MapWidth, Topography.MapHeight) / 2;

            //Creates the initial offset and 'subtracts' the pivot influence of the element.
            Vector2 currentOffset = startingPosition + Vector2.Transform(new Vector2(0, flipbookPivot.Y), Matrix.CreateRotationZ(rotation)) * Scale;

            int startingFrame = 0;
            Vector2 temporaryOffset = Vector2.Zero;

            do
            {
                currentOffset += temporaryOffset;

                //FixedAnimationsFrames is used on random
                //VariableAnimationFrame is used on tornado, weakness and force
                AnimationInstance animation = new AnimationInstance() { TimePerFrame = 1 / 15f };

                switch (animationType)
                {
                    case WeatherAnimationType.FixedAnimationFrame:
                        animation.StartingFrame = animation.EndingFrame = startingFrame % numberOfFrames;
                        break;
                    case WeatherAnimationType.VariableAnimationFrame:
                        animation.EndingFrame = numberOfFrames - 1;
                        break;
                }

                startingFrame++;

                Flipbook fb = Flipbook.CreateFlipbook(currentOffset, flipbookPivot, (int)flipbookPivot.X * 2, (int)flipbookPivot.Y * 2, texturePath, animation, true, DepthParameter.WeatherEffect, rotation);
                fb.Scale *= Scale;
                flipbookList.Add(fb);

                fb.SetCurrentFrame(startingFrame % numberOfFrames);

                if (temporaryOffset == Vector2.Zero)
                    temporaryOffset = Vector2.Transform(new Vector2(0, fb.SpriteHeight), Matrix.CreateRotationZ(rotation)) * Scale;

                //While it is inside the map boundaries and extraSpawns is not 0
            } while (Topography.IsInsideMapBoundaries(currentOffset) || extraSpawns-- > 0);

            //If the rotation isn't 0 it means there are no good reasons for collision boxes
            if (rotation == 0)
            {
                collisionRectangle = new Rectangle((int)(startingPosition.X - collisionRectangleOffset.X * Scale), (int)startingPosition.Y,
                    (int)(collisionRectangleOffset.X * 2 * Scale), (int)(endingPosition.Y - startingPosition.Y));

                outerCollisionRectangle = new Rectangle(collisionRectangle.X - (int)outerCollisionRectangleOffset.X, collisionRectangle.Y - (int)outerCollisionRectangleOffset.Y,
                    collisionRectangle.Width + (int)outerCollisionRectangleOffset.X * 2, collisionRectangle.Height + 10 * 2);

#if DEBUG
                debugRectangle = new DebugRectangle(Color.Blue);
                debugRectangle.Update(collisionRectangle);
                DebugHandler.Instance.Add(debugRectangle);
                outerDebugRectangle = new DebugRectangle(Color.Red);
                outerDebugRectangle.Update(outerCollisionRectangle);
                DebugHandler.Instance.Add(outerDebugRectangle);
#endif
            }
        }

        /// <summary>
        /// Check if this <see cref="outerCollisionRectangle"/> intersects with another weather's <see cref="outerCollisionRectangle"/>.
        /// </summary>
        public bool Intersects(Weather weather) => weather.outerCollisionRectangle.Intersects(outerCollisionRectangle);

        /// <summary>
        /// Check if this <see cref="collisionRectangle"/> intersects with a <see cref="Projectile.Position"/>.
        /// </summary>
        public bool Intersects(Projectile projectile) => collisionRectangle.Intersects(projectile.Position);

        /// <summary>
        /// Check if this Weather is interacting with a Projectile.
        /// </summary>
        public bool IsInteracting(Projectile projectile) => ModifiedProjectileList.Contains(projectile);

        /// <summary>
        /// Define what happens when a projectille interacts with the <see cref="collisionRectangle"/>
        /// </summary>
        public abstract void OnInteract(Projectile projectile);

        /// <summary>
        /// Define what happens when a projectille stop interacting with the <see cref="outerCollisionRectangle"/>
        /// </summary>
        public abstract void OnStopInteracting(Projectile projectile);

        /// <summary>
        /// Defines how a merge happen between two similar Weathers
        /// </summary>
        public abstract Weather Merge(Weather weather);

        /// <summary>
        /// Defines how to proceed with updating values.
        /// </summary>
        public abstract void Update(GameTime gameTime);

        /// <summary>
        /// Updates the entire weather animation to a random animation frame, should be used on the tornado and electricity
        /// </summary>
        public void RandomFlipbookUpdate(GameTime gameTime)
        {
            if ((animationRandomFlipbookElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds) >= Parameter.WeaterEffectRandomFlipbookUpdateTimer)
            {
                flipbookList.ForEach((x) => x.JumpToRandomAnimationFrame());
                animationRandomFlipbookElapsedTime = 0;
            }
        }

        /// <summary>
        /// Fades the weather object after the timespan is ended.
        /// </summary>
        /// <param name="timespan"></param>
        protected void Fade(GameTime gameTime, float timeLimit = 1)
        {
            //transparency -= (float)gameTime.ElapsedGameTime.TotalSeconds * timespawn;
            //timespan -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            fadeAnimationElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            float transparency = Math.Max(0, 1 - fadeAnimationElapsedTime / timeLimit);
            flipbookList.ForEach(x => x.SetTransparency(transparency));
        }

        /// <summary>
        /// Makes the weatherEffect to move down and loop on screen
        /// </summary>
        public void VerticalScrollingUpdate(GameTime gameTime)
        {
            animationVerticalScrollingOffset += new Vector2(0, Parameter.WeatherEffectVerticalScrollingUpdateSpeed) * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Update position
            if (animationVerticalScrollingOffset.Y >= 1)
            {
                Vector2 roundOffset = new Vector2((float)Math.Round(animationVerticalScrollingOffset.X), (float)Math.Round(animationVerticalScrollingOffset.Y));
                flipbookList.ForEach((x) => x.Position += roundOffset);
                animationVerticalScrollingOffset = Vector2.Zero;
            }

            //Move the flipbook to the origin
            Flipbook lE = flipbookList.Last();
            if (lE.Position.Y - lE.SourceRectangle.Height * Scale >= Topography.MapHeight / 2)
            {
                lE.Position = flipbookList[0].Position - new Vector2(0, lE.Pivot.Y * 2) * Scale;
                int newFrame = flipbookList[0].GetCurrentFrame() - 1;
                lE.SetCurrentFrame(newFrame < 0 ? (numberOfFrames - 1) : newFrame);
                flipbookList.Remove(lE);
                flipbookList.Insert(0, lE);
            }
        }

        /// <summary>
        /// Check if a projectile is able to interact with the weather.
        /// </summary>
        /// <returns>Returns <see cref="true"/> if the interaction has started, otherwise returns <see cref="false"/>.</returns>
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

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            flipbookList.ForEach((x) => x.Draw(gameTime, spriteBatch));
        }
    }
}


