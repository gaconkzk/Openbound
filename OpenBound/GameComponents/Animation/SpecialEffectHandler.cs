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
using System;
using System.Collections.Generic;

namespace OpenBound.GameComponents.Animation
{
    public class SpecialEffectHandler
    {
        private static List<SpecialEffect> toBeAddedSpecialEffect;
        private static List<SpecialEffect> specialEffectList;
        private static List<SpecialEffect> specialEffectToBeDestroyed;

        public static void Initialize()
        {
            toBeAddedSpecialEffect = new List<SpecialEffect>();
            specialEffectList = new List<SpecialEffect>();
            specialEffectToBeDestroyed = new List<SpecialEffect>();
        }

        public static void Add(SpecialEffect specialEffect)
        {
            lock (toBeAddedSpecialEffect)
            {
                toBeAddedSpecialEffect.Add(specialEffect);
            }
        }

        public static void Remove(SpecialEffect specialEffect)
        {
            specialEffectToBeDestroyed.Add(specialEffect);
        }

        public static void Remove(List<SpecialEffect> specialEffectList)
        {
            specialEffectToBeDestroyed.AddRange(specialEffectList);
        }

        public static void Update(GameTime gameTime)
        {
            lock (toBeAddedSpecialEffect)
            {
                specialEffectList.AddRange(toBeAddedSpecialEffect);
                toBeAddedSpecialEffect.Clear();
            }

            specialEffectToBeDestroyed.ForEach((x) => specialEffectList.Remove(x));
            specialEffectToBeDestroyed.Clear();

            specialEffectList.ForEach((x) => x.Update(gameTime));
        }

        public static void Draw(GameTime GameTime, SpriteBatch SpriteBatch)
        {
            specialEffectList.ForEach((x) => x.Flipbook.Draw(GameTime, SpriteBatch));
        }
    }
}
