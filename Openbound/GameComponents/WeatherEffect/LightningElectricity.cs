using Microsoft.Xna.Framework;
using OpenBound.Common;
using OpenBound.GameComponents.Level;
using OpenBound.GameComponents.PawnAction;

namespace OpenBound.GameComponents.WeatherEffect
{
    public class LightningElectricity : Weather
    {
        public LightningElectricity(Vector2 position, float angle) : base(new Vector2(position.X, -Topography.MapHeight / 2), new Vector2(31, 31), 3, default, default, default, 1, angle)
        {
            Initialize(@"Graphics/Special Effects/Tank/Lightning/Flame1", position, WeatherAnimationType.VariableAnimationFrame);
        }

        public override void OnInteract(Projectile projectile) { }

        public override void OnStopInteracting(Projectile projectile) { }

        public override void Update(GameTime gameTime)
        {
            Fade(gameTime, Parameter.ProjectileLightningElectricityFadeTime);
        }

        public override Weather Merge(Weather weather) { return this; }
    }
}
