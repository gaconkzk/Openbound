using Microsoft.Xna.Framework;
using OpenBound.Common;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.PawnAction;
using Openbound_Network_Object_Library.Entity;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenBound.GameComponents.Pawn.UnitProjectiles
{
    public class DummyProjectile : Projectile
    {
        public DummyProjectile(Mobile mobile, ShotType shotType, int explosionRadius, int baseDamage, bool canCollide = true, Vector2 projectileInitialPosition = default)
            : base(mobile, shotType, explosionRadius, baseDamage, canCollide: canCollide, projectileInitialPosition: projectileInitialPosition)
        {
            this.mobile = mobile;

            //Initializing Flipbook
            FlipbookList.Add(Flipbook.CreateFlipbook(
                mobile.Crosshair.CannonPosition, Vector2.One / 2,
                1, 1, "Misc/Dummy",
                new List<AnimationInstance>() {
                    new AnimationInstance()
                }, true, DepthParameter.Projectile, angle));
        }

        protected override void Destroy()
        {
            base.Destroy();

            List<Projectile> pjList = mobile.ProjectileList.Except(mobile.UnusedProjectile).ToList();

            if (pjList.Count() == 0)
                OnFinalizeExecution?.Invoke();
        }
    }
}
