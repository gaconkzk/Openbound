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
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Collision;
using OpenBound.GameComponents.Interface;
using OpenBound.GameComponents.Pawn.UnitProjectiles;
using OpenBound.GameComponents.MobileAction;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Models;

namespace OpenBound.GameComponents.Pawn.Unit
{
    public class Knight : Mobile
    {
        public KnightSword Satellite;

        public Knight(Player player, Vector2 position) : base(player, MobileType.Knight)
        {
            Position = position;

            MobileFlipbook = MobileFlipbook.CreateMobileFlipbook(MobileType.Knight, position);

            Movement.CollisionOffset = 25;
            Movement.MaximumStepsPerTurn = 100;

            Crosshair = new Crosshair(this);

            Satellite = new KnightSword(this);

            CollisionBox = new CollisionBox(this, new Rectangle(0, 0, 45, 30), new Vector2(0, 10));
        }

        public override void Update(GameTime GameTime)
        {
            base.Update(GameTime);

            Satellite.Update(GameTime);
        }

        public override void Die()
        {
            base.Die();
            Movement.CollisionOffset = 18;
            Satellite.Flipbook.HideElement();
        }

        public override void Draw(GameTime GameTime, SpriteBatch SpriteBatch)
        {
            base.Draw(GameTime, SpriteBatch);

            Satellite.Draw(GameTime, SpriteBatch);
        }

        public override void ChangeShot(ShotType ShotType)
        {
            base.ChangeShot(ShotType);
            Satellite.ChangeShotType(ShotType);
        }

        protected override void Shoot()
        {
            if (SelectedShotType == ShotType.S1)
                LastCreatedProjectileList.Add(new KnightProjectile1(this));
            else if (SelectedShotType == ShotType.S2)
                LastCreatedProjectileList.Add(new KnightProjectile2(this));
            else if (SelectedShotType == ShotType.SS)
                LastCreatedProjectileList.Add(new KnightProjectile3(this));

            base.Shoot();
        }
    }
}
