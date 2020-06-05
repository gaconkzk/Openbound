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
using OpenBound.GameComponents.Pawn.Unit;
using System.Collections.Generic;
using System.Linq;

namespace OpenBound.GameComponents.WeatherEffect
{
    public enum WeatherEffectType
    {
        Electricity,
        Force,
        LightningSES1,
        LightningSES2,
        Mirror,
        Random,
        Tornado,
        Weakness
    }

    public class WeatherHandler
    {
        /// <summary>
        /// List of all instanced weathers on the scenario. Do not add elements in here. In order to add new elements and prevent
        /// access/overwriting problems use <see cref="WeatherHandler.Add"/>.
        /// </summary>
        public List<Weather> WeatherList;

        //List of weathers that are going to be removed
        List<Weather> unusedWeatherList;

        public WeatherHandler()
        {
            WeatherList = new List<Weather>();
            unusedWeatherList = new List<Weather>();
        }

        public void Add(WeatherEffectType weatherEffectType, Vector2 position, float rotation = 0)
        {
            Weather weather = null;

            switch (weatherEffectType)
            {
                case WeatherEffectType.Tornado:
                    weather = new Tornado(position, 1);
                    break;
                case WeatherEffectType.LightningSES1:
                    weather = new EletricityStrike(position, 1);
                    break;
            }

            CheckAndMergeWeatherEffects(ref weather);
        }

        /// <summary>
        /// Check if there is an existing weather and if this weather can be merged together. The merging happen if
        /// their (existing and new weather) collider overlap.
        /// </summary>
        /// <param name="weather">The weather to be added on the list.</param>
        public void CheckAndMergeWeatherEffects(ref Weather weather)
        {
            bool hasMerged;

            do
            {
                hasMerged = false;
                foreach (Weather w in WeatherList.Except(unusedWeatherList))
                {
                    if (w.WeatherEffectType == weather.WeatherEffectType && w.Intersects(weather) && w != weather)
                    {
                        weather = w.Merge(weather);
                        unusedWeatherList.Add(w);
                        hasMerged = true;
                    }
                }
            } while (hasMerged);

            WeatherList.Add(weather);
        }

        /// <summary>
        /// Removes a weather from the list/scene.
        /// </summary>
        public void RemoveWeather(WeatherEffectType weatherEffectType)
        {
            unusedWeatherList.Add(WeatherList.Find((x) => x.WeatherEffectType == weatherEffectType));
        }

        public void Update(GameTime gameTime)
        {
            WeatherList.ForEach((x) => x.Update(gameTime));

            unusedWeatherList.ForEach((x) => WeatherList.Remove((x)));
            unusedWeatherList.Clear();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            WeatherList.ForEach((x) => x.Draw(gameTime, spriteBatch));
        }
    }
}
