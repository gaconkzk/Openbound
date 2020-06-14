using Microsoft.Xna.Framework;
using OpenBound.GameComponents.PawnAction;
using Openbound_Network_Object_Library.Entity;
using OpenBound.GameComponents.Pawn;
using OpenBound.GameComponents.Level.Scene;

namespace OpenBound.GameComponents.WeatherEffect
{
    public class Thor : Weather
    {
        public Thor(Vector2 position) : base(default, default, 0, default, default, WeatherType.Thor, 0)
        {
            LevelScene.ThorSatellite.SetPosition(position);

            //Initialize();
            SetTransparency(0);
        }

        public override bool CheckProjectileInteraction(Projectile projectile) {
            if (ModifiedProjectileList.Contains(projectile)) return false;

            ModifiedProjectileList.Add(projectile);
            OnInteract(projectile);

            return true;
        }

        public override bool Intersects(Weather weather) { return true; }

        public override Weather Merge(Weather weather) { return this; }

        public override void OnInteract(Projectile projectile) { LevelScene.ThorSatellite.Attatch(projectile); }

        public override void OnStopInteracting(Projectile projectile) { }

        public override void Update(GameTime gameTime) { }
    }
}
