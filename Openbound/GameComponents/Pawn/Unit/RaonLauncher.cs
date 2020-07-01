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
using Microsoft.Xna.Framework.Graphics;
using OpenBound.Extension;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Collision;
using OpenBound.GameComponents.Interface;
using OpenBound.GameComponents.Pawn.UnitProjectiles;
using OpenBound.GameComponents.MobileAction;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Models;
using System.Collections;
using System.Collections.Generic;
using OpenBound.GameComponents.Level.Scene;
using System.Linq;

namespace OpenBound.GameComponents.Pawn.Unit
{
    public class RaonLauncher : Mobile
    {
        bool mineTurn;

        public RaonLauncher(Player player, Vector2 position) : base(player, MobileType.RaonLauncher)
        {
            Position = position;

            MobileFlipbook = MobileFlipbook.CreateMobileFlipbook(MobileType.RaonLauncher, position);

            Movement.CollisionOffset = 22;
            Movement.MaximumStepsPerTurn = 90;

            Crosshair = new Crosshair(this);

            CollisionBox = new CollisionBox(this, new Rectangle(0, 0, 30, 38), new Vector2(2, 0));

            mineTurn = true;
        }

        public override void GrantTurn()
        {
            if (mineTurn)
            {
                List<RaonLauncherMine> ownedMines = LevelScene
                    .MineList.Except(LevelScene.ToBeRemovedMineList)
                    .Where((x) => x.Owner.ID == Owner.ID)
                    .ToList()
                    .ConvertAll((x) => (RaonLauncherMine)x);

                if (ownedMines.Count > 0)
                {
                    ownedMines[0].GrantTurn(ownedMines);
                    mineTurn = false;
                    return;
                }
            }

            mineTurn = true;
            base.GrantTurn();
        }

        public override void PlayUnableToMoveSE(float pitch = 0, float pan = 0)
        {
            base.PlayUnableToMoveSE(pitch: 0.75f);
        }

        protected override void Shoot()
        {
            if (SelectedShotType == ShotType.S1)
                RaonLauncherProjectileEmitter.Shot1(this);
            else if (SelectedShotType == ShotType.S2)
                RaonLauncherProjectileEmitter.Shot2(this);
            else if (SelectedShotType == ShotType.SS)
                LastCreatedProjectileList.Add(new RaonProjectile3(this));

            base.Shoot();
        }
    }
}
