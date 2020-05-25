/* 
 * Copyright (C) 2020, Carlos H.M.S. <carlos_judo@hotmail.com>
 * This file is part of OpenBound.
 * OpenBound is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or(at your option) any later version.
 * 
 * OpenBound is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with OpenBound. If not, see http://www.gnu.org/licenses/.
 */

using Microsoft.Xna.Framework;
using OpenBound.Common;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Pawn.Unit;
using OpenBound.GameComponents.PawnAction;
using Openbound_Network_Object_Library.Entity;
using System.Collections.Generic;
using System.Linq;

namespace OpenBound.GameComponents.Pawn.UnitProjectiles
{
    public class ArmorProjectile1 : Projectile
    {
        public ArmorProjectile1(Armor mobile)
            : base(mobile, ShotType.S1, Parameter.ProjectileArmorS1ExplosionRadius, Parameter.ProjectileArmorS1BaseDamage)
        {
            //Initializing Flipbook
            FlipbookList.Add(Flipbook.CreateFlipbook(
                mobile.Crosshair.CannonPosition, new Vector2(18, 14),
                36, 28, "Graphics/Tank/Armor/Bullet1",
                new List<AnimationInstance>() {
                    new AnimationInstance()
                    { StartingFrame = 0, EndingFrame = 4, TimePerFrame = 1 / 20f }
                }, true, DepthParameter.Projectile));

            //Physics/Trajectory setups
            mass = Parameter.ProjectileArmorS1Mass;
            windInfluence = Parameter.ProjectileArmorS1WindInfluence;
        }

        protected override void Explode()
        {
            base.Explode();
            SpecialEffectBuilder.ArmorProjectile1Explosion(FlipbookList[0].Position);
        }

        protected override void Destroy()
        {
            base.Destroy();

            List<Projectile> pjList = mobile.ProjectileList.Except(mobile.UnusedProjectile).ToList();

            if (pjList.Count() == 0)
                OnFinalizeExecution?.Invoke();
        }
    }

    public class ArmorProjectile2 : Projectile
    {
        bool primaryExplosion;

        public ArmorProjectile2(Armor mobile)
            : base(mobile, ShotType.S2, Parameter.ProjectileArmorS2ExplosionRadius, Parameter.ProjectileArmorS2BaseDamage)
        {
            primaryExplosion = true;
            this.mobile = mobile;

            //Initializing Flipbook
            FlipbookList.Add(Flipbook.CreateFlipbook(
                mobile.Crosshair.CannonPosition, new Vector2(31, 14),
                63, 28, "Graphics/Tank/Armor/Bullet2",
                new List<AnimationInstance>() {
                    new AnimationInstance()
                    { StartingFrame = 0, EndingFrame = 4, TimePerFrame = 1 / 20f }
                }, true, DepthParameter.Projectile));

            //Physics/Trajectory setups
            mass = Parameter.ProjectileArmorS2Mass;
            windInfluence = Parameter.ProjectileArmorS2WindInfluence;

            SpawnTime = 0.6;
        }

        protected override void Explode()
        {
            SpecialEffectBuilder.ArmorProjectile2Explosion(FlipbookList[0].Position);
            base.Explode();
            ExplosionRadius = Parameter.ProjectileArmorS2EExplosionRadius;
            BaseDamage = Parameter.ProjectileArmorS2EBaseDamage;
        }

        protected override void Destroy()
        {
            if (!primaryExplosion)
            {
                base.Destroy();

                List<Projectile> pjList = mobile.ProjectileList.Except(mobile.UnusedProjectile).ToList();

                if (pjList.Count() == 0)
                    OnFinalizeExecution?.Invoke();
            }

            primaryExplosion = false;
        }
    }

    public class ArmorProjectile3 : Projectile
    {
        ProjectileAnimation rocketAnimation;

        public Dictionary<ProjectileAnimation, AnimationInstance> rocketAnimationPresets
            = new Dictionary<ProjectileAnimation, AnimationInstance>()
            {
                {
                    ProjectileAnimation.Closed,
                    new AnimationInstance()
                    { StartingFrame = 0, EndingFrame = 10, TimePerFrame = 1 / 20f }
                },
                {
                    ProjectileAnimation.Opening,
                    new AnimationInstance()
                    { StartingFrame = 11, EndingFrame = 21, TimePerFrame = 1 / 15f }
                },
                {
                    ProjectileAnimation.Opened,
                    new AnimationInstance()
                    { StartingFrame = 22, EndingFrame = 31, TimePerFrame = 1 / 20f }
                }
            };

        float totalTravelledTime;

        public ArmorProjectile3(Armor mobile)
            : base(mobile, ShotType.SS, Parameter.ProjectileArmorSSExplosionRadius, Parameter.ProjectileArmorSSBaseDamage)
        {
            this.mobile = mobile;

            rocketAnimation = ProjectileAnimation.Closed;
            totalTravelledTime = 0;

            //Initializing Flipbook
            FlipbookList.Add(Flipbook.CreateFlipbook(
                mobile.Crosshair.CannonPosition, new Vector2(65, 54),
                131, 109, "Graphics/Tank/Armor/Bullet3",
                new List<AnimationInstance>() { rocketAnimationPresets[rocketAnimation] },
                true, DepthParameter.Projectile));

            //Physics/Trajectory setups
            mass = Parameter.ProjectileArmorSSMass;
            windInfluence = Parameter.ProjectileArmorSSWindInfluence;

            SpawnTime = 0.7;
        }

        protected override void UpdatePosition()
        {
            switch (rocketAnimation)
            {
                case ProjectileAnimation.Closed:
                    if (totalTravelledTime < Parameter.ProjectileArmorSSTransformTime)
                    {
                        totalTravelledTime += Parameter.ProjectileMovementTotalTimeElapsed;
                        base.UpdatePosition();
                    }
                    else
                    {
                        rocketAnimation = ProjectileAnimation.Opening;
                        FlipbookList[0].AppendAnimationIntoCycle(rocketAnimationPresets[ProjectileAnimation.Opening], true);
                        FlipbookList[0].AppendAnimationIntoCycle(rocketAnimationPresets[ProjectileAnimation.Opened]);
                        BaseDamage = Parameter.ProjectileArmorSSExplosionRadius;
                        ExplosionRadius = Parameter.ProjectileArmorSSEExplosionRadius;
                    }
                    break;
                case ProjectileAnimation.Opening:
                    if (FlipbookList[0].CurrentAnimationInstance == rocketAnimationPresets[ProjectileAnimation.Opened])
                    {
                        rocketAnimation = ProjectileAnimation.Opened;
                    }
                    break;
                case ProjectileAnimation.Opened:
                    base.UpdatePosition();
                    break;
            }
        }

        protected override void Explode()
        {
            SpecialEffectBuilder.ArmorProjectile3Explosion(FlipbookList[0].Position);
            base.Explode();
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
