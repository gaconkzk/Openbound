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
    public class EletricityStrike : Weather
    {

        public EletricityStrike(Vector2 position, float scale) : base(new Vector2(position.X, -Topography.MapHeight / 2)/*position*/, new Vector2(31, 128), 3, new Vector2(35, 0), new Vector2(10, 10), Openbound_Network_Object_Library.Entity.WeatherType.LightningSES1, scale, MathHelper.Pi/*rotation*/)
        {
            Initialize(@"Graphics/Special Effects/Tank/Lightning/Flame1", position, WeatherAnimationType.VariableAnimationFrame);
        }

        public override void OnInteract(Projectile projectile)
        {
            //throw new NotImplementedException();
        }

        public override void OnStopInteracting(Projectile projectile)
        {
            //throw new NotImplementedException();
        }

        public override void Update(GameTime gameTime)
        {
            this.Fade(gameTime, 0.4f);
        }

        public override Weather Merge(Weather weather)
        {
            return this;
        }
    }
}
