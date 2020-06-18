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

    public class JDProjectile1 : Projectile
    {
        public JDProjectile1(JD mobile) : base(mobile, ShotType.S1, Parameter.ProjectileJDS1ExplosionRadius, Parameter.ProjectileJDS1BaseDamage)
        {
            //Initializing Flipbook
            FlipbookList.Add(Flipbook.CreateFlipbook(
                mobile.Crosshair.CannonPosition, new Vector2(15, 15),
                30, 30, "Graphics/Tank/JD/Bullet1",
                new List<AnimationInstance>() {
                    new AnimationInstance()
                    { StartingFrame = 0, EndingFrame = 7, TimePerFrame = 1 / 20f }
                }, true, DepthParameter.Projectile, angle));

            //Physics/Trajectory setups
            mass = Parameter.ProjectileJDS1Mass;
            windInfluence = Parameter.ProjectileJDS1WindInfluence;

            SpawnTime = 0.2f;
        }

        protected override void Explode()
        {
            SpecialEffectBuilder.JDProjectile1Explosion(FlipbookList[0].Position, 0);
            base.Explode();

        }

        protected override void Destroy()
        {
            base.Destroy();

            List<Projectile> pjList = Mobile.ProjectileList.Except(Mobile.UnusedProjectile).ToList();

            if (pjList.Count() == 0)
                OnFinalizeExecutionAction?.Invoke();
        }
    }

    public class JDProjectile2 : Projectile
    {
        public JDProjectile2(JD mobile) : base(mobile, ShotType.S2, Parameter.ProjectileJDS2ExplosionRadius, Parameter.ProjectileJDS2BaseDamage)
        {
            this.Mobile = mobile;

            //Initializing Flipbook
            FlipbookList.Add(Flipbook.CreateFlipbook(
                mobile.Crosshair.CannonPosition, new Vector2(23, 23),
                47, 47, "Graphics/Tank/JD/Bullet2",
                new List<AnimationInstance>() {
                    new AnimationInstance()
                    { StartingFrame = 0, EndingFrame = 7, TimePerFrame = 1 / 20f }
                }, true, DepthParameter.Projectile, angle));

            //Physics/Trajectory setups
            mass = Parameter.ProjectileJDS2Mass;
            windInfluence = Parameter.ProjectileJDS2WindInfluence;

            SpawnTime = 0.2;
        }

        protected override void Explode()
        {
            SpecialEffectBuilder.JDProjectile2Explosion(FlipbookList[0].Position, 0);
            base.Explode();
        }

        protected override void Destroy()
        {
            base.Destroy();

            List<Projectile> pjList = Mobile.ProjectileList.Except(Mobile.UnusedProjectile).ToList();

            if (pjList.Count() == 0)
                OnFinalizeExecutionAction?.Invoke();
        }
    }

    public class JDProjectile3 : Projectile
    {
        public JDProjectile3(JD mobile) : base(mobile, ShotType.SS, Parameter.ProjectileJDSSExplosionRadius, Parameter.ProjectileJDSSBaseDamage)
        {
            this.Mobile = mobile;

            //Initializing Flipbook
            FlipbookList.Add(Flipbook.CreateFlipbook(
                mobile.Crosshair.CannonPosition, new Vector2(26, 28),
                55, 56, "Graphics/Tank/JD/Bullet3",
                new List<AnimationInstance>() {
                    new AnimationInstance(){ StartingFrame = 0, EndingFrame = 7, TimePerFrame = 1 / 20f }
                },
                true, DepthParameter.Projectile, angle));

            //Physics/Trajectory setups
            mass = Parameter.ProjectileJDSSMass;
            windInfluence = Parameter.ProjectileJDSSWindInfluence;

            SpawnTime = 0.2;
        }

        protected override void Explode()
        {
            SpecialEffectBuilder.JDProjectile3Explosion(FlipbookList[0].Position, 0);
            base.Explode();
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