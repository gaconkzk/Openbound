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
using OpenBound.GameComponents.Level.Scene;
using OpenBound.GameComponents.Pawn.Unit;
using OpenBound.GameComponents.PawnAction;
using OpenBound.GameComponents.WeatherEffect;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Entity.Sync;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBound.GameComponents.Pawn.UnitProjectiles
{
    public class RaonLauncherProjectileEmitter
    {
        public static void Shot1(RaonLauncher mobile)
        {
            mobile.LastCreatedProjectileList.Add(new RaonBaseProjectile1(mobile, new Vector2(10, 0), 1 * MathHelper.PiOver2));
            mobile.LastCreatedProjectileList.Add(new RaonBaseProjectile1(mobile, new Vector2(10, 0), 2 * MathHelper.PiOver2));
            mobile.LastCreatedProjectileList.Add(new RaonBaseProjectile1(mobile, new Vector2(10, 0), 3 * MathHelper.PiOver2));
            mobile.LastCreatedProjectileList.Add(new RaonBaseProjectile1(mobile, new Vector2(10, 0), 4 * MathHelper.PiOver2));

            mobile.LastCreatedProjectileList[1].FlipbookList[0].SetCurrentFrame(5);
            mobile.LastCreatedProjectileList[2].FlipbookList[0].SetCurrentFrame(10);
            mobile.LastCreatedProjectileList[3].FlipbookList[0].SetCurrentFrame(15);
        }
    }

    public class RaonProjectile1 : Projectile
    {
        public RaonProjectile1(RaonLauncher mobile)
            : base(mobile, ShotType.S1, Parameter.ProjectileTricoS2ExplosionRadius, Parameter.ProjectileTricoS2BaseDamage)
        {
            //Initializing Flipbook
            FlipbookList.Add(Flipbook.CreateFlipbook(
                mobile.Crosshair.CannonPosition, new Vector2(19, 21),
                38, 42, "Graphics/Tank/RaonLauncher/Bullet1",
                new List<AnimationInstance>() { new AnimationInstance() { StartingFrame = 0, EndingFrame = 39, TimePerFrame = 1 / 20f } }, true, DepthParameter.Projectile, angle));

            FlipbookList[0].SetTransparency(0);
        }

        protected override void Explode()
        {
            base.Explode();
            SpecialEffectBuilder.TricoProjectile2Explosion(FlipbookList[0].Position, FlipbookList[0].Rotation);
        }
    }

    public class RaonBaseProjectile1 : DummyProjectile
    {
        List<SpecialEffect> specialEffectList;
        Vector2 lastSpawnPosition;

        RaonProjectile1 projectile;

        float rotationAngle;

        Vector2 offset;

        public RaonBaseProjectile1(RaonLauncher mobile, Vector2 positionOffset, float rotationAngle)
            : base(mobile, ShotType.S1, 0, 0, canCollide: false)
        {
            projectile = new RaonProjectile1(mobile);
            lastSpawnPosition = mobile.Crosshair.CannonPosition;

            this.rotationAngle = rotationAngle;
            offset = positionOffset;

            specialEffectList = new List<SpecialEffect>();

            SpawnTime = 0.7f;

            projectile.OnFinalizeExecutionAction = OnFinalizeExecutionAction;

            //Physics/Trajectory setups
            mass = Parameter.ProjectileTricoS2Mass;
            windInfluence = Parameter.ProjectileTricoS2WindInfluence;
        }

        #region Weather
        protected override void CheckCollisionWithWeather()
        {
            foreach (Weather w in LevelScene.WeatherHandler.WeatherList)
            {
                if (w.Intersects(projectile) && !w.IsInteracting(this))
                {
                    Position = projectile.Position;
                    w.ModifiedProjectileList.Add(this);
                    w.OnInteract(this);
                }
            }
        }

        //Force
        public override void OnBeginForceInteraction(Force force)
        {
            force.OnInteract(projectile);
        }

        //Weakness
        public override void OnBeginWeaknessInteraction(Weakness weakness)
        {
            weakness.OnInteract(projectile);
        }

        //Electricity
        public override void OnBeginElectricityInteraction(Electricity electricity)
        {
            //electricity.OnInteract(this);
            electricity.OnInteract(projectile);
            OnAfterUpdateAction = projectile.OnAfterUpdateAction;
        }

        //Random
        public override void OnBeginRandomInteraction(WeatherEffect.Random random)
        {
            random.OnInteract(projectile);
        }

        //Thor
        public override void OnBeginThorInteraction(Thor thor)
        {
            thor.OnInteract(projectile);
        }
        #endregion

        public override void OnSpawn()
        {
            base.OnSpawn();
            projectile.FlipbookList[0].SetTransparency(1);
        }

        protected override void UpdateRotation()
        {
            base.UpdateRotation();
            projectile.FlipbookList[0].Rotation = FlipbookList[0].Rotation;
        }

        protected override void UpdatePosition()
        {
            for (float i = 0; i < Parameter.ProjectileMovementTotalTimeElapsed; i += Parameter.ProjectileMovementTimeElapsedPerInteraction)
            {
                base.UpdateMovementIteraction(Parameter.ProjectileMovementTimeElapsedPerInteraction);

                rotationAngle += Parameter.ProjectileMovementTimeElapsedPerInteraction * MathHelper.Pi;

                projectile.FlipbookList[0].Position = Position + Vector2.Transform(offset * (float)Math.Sin(rotationAngle), Matrix.CreateRotationZ(FlipbookList[0].Rotation + MathHelper.PiOver2));

                if (projectile.CheckOutOfBounds(projectile.FlipbookList[0].Position) || projectile.UpdateCollider(projectile.FlipbookList[0].Position))
                {
                    Destroy();
                    break;
                }

                if (Helper.SquaredEuclideanDistance(lastSpawnPosition, projectile.Position) > 600)
                {
                    SpecialEffect se = SpecialEffectBuilder.RaonProjectile1(projectile.Position, projectile.CurrentFlipbookRotation);
                    specialEffectList.Add(se);

                    se.Flipbook.SetCurrentFrame(projectile.FlipbookList[0].GetCurrentFrame());

                    lastSpawnPosition = projectile.Position;
                }
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            projectile.Draw(gameTime, spriteBatch);
        }
    }

    public class RaonProjectile2 : Projectile
    {
        public RaonProjectile2(RaonLauncher mobile) : base(mobile, ShotType.S2, Parameter.ProjectileIceS2ExplosionRadius, Parameter.ProjectileIceS2BaseDamage)
        {
            this.Mobile = mobile;

            //Initializing Flipbook
            FlipbookList.Add(Flipbook.CreateFlipbook(
                mobile.Crosshair.CannonPosition, new Vector2(16.5f, 17f),
                37, 34, "Graphics/Tank/Ice/Bullet2",
                new List<AnimationInstance>() {
                    new AnimationInstance()
                    { StartingFrame = 0, EndingFrame = 11, TimePerFrame = 1 / 20f }
                }, true, DepthParameter.Projectile, angle));

            //Physics/Trajectory setups
            mass = Parameter.ProjectileIceS2Mass;
            windInfluence = Parameter.ProjectileIceS2WindInfluence;

            SpawnTime = 0.2;
        }

        protected override void Explode()
        {
            SpecialEffectBuilder.IceProjectile2Explosion(FlipbookList[0].Position, FlipbookList[0].Rotation);
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

    public class RaonProjectile3 : Projectile
    {
        public RaonProjectile3(RaonLauncher mobile) : base(mobile, ShotType.SS, Parameter.ProjectileIceSSExplosionRadius, Parameter.ProjectileIceSSBaseDamage)
        {
            this.Mobile = mobile;

            //Initializing Flipbook
            FlipbookList.Add(Flipbook.CreateFlipbook(
                mobile.Crosshair.CannonPosition, new Vector2(38.5f, 35f),
                77, 70, "Graphics/Tank/Ice/Bullet3",
                new List<AnimationInstance>() {
                    new AnimationInstance(){ StartingFrame = 0, EndingFrame = 14, TimePerFrame = 1 / 20f }
                },
                true, DepthParameter.Projectile, angle));

            //Physics/Trajectory setups
            mass = Parameter.ProjectileIceSSMass;
            windInfluence = Parameter.ProjectileIceSSWindInfluence;

            SpawnTime = 0.7;
        }

        protected override void Explode()
        {
            SpecialEffectBuilder.IceProjectile3Explosion(FlipbookList[0].Position);
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