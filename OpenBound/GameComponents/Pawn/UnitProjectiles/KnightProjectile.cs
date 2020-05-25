﻿/* 
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBound.GameComponents.Pawn.UnitProjectiles
{
    public abstract class KnightProjectile : Projectile
    {
        protected int numberOfSwords;
        protected Vector2 ownerOffset;

        public KnightProjectile(Knight Mobile)
            : base(Mobile, ShotType.S1, 0, 0)
        {
            //Initializing Flipbook
            FlipbookList.Add(Flipbook.CreateFlipbook(
                Mobile.Crosshair.CannonPosition, new Vector2(31, 32),
                64, 66, "Graphics/Tank/Knight/Shot1",
                new List<AnimationInstance>() {
                    new AnimationInstance()
                    { StartingFrame = 0, EndingFrame = 19, TimePerFrame = 1 / 20f }
                }, true, DepthParameter.Projectile));

            //Knightsword Animation
            Mobile.Satellite.AttackingTarget = FlipbookList[0];
        }

        protected override void Explode()
        {
            base.Explode();

            Knight knight = (Knight)mobile;
            knight.Satellite.Flipbook.HideElement();

            //Calculate the starting position of the shot
            Vector2 shotPos = knight.Position + ownerOffset;

            //Angle necessary to spawn the swords with the correct distance between each other
            float angle = (float)Helper.AngleBetween(shotPos, FlipbookList[0].Position) + MathHelper.PiOver2;

            //The offset between swords
            Vector2 offset = new Vector2(Parameter.ProjectileKnightDistanceBetweenShots * (float)Math.Cos(angle), Parameter.ProjectileKnightDistanceBetweenShots * (float)Math.Sin(angle));

            //Spawn 3 swords with offset distance between each other
            for (int i = -numberOfSwords / 2; i <= numberOfSwords / 2; i++)
            {
                KnightSatelliteProjectile ksp = new KnightSatelliteProjectile(knight, shotPos + offset * i,
                    FlipbookList[0].Position, 0.08f * Math.Abs(i));
                ksp.OnFinalizeExecution = OnFinalizeExecution;
                knight.LastCreatedProjectileList.Add(ksp);
            }
        }

        protected override void OutofboundsDestroy()
        {
            OnFinalizeExecution?.Invoke();
            Destroy();
        }
    }

    public class KnightProjectile1 : KnightProjectile
    {
        public KnightProjectile1(Knight Mobile) : base(Mobile)
        {
            //Physics/Trajectory setups
            mass = Parameter.ProjectileKnightS1Mass;
            windInfluence = Parameter.ProjectileKnightS1WindInfluence;

            numberOfSwords = 3;
            ownerOffset = Mobile.Satellite.S1OwnerOffset;
        }
    }

    public class KnightProjectile2 : KnightProjectile
    {
        public KnightProjectile2(Knight Mobile) : base(Mobile)
        {
            //Physics/Trajectory setups
            mass = Parameter.ProjectileKnightS2Mass;
            windInfluence = Parameter.ProjectileKnightS2WindInfluence;

            numberOfSwords = 5;
            ownerOffset = Mobile.Satellite.S2OwnerOffset;
        }
    }

    public class KnightProjectile3 : Projectile
    {
        public KnightProjectile3(Knight Mobile)
            : base(Mobile, ShotType.SS, 0, 0)
        {
            //Initializing Flipbook
            FlipbookList.Add(Flipbook.CreateFlipbook(
                Mobile.Crosshair.CannonPosition,
                new Vector2(25 / 2f, 24 / 2f),
                25, 24,
                "Graphics/Tank/Knight/Shot3",
                new AnimationInstance()
                {
                    StartingFrame = 0,
                    EndingFrame = 19,
                    TimePerFrame = 1 / 20f,
                },
                true,
                DepthParameter.Projectile));

            //Physics/Trajectory setups
            mass = Parameter.ProjectileKnightSSMass;
            windInfluence = Parameter.ProjectileKnightSSWindInfluence;

            //Knightsword Animation
            Mobile.Satellite.AttackingTarget = FlipbookList[0];
        }

        protected override void Explode()
        {
            base.Explode();

            Knight knight = (Knight)mobile;

            //Play the satellite animation
            knight.Satellite.StartSSAnimation();

            //Spawn the shots with spawningTime set based on the future satellite positions
            for (int i = 0; i < 7; i++)
            {
                KnightSatelliteProjectile ksp = new KnightSatelliteProjectile(knight,
                    FlipbookList[0].Position + knight.Satellite.SSTargetOffset - Parameter.SatelliteSSAnimationMovementRange * ((float)Math.Cos(MathHelper.ToRadians(60 * i))),
                    FlipbookList[0].Position, 0, Parameter.SatelliteSSAnimationTotalMotionTime + i / (3f * Parameter.StelliteAttackSpeedFactor));
                ksp.OnFinalizeExecution = OnFinalizeExecution;
                knight.LastCreatedProjectileList.Add(ksp);
            }
        }

        protected override void OutofboundsDestroy()
        {
            OnFinalizeExecution?.Invoke();
            Destroy();
        }
    }

    public class KnightSatelliteProjectile : Projectile
    {
        List<SpecialEffect> swordTrace;
        Vector2 initialPosition;
        float initialAlpha;

        public KnightSatelliteProjectile(Knight Mobile, Vector2 InitialPosition, Vector2 FinalPosition, double FreezeTime = 0, double SpawnTime = 0)
            : base(Mobile, ShotType.Satellite, Parameter.ProjectileKnightExplosionRadius, Parameter.ProjectileKnightSwordBaseDamage,
                  projectileInitialPosition: InitialPosition)
        {
            initialPosition = InitialPosition;
            initialAlpha = Parameter.ProjectileKnightInitialAlpha;

            //Calculate the angle of the swords
            double angle = (float)Helper.AngleBetween(FinalPosition, InitialPosition);

            //Initializing Flipbook
            FlipbookList.Add(
                Flipbook.CreateFlipbook(
                InitialPosition,
                new Vector2(81, 32),
                162, 65,
                "Graphics/Tank/Knight/Bullet1",
                new AnimationInstance()
                {
                    StartingFrame = 0,
                    EndingFrame = 19,
                    TimePerFrame = 1 / 40f,
                },
                true,
                DepthParameter.Projectile,
                Rotation: (float)angle));

            this.FreezeTime = FreezeTime;
            this.SpawnTime = SpawnTime;

            SpawnTimeCounter = FreezeTimeCounter = 0;

            yMovement.Preset((float)(Math.Sin(angle) * Parameter.ProjectileKnightSpeed), 0);
            xMovement.Preset((float)(Math.Cos(angle) * Parameter.ProjectileKnightSpeed), 0);

            swordTrace = new List<SpecialEffect>();
        }

        protected override void UpdatePosition()
        {
            //if the number of swords in trace is 0 
            //or the last created sword reach its limit distance
            if (swordTrace.Count == 0 ||
                Helper.EuclideanDistance(swordTrace.Last().Flipbook.Position, initialPosition) > Parameter.ProjectileKnightTraceDistanceOffset)
            {
                //a new sword and sfx is appended to the trace
                SpecialEffect sword = SpecialEffectBuilder.KnightProjectileBullet1(initialPosition, FlipbookList[0].Rotation);

                sword.Flipbook.Color = new Color(new Vector4(initialAlpha, initialAlpha, initialAlpha, initialAlpha));

                initialAlpha -= Parameter.ProjectileKnightTraceAlphaDecay + swordTrace.Count * Parameter.ProjectileKnightTraceAlphaDecayFactor;

                SpecialEffectHandler.Add(sword);

                swordTrace.Add(sword);
            }

            //and update the sword and the trace positions
            Vector2 position = FlipbookList[0].Position;

            if (FreezeTimeCounter >= FreezeTime)
            {
                base.UpdatePosition();
            }
            else
            {
                FreezeTimeCounter += Parameter.GameplayTimeFrameMaximumDeltaTime;

                if (FreezeTimeCounter >= FreezeTime)
                    PlayLaunchSFX();
            }


            position -= FlipbookList[0].Position;

            swordTrace.ForEach((x) => x.Flipbook.Position -= position);
        }

        protected override void Destroy()
        {
            base.Destroy();

            //Destroy the trace
            SpecialEffectHandler.Remove(swordTrace);

            //If this shot is the last one the satellite should reappear
            if (mobile.ProjectileList.Except(mobile.UnusedProjectile).Count() == 0)
            {
                OnFinalizeExecution?.Invoke();
                ((Knight)mobile).Satellite.Flipbook.ShowElement();
            }
        }
    }
}
