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
using System;

namespace OpenBound.GameComponents.Animation
{
    public class SpecialEffectBuilder
    {
        #region Weather
        public static void ForceRandomParticle(Vector2 position)
        {
            int frame = Parameter.Random.Next(0, 8);

            SpecialEffect se = new SpecialEffect(Flipbook.CreateFlipbook(position, new Vector2(17, 17), 32, 32, "Graphics/Special Effects/Weather/ForceParticle",
                new AnimationInstance() { StartingFrame = frame, EndingFrame = frame }, false, DepthParameter.ProjectileSFX, (float)Parameter.Random.NextDouble() * MathHelper.TwoPi), 0);
            SpecialEffectHandler.Add(se);

            float transparency = 1;
            float transparencyFactor = 0.5f + (float)Parameter.Random.NextDouble();
            float scaleFactor = (float)Parameter.Random.NextDouble() - 0.5f;

            se.UpdateAction += (a, b) =>
            {
                se.Flipbook.SetTransparency(Math.Max(0, transparency -= transparencyFactor * (float)b.ElapsedGameTime.TotalSeconds));
                se.Flipbook.Scale += Vector2.One * scaleFactor * (float)b.ElapsedGameTime.TotalSeconds;

                if (transparency <= 0)
                    SpecialEffectHandler.Remove(se);
            };
        }

        public static SpecialEffect ElectricityParticle(Vector2 position)
        {
            SpecialEffect se = new SpecialEffect(Flipbook.CreateFlipbook(position, new Vector2(16, 16), 32, 32, "Graphics/Special Effects/Weather/ElectricityParticle",
                new AnimationInstance() { EndingFrame = 8, TimePerFrame = 1 / 15f, AnimationType = AnimationType.Cycle }, false, DepthParameter.ProjectileSFX, (float)Parameter.Random.NextDouble() * MathHelper.TwoPi), 0);
            se.Flipbook.JumpToRandomAnimationFrame();
            SpecialEffectHandler.Add(se);
            return se;
        }
        #endregion

        #region Common
        public static void CommonFlameSS(Vector2 position, float rotation)
        {
            SpecialEffectHandler.Add(new SpecialEffect(Flipbook.CreateFlipbook(position, new Vector2(185, 170) / 2, 185, 170, "Graphics/Special Effects/Tank/Common/FlameSS",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 30, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX, rotation - MathHelper.PiOver2), 1));
        }
        #endregion
        #region Armor
        public static void ArmorProjectile1Explosion(Vector2 position)
        {
            SpecialEffectHandler.Add(new SpecialEffect(Flipbook.CreateFlipbook(position, new Vector2(96, 96), 192, 192, "Graphics/Special Effects/Tank/Armor/Flame1",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 19, TimePerFrame = 1 / 30f }, false, DepthParameter.Projectile), 1));
        }

        public static void ArmorProjectile2Explosion(Vector2 position)
        {
            SpecialEffectHandler.Add(new SpecialEffect(Flipbook.CreateFlipbook(position, new Vector2(87, 91), 175, 183, "Graphics/Special Effects/Tank/Armor/Flame2",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 19, TimePerFrame = 1 / 30f }, false, DepthParameter.Projectile), 1));
        }

        public static void ArmorProjectile3Explosion(Vector2 position)
        {
            SpecialEffectHandler.Add(new SpecialEffect(Flipbook.CreateFlipbook(position, new Vector2(96, 96), 192, 192, "Graphics/Special Effects/Tank/Armor/Flame3",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 29, TimePerFrame = 1 / 30f }, false, DepthParameter.Projectile), 1));
        }
        #endregion
        #region Bigfoot
        public static void BigfootProjectile1Explosion(Vector2 position)
        {
            SpecialEffectHandler.Add(new SpecialEffect(Flipbook.CreateFlipbook(position, new Vector2(166, 158) / 2, 166, 158, "Graphics/Special Effects/Tank/Bigfoot/Flame1",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 19, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX), 1));
        }

        public static void BigfootProjectile2Explosion(Vector2 position)
        {
            SpecialEffectHandler.Add(new SpecialEffect(Flipbook.CreateFlipbook(position, new Vector2(78, 73) / 2, 78, 73, "Graphics/Special Effects/Tank/Bigfoot/Flame2",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 19, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX), 1)); ;
        }

        public static void BigfootProjectile3Explosion(Vector2 position)
        {
            SpecialEffectHandler.Add(new SpecialEffect(Flipbook.CreateFlipbook(position, new Vector2(96, 96), 192, 192, "Graphics/Special Effects/Tank/Bigfoot/Flame3",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 29, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX), 1));
        }
        #endregion
        #region Dragon
        public static SpecialEffect DragonProjectile1Explosion(Vector2 position, float rotation, float layerDepth = DepthParameter.ProjectileSFX)
        {
            Flipbook fb = Flipbook.CreateFlipbook(position, new Vector2(94, 92.5f), 188, 185, "Graphics/Special Effects/Tank/Dragon/Flame1",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 30, TimePerFrame = 1 / 30f }, false, layerDepth, rotation);

            SpecialEffect se = new SpecialEffect(fb, 1);

            SpecialEffectHandler.Add(se);

            return se;
        }
        #endregion
        #region Ice
        public static void IceProjectile1Explosion(Vector2 position, float rotation)
        {
            Flipbook fb = Flipbook.CreateFlipbook(position, new Vector2(90, 86), 180, 172, "Graphics/Special Effects/Tank/Ice/Flame1",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 16, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX, rotation);

            SpecialEffect se = new SpecialEffect(fb, 1);

            SpecialEffectHandler.Add(se);
        }

        public static void IceProjectile2Explosion(Vector2 position, float rotation)
        {
            Flipbook fb = Flipbook.CreateFlipbook(position, new Vector2(87.5f, 87f), 175, 174, "Graphics/Special Effects/Tank/Ice/Flame2",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 27, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX, rotation);

            SpecialEffect se = new SpecialEffect(fb, 1);
            SpecialEffectHandler.Add(se);
        }

        public static void IceProjectile3Explosion(Vector2 position)
        {
            Flipbook fb = Flipbook.CreateFlipbook(position, new Vector2(185, 187), 370, 374, "Graphics/Special Effects/Tank/Ice/Flame3",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 30, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX);

            SpecialEffect se = new SpecialEffect(fb, 1);

            SpecialEffectHandler.Add(se);
        }
        #endregion
        #region Knight
        public static SpecialEffect KnightProjectileBullet1(Vector2 position, float rotation)
        {
            SpecialEffect se = new SpecialEffect(Flipbook.CreateFlipbook(position, new Vector2(81, 32), 162, 65, "Graphics/Tank/Knight/Bullet1",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 19, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX, rotation), 0);
            SpecialEffectHandler.Add(se);
            return se;
        }
        #endregion
        #region Mage
        public static void MageProjectile1Explosion(Vector2 position, float rotation)
        {
            Flipbook fb = Flipbook.CreateFlipbook(position, new Vector2(96.5f, 96), 193, 192, "Graphics/Special Effects/Tank/Mage/Flame1",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 17, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX, rotation);

            SpecialEffect se = new SpecialEffect(fb, 1);

            SpecialEffectHandler.Add(se);
        }

        public static void MageProjectile2Explosion(Vector2 position, float rotation)
        {
            Flipbook fb = Flipbook.CreateFlipbook(position, new Vector2(82, 82), 143, 142, "Graphics/Special Effects/Tank/Mage/Flame2",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 17, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX, rotation);

            SpecialEffect se = new SpecialEffect(fb, 1);

            SpecialEffectHandler.Add(se);
        }

        public static void MageProjectile3Explosion(Vector2 position, float rotation)
        {
            Flipbook fb = Flipbook.CreateFlipbook(position, new Vector2(192.5f, 192), 385, 384, "Graphics/Special Effects/Tank/Mage/Flame3",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 30, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX, rotation);

            SpecialEffect se = new SpecialEffect(fb, 1);

            float transparency = 1f;
            se.UpdateAction += (specialEffect, gameTime) =>
            {
                se.Flipbook.Rotation += MathHelper.Pi * (float)gameTime.ElapsedGameTime.TotalSeconds;
                transparency -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                se.Flipbook.SetTransparency(transparency);
            };

            SpecialEffectHandler.Add(se);
        }
        #endregion
        #region Turtle
        public static void TurtleProjectile1Explosion(Vector2 position, float rotation)
        {
            Flipbook fb = Flipbook.CreateFlipbook(position, new Vector2(104f, 105f), 193, 200, "Graphics/Special Effects/Tank/Turtle/Flame1",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 18, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX, rotation);

            SpecialEffect se = new SpecialEffect(fb, 1);

            SpecialEffectHandler.Add(se);
        }

        public static void TurtleProjectile2Explosion(Vector2 position, float rotation)
        {
            Flipbook fb = Flipbook.CreateFlipbook(position, new Vector2(86f, 89f), 164, 170, "Graphics/Special Effects/Tank/Turtle/Flame2",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 18, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX, rotation);

            SpecialEffect se = new SpecialEffect(fb, 1);

            SpecialEffectHandler.Add(se);
        }

        public static void TurtleProjectile3Explosion(Vector2 position, float rotation)
        {
            Flipbook fb = Flipbook.CreateFlipbook(position, new Vector2(90f, 93f), 159, 173, "Graphics/Special Effects/Tank/Turtle/Flame3",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 28, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX, rotation);

            SpecialEffect se = new SpecialEffect(fb, 1);

            SpecialEffectHandler.Add(se);
        }

        public static void TurtleProjectile3EExplosion(Vector2 position, float rotation)
        {
            Flipbook fb = Flipbook.CreateFlipbook(position, new Vector2(92f, 95f), 157, 188, "Graphics/Special Effects/Tank/Turtle/Flame3E",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 15, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX, rotation);

            SpecialEffect se = new SpecialEffect(fb, 1);

            SpecialEffectHandler.Add(se);
        }

        public static void TurtleProjectile3Division(Vector2 position, float rotation)
        {
            Flipbook fb = Flipbook.CreateFlipbook(position, new Vector2(78f, 65f), 186, 129, "Graphics/Special Effects/Tank/Turtle/Flame3Divide",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 10, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX, rotation);

            SpecialEffect se = new SpecialEffect(fb, 1);

            SpecialEffectHandler.Add(se);
        }
        #endregion
        #region Trico
        public static void TricoProjectile1Explosion(Vector2 position, float rotation)
        {
            Flipbook fb = Flipbook.CreateFlipbook(position, new Vector2(90f, 100f), 192, 192, "Graphics/Special Effects/Tank/Trico/Flame1",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 19, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX, rotation);
            SpecialEffect se = new SpecialEffect(fb, 1);
            SpecialEffectHandler.Add(se);
        }

        public static void TricoProjectile2Explosion(Vector2 position, float rotation)
        {
            Flipbook fb = Flipbook.CreateFlipbook(position, new Vector2(90f, 100f), 192, 192, "Graphics/Special Effects/Tank/Trico/Flame1",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 19, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX, rotation);
            fb.Scale = new Vector2(0.7f, 0.7f);
            fb.Effect = SpriteEffects.FlipVertically;
            SpecialEffect se = new SpecialEffect(fb, 1);
            SpecialEffectHandler.Add(se);
        }

        public static void TricoProjectile3Explosion(Vector2 position)
        {
            Flipbook fb = Flipbook.CreateFlipbook(position, new Vector2(179f, 197f), 311, 313, "Graphics/Special Effects/Tank/Trico/Flame3",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 29, TimePerFrame = 1 / 20f }, false, DepthParameter.ProjectileSFX, 0);
            SpecialEffect se = new SpecialEffect(fb, 1);

            SpecialEffectHandler.Add(se);
        }
        #endregion
        #region Lightning

        public static void LightningProjectile3Explosion(Vector2 position)
        {
            Flipbook fb = Flipbook.CreateFlipbook(position, new Vector2(196, 193), 324, 326, "Graphics/Special Effects/Tank/Lightning/Flame2",
                new AnimationInstance() { StartingFrame = 0, EndingFrame = 17, TimePerFrame = 1 / 30f }, false, DepthParameter.ProjectileSFX);

            SpecialEffect se = new SpecialEffect(fb, 1);

            float transparency = 1f;
            se.UpdateAction += (specialEffect, gameTime) =>
            {
                se.Flipbook.Rotation += MathHelper.Pi * (float)gameTime.ElapsedGameTime.TotalSeconds / 25;
                transparency -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                se.Flipbook.SetTransparency(transparency);
            };

            SpecialEffectHandler.Add(se);
        }

        #endregion
        /*
        public static void BuildMobileItem(Mobile mobile)
        {
            SpecialEffect.Add(new SpecialEffect(Flipbook.CreateFlipbook(mobile.MobileFlipbook.Flipbook.Position, new Vector2(42, 71), 157, 123, "Graphics/SpecialEffects/MobileUseItem",
                new AnimationInstance() { AnimationType = AnimationType.Foward, StartingFrame = 0, EndingFrame = 18, TimePerFrame = 1 / 36f }, false, 1, Rotation: mobile.MobileFlipbook.Flipbook.Rotation), 1));
        }*/
    }
}
