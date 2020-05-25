
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

                Particle p = new Particle(flipbookList[i], new Vector2((float)Math.Cos(angularSpread), (float)Math.Sin(angularSpread)), windVector);
                SpecialEffectHandler.Add(p);
            }
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
