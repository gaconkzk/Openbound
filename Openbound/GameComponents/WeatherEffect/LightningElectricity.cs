using Microsoft.Xna.Framework;
using OpenBound.GameComponents.Debug;
using OpenBound.GameComponents.Level;
using OpenBound.GameComponents.PawnAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBound.GameComponents.WeatherEffect
{
    public class LightningElectricity : Weather
    {
        public LightningElectricity(Vector2 position, float angle) : base(new Vector2(position.X, -Topography.MapHeight / 2), new Vector2(31, 128), 3, new Vector2(35, 0), new Vector2(10, 10), default, 1, angle)
        {
            Initialize(@"Graphics/Special Effects/Tank/Lightning/Flame1", position, WeatherAnimationType.VariableAnimationFrame);
        }

        public override void OnInteract(Projectile projectile) { }

        public override void OnStopInteracting(Projectile projectile) { }

        public override void Update(GameTime gameTime)
        {
            Fade(gameTime, 0.4f);
        }

        public override Weather Merge(Weather weather) { return this; }
    }
}
