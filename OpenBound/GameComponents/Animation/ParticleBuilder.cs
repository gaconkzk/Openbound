
using Microsoft.Xna.Framework;
using OpenBound.Common;
using OpenBound.Extension;
using OpenBound.GameComponents.Level.Scene;
using Openbound_Network_Object_Library.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBound.GameComponents.Animation
{
    public class ParticleBuilder
    {
        static readonly ParticleUpdateConfiguration groundParticleUpdateConfiguration = new ParticleUpdateConfiguration() { Rotate = true, SetScale = true, UseYAcceleration = true, UseXAcceleration = true };
        static readonly ParticleUpdateConfiguration forceParticleUpdateConfiguration = new ParticleUpdateConfiguration() { SetScale = true, Fade = true, Rotate = true, UseXAcceleration = true, UseYAcceleration = true };
        public static void CreateGroundCollapseParticleEffect(int numberOfParticles, Vector2 initialPosition, float angleTrajectory)
        {
            List<Flipbook> flipbookList = BuildGroundParticle(numberOfParticles);
           
            float windAngle = LevelScene.MatchMetadata.WindAngleRadians;
            Vector2 windVector = new Vector2((float)Math.Cos(windAngle), (float)Math.Sin(windAngle)) * LevelScene.MatchMetadata.WindForce * Parameter.GroundParticleWindInfluenceFactor;

            for (int i = 0; i < flipbookList.Count; i++)
            {
                float spreadFactor = i % Parameter.GroundParticleSpreadFactor + 1;
                float disturbance = ((float)Parameter.Random.NextDouble() - 0.5f) * MathHelper.TwoPi / spreadFactor;
                float angularSpread = angleTrajectory + disturbance;

                flipbookList[i].Position = initialPosition;
                flipbookList[i].Rotation = angularSpread;

                Particle p = new Particle(flipbookList[i], new Vector2((float)Math.Cos(angularSpread), (float)Math.Sin(angularSpread)), windVector, groundParticleUpdateConfiguration);
                SpecialEffectHandler.Add(p);
            }
        }

        public static void CreateForceCollapseParticleEffect(int numberOfParticles, Vector2 initialPosition, float angleTrajectory)
        {
            List<Flipbook> flipbookList = BuildForceParticle(numberOfParticles);

            for (int i = 0; i < flipbookList.Count; i++)
            {
                float angle = (float)Parameter.Random.NextDouble() * MathHelper.TwoPi;
                flipbookList[i].Position = initialPosition;
                flipbookList[i].Rotation = angle;

                Particle p = new Particle(flipbookList[i], new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)), Vector2.Zero, forceParticleUpdateConfiguration);
                SpecialEffectHandler.Add(p);
            }
        }

        private static List<Flipbook> BuildForceParticle(int particleNumber)
        {
            List<Flipbook> flipbookList = new List<Flipbook>();

            Vector2 pivot = new Vector2(16, 16);
            string groundParticlePath = $"Graphics/Special Effects/Weather/ForceParticle";

            for (; particleNumber > 0; particleNumber--)
            {
                AnimationInstance animationInstance = new AnimationInstance();
                animationInstance.StartingFrame = animationInstance.EndingFrame = Parameter.Random.Next(0, 8);

                flipbookList.Add(Flipbook.CreateFlipbook(Vector2.Zero, pivot, (int)(pivot.X * 2), (int)(pivot.Y * 2),
                    groundParticlePath, animationInstance, false, DepthParameter.Projectile));
            }

            return flipbookList;
        }

        private static List<Flipbook> BuildParticle(int particleNumber, Vector2 pivot, string particlePath, int numberOfFrames)
        {
            List<Flipbook> flipbookList = new List<Flipbook>();

            for (; particleNumber > 0; particleNumber--)
            {
                AnimationInstance animationInstance = new AnimationInstance();
                animationInstance.StartingFrame = animationInstance.EndingFrame = Parameter.Random.Next(0, numberOfFrames);

                flipbookList.Add(Flipbook.CreateFlipbook(Vector2.Zero, pivot, (int)(pivot.X * 2), (int)(pivot.Y * 2),
                    particlePath, animationInstance, false, DepthParameter.Projectile));
            }

            return flipbookList;
        }

        private static List<Flipbook> BuildGroundParticle(int particleNumber)
        {
            List<Flipbook> flipbookList = new List<Flipbook>();

            Map map = GameInformation.Instance.RoomMetadata.Map;
            Vector2 pivot = map.GroundParticlePivot.ToVector2();
            string groundParticlePath = $"Graphics/Maps/{map.GameMap}/Particle";

            for (; particleNumber > 0; particleNumber--)
            {
                AnimationInstance animationInstance = new AnimationInstance();
                animationInstance.StartingFrame = animationInstance.EndingFrame = Parameter.Random.Next(0, map.GroundParticleNumbers);

                flipbookList.Add(Flipbook.CreateFlipbook(Vector2.Zero, pivot, (int)(pivot.X * 2), (int)(pivot.Y * 2),
                    groundParticlePath, animationInstance, false, DepthParameter.Projectile));
            }

            return flipbookList;
        }
    }
}
