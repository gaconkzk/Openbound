using Microsoft.Xna.Framework;
using OpenBound.Common;
using OpenBound.GameComponents.Debug;
using OpenBound.GameComponents.Level;
using OpenBound.GameComponents.Level.Scene;
using OpenBound.GameComponents.PawnAction;
using OpenBound.GameComponents.WeatherEffect;
using Openbound_Network_Object_Library.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBound.GameComponents.Pawn.UnitProjectiles
{
    public class ElectricityProjectile : DummyProjectile
    {
        HashSet<Mobile> mobileList;
        int extraExplosionRadius;
        int extraDamage;
        float lightningAngle;

        Vector2 parentPosition;
        Vector2 positionOffset;

        public ElectricityProjectile(Mobile mobile, Vector2 parentPosition, float lightningAngle, int explosionRadius, int extraExplosionRadius, int baseDamage, int extraDamage)
            : base(mobile, ShotType.Dummy, explosionRadius, baseDamage)
        {
            this.extraExplosionRadius = extraExplosionRadius;
            this.extraDamage = extraDamage;
            this.lightningAngle = lightningAngle;
            this.parentPosition = parentPosition;

            positionOffset = Vector2.Transform(Vector2.UnitX, Matrix.CreateRotationZ(-lightningAngle));

            //copy mobile list
            mobileList = new HashSet<Mobile>();
        }

        public override void Update()
        {
            Vector2 explosionPosition = parentPosition;

            while (Topography.IsInsideMapBoundaries(Position))
            {
                CheckAffectedMobiles();

                if (Topography.CheckCollision(Position) || LevelScene.MobileList.Any((m) => m.CollisionBox.CheckCollision(Position)))
                {
                    mobileList.Clear();
                    explosionPosition = Position;
                }

                Position += positionOffset;
            }

            Position = explosionPosition;
            Explode();
        }

        public void CheckAffectedMobiles()
        {
            foreach (Mobile m in LevelScene.MobileList)
            {
                double distance = m.CollisionBox.GetDistance(FlipbookList[0].Position, ExplosionRadius);

                if (distance < extraExplosionRadius)
                {
                    if (!mobileList.Contains(m))
                    {
                        mobileList.Add(m);
                    }
                }
            }
        }

        protected override void Explode()
        {
            base.Explode();

            LevelScene.WeatherHandler.Add(new LightningElectricity(Position, MathHelper.PiOver2 * 3 - lightningAngle));

            foreach (Mobile m in mobileList)
            {
                m.ReceiveShock(extraDamage);
            }
        }
    }
}
