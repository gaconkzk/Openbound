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
using Microsoft.Xna.Framework.Graphics;
using OpenBound.Common;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Pawn.Unit;
using OpenBound.GameComponents.PawnAction;
using Openbound_Network_Object_Library.Entity;
using System.Collections.Generic;
using System.Linq;

namespace OpenBound.GameComponents.Pawn.UnitProjectiles
{
    public class TricoProjectile1 : Projectile
    {
        public TricoProjectile1(Trico mobile)
            : base(mobile, ShotType.S1, Parameter.ProjectileTricoS1ExplosionRadius, Parameter.ProjectileTricoS1BaseDamage)
        {
            //Initializing Flipbook
            FlipbookList.Add(Flipbook.CreateFlipbook(
                mobile.Crosshair.CannonPosition, new Vector2(16, 16),
                32, 32, "Graphics/Tank/Trico/Bullet1",
                new List<AnimationInstance>() {
                    new AnimationInstance()
                    { StartingFrame = 0, EndingFrame = 7, TimePerFrame = 1 / 20f }
                }, true, DepthParameter.Projectile));

            //Physics/Trajectory setups
            mass = Parameter.ProjectileTricoS1Mass;
            windInfluence = Parameter.ProjectileTricoS1WindInfluence;
        }

        protected override void Explode()
        {
            base.Explode();
            //SpecialEffectBuilder.ArmorProjectile1Explosion(FlipbookList[0].Position);
        }

        protected override void Destroy()
        {
            base.Destroy();

            List<Projectile> pjList = mobile.ProjectileList.Except(mobile.UnusedProjectile).ToList();

            if (pjList.Count() == 0)
                OnFinalizeExecution?.Invoke();
        }
    }

    public class TricoProjectile2 : Projectile
    {
        public TricoProjectile2(Trico mobile)
            : base(mobile, ShotType.S2, Parameter.ProjectileTricoS2ExplosionRadius, Parameter.ProjectileTricoS2BaseDamage)
        {
            //Initializing Flipbook
            FlipbookList.Add(Flipbook.CreateFlipbook(
                mobile.Crosshair.CannonPosition, new Vector2(16, 16),
                32, 32, "Graphics/Tank/Trico/Bullet2",
                new List<AnimationInstance>() {
                    new AnimationInstance()
                    { StartingFrame = 0, EndingFrame = 7, TimePerFrame = 1 / 20f }
                }, true, DepthParameter.Projectile));
        }

        protected override void Explode()
        {
            base.Explode();
            //SpecialEffectBuilder.ArmorProjectile1Explosion(FlipbookList[0].Position);
        }
    }

    public class TricoDummyProjectile2 : DummyProjectile
    {
        TricoProjectile2 projectile;
        float angleOffset;
        float rotationAngle;
        Vector2 offset;
        
        public TricoDummyProjectile2(Trico mobile, float angleOffset)
            : base(mobile, ShotType.S2, 0, 0, canCollide: false)
        {
            projectile = new TricoProjectile2(mobile);

            this.angleOffset = angleOffset;
            rotationAngle = 0;
            offset = new Vector2(20, 0);

            //Physics/Trajectory setups
            mass = Parameter.ProjectileTricoS2Mass;
            windInfluence = Parameter.ProjectileTricoS2WindInfluence;
        }

        public override void Update()
        {
            base.Update();

            for (float i = 0; i < Parameter.ProjectileMovementTotalTimeElapsed; i += Parameter.ProjectileMovementTimeElapsedPerInteraction)
            {
                UpdateMovementIteraction(Parameter.ProjectileMovementTimeElapsedPerInteraction);

                rotationAngle += Parameter.ProjectileMovementTimeElapsedPerInteraction * MathHelper.Pi;
                
                projectile.FlipbookList[0].Position = Position + Vector2.Transform(offset, Matrix.CreateRotationZ(angleOffset + rotationAngle));
                if (projectile.UpdateCollider(projectile.FlipbookList[0].Position))
                    Destroy();
            }
        }

        protected override void Destroy()
        {
            base.Destroy();

            List<Projectile> pjList = mobile.ProjectileList.Except(mobile.UnusedProjectile).ToList();

            if (pjList.Count() == 0)
                OnFinalizeExecution?.Invoke();
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            projectile.Draw(gameTime, spriteBatch);
        }
    }

    public class TricoProjectile3 : Projectile
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

        public TricoProjectile3(Armor mobile)
            : base(mobile, ShotType.SS, Parameter.ProjectileArmorSSExplosionRadius, Parameter.ProjectileArmorSSBaseDamage)
        {
            this.mobile = mobile;

            rocketAnimation = ProjectileAnimation.Closed;
            totalTravelledTime = 0;

            //Initializing Flipbook
            FlipbookList.Add(Flipbook.CreateFlipbook(
                mobile.Crosshair.CannonPosition, new Vector2(65, 54),
                131, 109, "Graphics/Tank/Trico/Bullet3",
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
