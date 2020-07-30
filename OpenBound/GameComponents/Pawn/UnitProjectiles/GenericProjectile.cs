using Microsoft.Xna.Framework;
using OpenBound.Common;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Audio;
using OpenBound.GameComponents.Level;
using OpenBound.GameComponents.MobileAction;
using Openbound_Network_Object_Library.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBound.GameComponents.Pawn.UnitProjectiles
{
    public class TeleportationBeacon : Projectile
    {
        public TeleportationBeacon(Mobile mobile)
        : base(mobile, ShotType.TeleportationBeacon, 0, 0)
        {
            //Initializing Flipbook
            FlipbookList.Add(new Flipbook(
                mobile.Crosshair.CannonPosition, new Vector2(18, 14),
                36, 28, "Graphics/Tank/Armor/Bullet1",
                new List<AnimationInstance>() {
                    new AnimationInstance()
                    { StartingFrame = 0, EndingFrame = 4, TimePerFrame = 1 / 20f }
                }, DepthParameter.Projectile, angle));

            //Physics/Trajectory setups
            mass = Parameter.ProjectileTeleportationBeaconMass;
            windInfluence = Parameter.ProjectileTeleportationBeaconWindInfluence;
        }

        public override bool UpdateCollider(Vector2 position)
        {
            bool hasExploded = false;

            //Check collision with ground
            if (CanCollide && Topography.CheckCollision(position))
            {
                hasExploded = true;
                Explode();
#if Debug
                debugCrosshair.Update(FlipbookList[0].Position);
#endif
            }

            return hasExploded;
        }

        public override void PlayLaunchSFX()
        {
            AudioHandler.PlaySoundEffect("Audio/SFX/Tank/Shoot/Teleport", pitch: -0.5f);
        }

        public override void PlayExplosionSFX()
        {
            AudioHandler.PlaySoundEffect("Audio/SFX/Tank/Blast/Teleport", pitch:  0.5f);
        }

        public override void Explode()
        {
            base.Explode();
            PlayExplosionSFX();
            SpecialEffectBuilder.TeleportFlame1(previousPosition);
            SpecialEffectBuilder.TeleportFlame2(Mobile.Position);
            Mobile.Position = previousPosition;
            Mobile.SyncPosition = previousPosition;
        }

        protected override void Destroy()
        {
            base.Destroy();

            List<Projectile> pjList = Mobile.ProjectileList.Except(Mobile.UnusedProjectile).ToList();

            if (pjList.Count() == 0)
                OnFinalizeExecutionAction?.Invoke();
        }
    }
}
