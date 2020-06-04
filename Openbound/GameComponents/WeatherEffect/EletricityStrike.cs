using Microsoft.Xna.Framework;
using OpenBound.GameComponents.Debug;
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
        private Rectangle outerCollisionRectangle;

        private DebugRectangle outerDebugRectangle;

        public EletricityStrike(Vector2 startingPosition)
        {
            //startingPosition.X = 0;
            _startingPosition = new Vector2(startingPosition.X, -500);
            Initialize(@"Graphics/Special Effects/Tank/Lightning/Flame1", _startingPosition, WeatherAnimationType.VariableAnimationFrame);
        }

        public override void Initialize(string texturePath, Vector2 startingPosition, WeatherAnimationType animationType)
        {
            base.Initialize(texturePath, startingPosition, animationType);

            outerCollisionRectangle = new Rectangle(collisionRectangle.X - 10, collisionRectangle.Y - 10, collisionRectangle.Width + 10 * 2, collisionRectangle.Height + 10 * 2);
            outerDebugRectangle = new DebugRectangle(Color.Red);
            outerDebugRectangle.Update(outerCollisionRectangle);
            DebugHandler.Instance.Add(outerDebugRectangle);
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
            debugRectangle.Update(collisionRectangle);
            outerCollisionRectangle = new Rectangle(collisionRectangle.X - 10, collisionRectangle.Y - 10, collisionRectangle.Width + 10 * 2, collisionRectangle.Height + 10 * 2);
            outerDebugRectangle.Update(outerCollisionRectangle);


        }
    }
}
