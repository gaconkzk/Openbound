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
using OpenBound.GameComponents.Level;
using OpenBound.GameComponents.PawnAction;
using System;
using System.Collections.Generic;

namespace OpenBound.GameComponents.WeatherEffect
{
    public enum TornadoAnimationState
    {
        FirstStep,
        SecondStep,
        ThirdStep,
        Leaving,
    }

    /// <summary>
    /// This class sumarizes the necessary objects which are going to be used by the tornado's motion
    /// </summary>
    public class TornadoProjectileState
    {
        public Vector2 SpeedVector { get; private set; }
        public TornadoAnimationState TornadoAnimationState;

        public TornadoProjectileState(Projectile projectile)
        {
            TornadoAnimationState = TornadoAnimationState.FirstStep;

            float speed = projectile.SpeedVector.Length();
            float initialSpeed = projectile.InitialSpeedVector.Length();

            //If the speed is zero the projectile wasn't instantiated yet. In this case we should use the initialSpeed.
            if (speed == 0)
                speed = initialSpeed;

            SpeedVector = projectile.CurrentFlipbookAngleVector * Math.Max(speed, Parameter.WeatherEffectTornadoMinimumProjectileSpeed);
        }
    }

    public class Tornado : Weather
    {
        Dictionary<Projectile, TornadoProjectileState> tornadoInteraction;
        List<Projectile> unusedProjectileList;

        public Tornado(Vector2 position, float scale/*, float rotation = 0*/) : base(new Vector2(position.X, -Topography.MapHeight / 2)/*position*/, new Vector2(64, 32), 8, new Vector2(35, 0), new Vector2(10, 10), WeatherEffectType.Tornado, scale, 0/*rotation*/)
        {
            tornadoInteraction = new Dictionary<Projectile, TornadoProjectileState>();
            unusedProjectileList = new List<Projectile>();

            Initialize("Graphics/Special Effects/Weather/Tornado", position, WeatherAnimationType.VariableAnimationFrame, 2);
        }

        public override void OnInteract(Projectile projectile)
        {
            //Forces projectile to stop refreshing by its own UpdatePosition method
            projectile.IsExternallyRefreshingPosition = true;
            
            //Setup projectile variables (if necessary)
            projectile.OnBeginTornadoInteraction();

            //Add projectile into interaction dictionary
            tornadoInteraction.Add(projectile, new TornadoProjectileState(projectile));
        }

        public override void OnStopInteracting(Projectile projectile)
        {
            //Allows projectile to keep refreshing with its own UpdatePosition method
            projectile.IsExternallyRefreshingPosition = false;

            //Reset physics variables to maintain its consistency after the tornado
            projectile.SetBasePosition(projectile.Position);

            //Setup after-tornado behavior (if necessary)
            projectile.OnEndTornadoInteraction();

            //Makes it stop being manipulated by the tornado, allowing it to interact again if possible
            ModifiedProjectileList.Remove(projectile);
            tornadoInteraction.Remove(projectile);
        }

        public override void Update(GameTime gameTime)
        {
            VerticalScrollingUpdate(gameTime);
            UpdateProjectiles();
        }

        private void UpdateProjectiles()
        {
            foreach (KeyValuePair<Projectile, TornadoProjectileState> kvp in tornadoInteraction)
            {
                if (!kvp.Key.IsExternallyRefreshingPosition || !kvp.Key.IsAbleToRefreshPosition) continue;

                switch (kvp.Value.TornadoAnimationState)
                {
                    case TornadoAnimationState.FirstStep:
                        //Interpolate Movement for collision
                        for (float elapsedTime = 0; elapsedTime < Parameter.ProjectileMovementTotalTimeElapsed; elapsedTime += Parameter.ProjectileMovementTimeElapsedPerInteraction)
                        {
                            Vector2 nextPosition = kvp.Key.Position + kvp.Value.SpeedVector * Parameter.ProjectileMovementTimeElapsedPerInteraction;

                            if (!collisionRectangle.Intersects(nextPosition))
                            {
                                kvp.Value.TornadoAnimationState = TornadoAnimationState.SecondStep;
                                kvp.Key.FlipbookList[0].LayerDepth = DepthParameter.WeatherEffectTornadoProjectile;
                                break;
                            }
                            else
                            {
                                kvp.Key.Position = nextPosition;
                            }
                        }
                        break;
                    case TornadoAnimationState.SecondStep:
                        for (float elapsedTime = 0; elapsedTime < Parameter.ProjectileMovementTotalTimeElapsed; elapsedTime += Parameter.ProjectileMovementTimeElapsedPerInteraction)
                        {
                            Vector2 nextPosition = kvp.Key.Position + kvp.Value.SpeedVector * Parameter.ProjectileMovementTimeElapsedPerInteraction * new Vector2(-1, 1);

                            if (!collisionRectangle.Intersects(nextPosition))
                            {
                                kvp.Value.TornadoAnimationState = TornadoAnimationState.ThirdStep;
                                kvp.Key.FlipbookList[0].LayerDepth = DepthParameter.Projectile;
                                break;
                            }
                            else
                            {
                                kvp.Key.Position = nextPosition;
                            }
                        }
                        break;
                    case TornadoAnimationState.ThirdStep:
                        //Interpolate Movement for collision
                        for (float elapsedTime = 0; elapsedTime < Parameter.ProjectileMovementTotalTimeElapsed; elapsedTime += Parameter.ProjectileMovementTimeElapsedPerInteraction)
                        {
                            kvp.Key.Position = kvp.Key.Position + kvp.Value.SpeedVector * Parameter.ProjectileMovementTimeElapsedPerInteraction;

                            if (!outerCollisionRectangle.Intersects(kvp.Key.Position))
                            {
                                kvp.Value.TornadoAnimationState = TornadoAnimationState.Leaving;
                                break;
                            }
                        }
                        break;
                    case TornadoAnimationState.Leaving:
                        unusedProjectileList.Add(kvp.Key);
                        break;
                }
            }

            unusedProjectileList.ForEach((x) => OnStopInteracting(x));
            unusedProjectileList.Clear();
        }

        public override Weather Merge(Weather weather)
        {
            return new Tornado((StartingPosition + weather.StartingPosition) / 2, Scale + weather.Scale);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
        }
    }
}
