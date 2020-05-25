using Microsoft.Xna.Framework;
using OpenBound.Common;
using OpenBound.GameComponents.Level;
using OpenBound.GameComponents.Level.Scene;
using OpenBound.GameComponents.Physics;
using System;

namespace OpenBound.GameComponents.Animation
{
    public class Particle : SpecialEffect
    {
        float rotatingSpeed;
        float scalingFactor;

        AcceleratedMovement yMovement, xMovement;

        Vector2 initialPosition;

        float RandomValue => (float)(Parameter.Random.NextDouble() - 0.5f);

        public Particle(Flipbook flipbook, Vector2 trajectory, Vector2 windInfluence) : base(flipbook)
        {
            rotatingSpeed = RandomValue * Parameter.GroundParticleMaximumRotatingSpeed;
            scalingFactor = RandomValue * Parameter.GroundParticleMaximumScalingFactor;

            float randomValue = (float)Parameter.Random.NextDouble();

            yMovement = new AcceleratedMovement();
            yMovement.Preset(Parameter.GroundParticleInitialYSpeed + (randomValue * Parameter.GroundParticleMaximumYSpeed * trajectory.Y), Parameter.GroundParticleInitialYAcceleration - windInfluence.Y);

            xMovement = new AcceleratedMovement();
            xMovement.Preset(randomValue * Parameter.GroundParticleMaximumXSpeed * trajectory.X, windInfluence.X);

            initialPosition = flipbook.Position;
        }

        public override bool CeaseFunction()
        {
            return Topography.IsNotInsideMapBoundaries(Flipbook.Position);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            Flipbook.Rotation += rotatingSpeed * elapsedTime;
            Flipbook.Scale += Vector2.One * scalingFactor * elapsedTime;

            xMovement.RefreshCurrentPosition(elapsedTime);
            yMovement.RefreshCurrentPosition(elapsedTime);

            Flipbook.Position = initialPosition + new Vector2(xMovement.CurrentPosition, yMovement.CurrentPosition);
        }
    }
}
