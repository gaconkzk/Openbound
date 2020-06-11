﻿using Microsoft.Xna.Framework;
using OpenBound.Common;
using OpenBound.GameComponents.Level;
using OpenBound.GameComponents.Level.Scene;
using OpenBound.GameComponents.PawnAction;

namespace OpenBound.GameComponents.WeatherEffect
{
    public class LightningElectricity : Weather
    {
        float elapsedTime;
        public LightningElectricity(Vector2 position, float angle) : base(new Vector2(position.X, -Topography.MapHeight / 2), new Vector2(31, 128), 3, default, default, default, 1, angle)
        {
            elapsedTime = 0;

            Initialize(@"Graphics/Special Effects/Tank/Lightning/Flame1", position, WeatherAnimationType.VariableAnimationFrame);
        }

        public override void OnInteract(Projectile projectile) { }

        public override void OnStopInteracting(Projectile projectile) { }

        public override void Update(GameTime gameTime)
        {
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (elapsedTime > Parameter.ProjectileLightningElectricityFadeTime)
                LevelScene.WeatherHandler.RemoveWeather(this);
            else
                FadeOut(gameTime, Parameter.ProjectileLightningElectricityFadeTime);
        }

        public override Weather Merge(Weather weather) { return this; }
    }
}
