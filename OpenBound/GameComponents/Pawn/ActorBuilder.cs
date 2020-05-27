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
using OpenBound.GameComponents.Pawn.Unit;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Models;

namespace OpenBound.GameComponents.Pawn
{
    public class ActorBuilder
    {
        public static Mobile BuildMobile(MobileType mobileType, Player player, Vector2 position)
        {
            switch (mobileType)
            {
                case MobileType.Random:  return new Random(player, position);

                case MobileType.Armor:   return new Armor(player, position);
                case MobileType.Bigfoot: return new Bigfoot(player, position);
                case MobileType.Dragon:  return new Dragon(player, position);
                case MobileType.Mage:    return new Mage(player, position);
                case MobileType.Ice:     return new Ice(player, position);
                case MobileType.Knight:  return new Knight(player, position);
                case MobileType.Trico:   return new Trico(player, position);
                case MobileType.Turtle:  return new Turtle(player, position);
            }

            return null;
        }
    }
}
