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
using OpenBound.GameComponents.Audio;
using OpenBound.GameComponents.Collision;
using OpenBound.GameComponents.Interface;
using OpenBound.GameComponents.Pawn.UnitProjectiles;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Models;

namespace OpenBound.GameComponents.Pawn.Unit
{
    public class Turtle : Mobile
    {
        public Turtle(Player player, Vector2 position) : base(player, MobileType.Turtle)
        {
            Position = position;

            MobileFlipbook = MobileFlipbook.CreateMobileFlipbook(MobileType.Turtle, position);

            Movement.CollisionOffset = 23;
            Movement.MaximumStepsPerTurn = 90;

            Crosshair = new Crosshair(this);

            CollisionBox = new CollisionBox(this, new Rectangle(0, 0, 38, 33), new Vector2(0, 10));
        }

        public override void PlayMovementSE(float pitch = 0, float pan = 0)
        {
            base.PlayMovementSE(pitch: -0.75f);
        }

        public override void PlayUnableToMoveSE(float pitch = 0, float pan = 0)
        {
            base.PlayUnableToMoveSE(pitch: -0.75f);
        }

        protected override void Shoot()
        {
            if (SelectedShotType == ShotType.S1)
                LastCreatedProjectileList.Add(new TurtleProjectile1(this));
            else if (SelectedShotType == ShotType.S2)
                TurtleProjectileEmitter.Shot2(this);
            else if (SelectedShotType == ShotType.SS)
                LastCreatedProjectileList.Add(new TurtleProjectile3(this));

            base.Shoot();
        }
    }
}
