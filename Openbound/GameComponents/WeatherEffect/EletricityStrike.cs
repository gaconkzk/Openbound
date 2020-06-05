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
        private Vector2 _startingPosition;

        public EletricityStrike(Vector2 position, float scale) : base(new Vector2(position.X, -Topography.MapHeight / 2)/*position*/, new Vector2(31, 128), 3, new Vector2(35, 0), new Vector2(10, 10), WeatherEffectType.LightningSES1, scale, MathHelper.Pi/*rotation*/)
        {
            //startingPosition.X = 0;
            //_startingPosition = new Vector2(startingPosition.X, -500);
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
            //VerticalScrollingUpdate(gameTime);
            //this.outerCollisionRectangle.Y += 1;
            _startingPosition.Y += 5; //(float)gameTime.ElapsedGameTime.TotalMilliseconds;
            collisionRectangle = new Rectangle((int)_startingPosition.X - 35, (int)_startingPosition.Y, (35 * 2), (int)(_startingPosition.Y - _startingPosition.Y));
            //debugRectangle.Update(collisionRectangle);
            outerCollisionRectangle = new Rectangle(collisionRectangle.X - 10, collisionRectangle.Y - 10, collisionRectangle.Width + 10 * 2, collisionRectangle.Height + 10 * 2);
            // outerDebugRectangle.Update(outerCollisionRectangle);
          
            this.Fade(gameTime, 0.4f);
        }

        public override Weather Merge(Weather weather)
        {
            return this;
        }
    }
}
