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
    /// <summary>
    /// Creates a "electricity" dummy projectile. This projectile has its own movement rules being updated all at once.
    /// </summary>
    public class ElectricityProjectile : DummyProjectile
    {   
        //Stores all mobiles affected by the proximity with the lightning
        HashSet<Mobile> mobileList;

        //Lightning effect variables
        int extraExplosionRadius;
        int extraDamage;
        float lightningAngle;

        //Position of parent projectile
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
            mobileList = new HashSet<Mobile>();
        }

        /// <summary>
        /// Executes all possible movements at once. Starting at <see cref="parentPosition"/> and finishing at the first pixel outside the area.
        /// </summary>
        public override void Update()
        {
            //This method starts at parentPosition and keep adding a offset until the position is outside the map boundaries.
            //While it hasn't found any collidable spot it keeps saving all mobiles that are going to be affected by the discharge extra effect
            //If it finds another collidable spot, resets all potential affected mobile list and save this position

            //Starts at parent position
            Vector2 explosionPosition = Position = parentPosition;

            //While it is still inside the map
            while (Topography.IsInsideMapBoundaries(Position))
            {
                //Check if there is any mobile whithin the extraExplosionRadius to receive the electrical extra dmaage
                CheckAffectedMobiles();

                //If it collides with the ground or with another mobile, reset all previous affected mobiles and save the new explosion position
                if (Topography.CheckCollision(Position) || LevelScene.MobileList.Any((m) => m.CollisionBox.CheckCollision(Position)))
                {
                    mobileList.Clear();
                    explosionPosition = Position;
                }

                Position += positionOffset;
            }

            //In the end, move the projectile to the last "explodable" spot and explode it
            Position = explosionPosition;
            Explode();
        }

        /// <summary>
        /// Checks if every mobile can be affected by the extra discharge damage
        /// </summary>
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

            //Creates the lightning effect
            LevelScene.WeatherHandler.Add(new LightningElectricity(Position, MathHelper.PiOver2 * 3 - lightningAngle));

            //Deals damage to any mobile whithin the extra damage area
            foreach (Mobile m in mobileList)
            {
                m.ReceiveShock(extraDamage);
            }
        }
    }
}
