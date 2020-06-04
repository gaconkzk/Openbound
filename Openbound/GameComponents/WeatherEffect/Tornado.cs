using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenBound.Common;
using OpenBound.Extension;
using OpenBound.GameComponents.Debug;
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

    public class TornadoProjectileState
    {
        public Vector2 SpeedVector;
        public TornadoAnimationState TornadoAnimationState;

        public TornadoProjectileState(Projectile projectile)
        {
            TornadoAnimationState = TornadoAnimationState.FirstStep;

            float speed = projectile.SpeedVector.Length();
            float initialSpeed = projectile.InitialSpeedVector.Length();

            if (speed == 0)
                speed = initialSpeed;

            SpeedVector = projectile.CurrentFlipbookAngleVector * Math.Max(speed, Parameter.WeatherEffectTornadoMinimumProjectileSpeed); 
        }
    }

    public class Tornado : Weather
    {
        Dictionary<Projectile, TornadoProjectileState> tornadoInteraction;
        List<Projectile> unusedProjectileList;

        public Tornado(Vector2 position, float scale) : base(position, new Vector2(64, 32), 8, new Vector2(35, 0), new Vector2(10, 10), WeatherEffectType.Tornado, scale)
        {
            tornadoInteraction = new Dictionary<Projectile, TornadoProjectileState>();
            unusedProjectileList = new List<Projectile>();

            Initialize("Graphics/Special Effects/Weather/Tornado", new Vector2(position.X, -Topography.MapHeight / 2), WeatherAnimationType.VariableAnimationFrame);
        }

        public override void OnInteract(Projectile projectile)
        {
            projectile.IsExternallyRefreshingPosition = true;
            projectile.OnBeginTornadoInteraction();
            tornadoInteraction.Add(projectile, new TornadoProjectileState(projectile));
        }

        public override void OnStopInteracting(Projectile projectile)
        {
            projectile.IsExternallyRefreshingPosition = false;
            projectile.SetBasePosition(projectile.Position);
            projectile.OnEndTornadoInteraction();
            ModifiedProjectileList.Remove(projectile);
            tornadoInteraction.Remove(projectile);
        }

        public override void Update(GameTime gameTime)
        {
            //VerticalScrollingUpdate(gameTime);
            UpdateProjectiles();
        }

        private void UpdateProjectiles()
        {
            foreach(KeyValuePair<Projectile, TornadoProjectileState> kvp in tornadoInteraction)
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
