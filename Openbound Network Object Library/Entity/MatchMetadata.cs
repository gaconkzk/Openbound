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
using Newtonsoft.Json;
using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.Entity.Sync;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Openbound_Network_Object_Library.Entity
{
    public class MatchMetadata
    {
        public SyncMobile CurrentTurnOwner;

        public int WindForce;
        public int WindAngleDegrees { get; set; }

        public float WindAngleRadians => MathHelper.ToRadians(WindAngleDegrees);

        public List<WeatherType> CurrentWeatherList;
        public List<WeatherType> UpcomingWeatherList;
        public int UpcomingWeatherPosition;

        public int TurnCount;
        public int RoundCount;

        [JsonIgnore]
        Map map;

        public MatchMetadata() { }

        public MatchMetadata(Map map)
        {
            this.map = map;

            UpcomingWeatherList = new List<WeatherType>();
            CurrentWeatherList = new List<WeatherType>();

            for (int i = 0; i < 4; i++) EnqueueNextWeather();

            ProcessUpcomingWeather();

        }

        public Vector2 WindForceComponents()
        {
            float waR = WindAngleRadians;
            return new Vector2((float)Math.Cos(waR), (float)Math.Sin(waR)) * WindForce;
        }

        internal void PassTurn(int RoomSize)
        {
            UpcomingWeatherList.Clear();
            if (++TurnCount == RoomSize)
            {
                TurnCount = 0;
                RoundCount++;

                EnqueueNextWeather();
                ProcessUpcomingWeather();
            }
        }

        private void EnqueueNextWeather()
        {
            int val = NetworkObjectParameters.Random.Next(0, map.WeatherPreset.Sum());
            WeatherType newWeather;

            if ((val -= map.Force) < 0) newWeather = WeatherType.Force;
            else if ((val -= map.Tornado) < 0) newWeather = WeatherType.Hurricane;
            else if ((val -= map.Electricity) < 0) newWeather = WeatherType.Electricity;
            else if ((val -= map.Wind) < 0) newWeather = WeatherType.Wind;
            else if ((val -= map.Weakness) < 0) newWeather = WeatherType.Weakness;
            else if ((val -= map.Protection) < 0) newWeather = WeatherType.Protection;
            else if ((val -= map.Ignorance) < 0) newWeather = WeatherType.Ignorance;
            else if ((val -= map.Thor) < 0) newWeather = WeatherType.Thor;
            else if ((val -= map.Mirror) < 0) newWeather = WeatherType.Mirror;
            else newWeather = WeatherType.Random;

            CurrentWeatherList.Add(newWeather);
            UpcomingWeatherList.Add(newWeather);
        }

        private void ProcessUpcomingWeather()
        {
            WeatherType w = CurrentWeatherList[0];
            CurrentWeatherList.RemoveAt(0);

            DisturbWind();

            switch (w)
            {
                case WeatherType.Wind:
                    ChangeWind();
                    return;

                default:
                    return;
            }
        }

        private void DisturbWind()
        {
            if (NetworkObjectParameters.Random.NextDouble() <= NetworkObjectParameters.WeatherWindAngleDisturbanceChance)
                return;

            WindForce = Math.Max(0, (WindForce + NetworkObjectParameters.Random.Next(0, NetworkObjectParameters.WeatherWindForceDisturbance) - NetworkObjectParameters.WeatherWindForceDisturbance / 2));
            WindAngleDegrees += NetworkObjectParameters.Random.Next(0, NetworkObjectParameters.WeatherWindAngleDisturbance) - NetworkObjectParameters.WeatherWindAngleDisturbance / 2;
            WindAngleDegrees = WindAngleDegrees % 360;
        }

        private void ChangeWind()
        {
            WindForce = NetworkObjectParameters.Random.Next(0, NetworkObjectParameters.WeatherMaximumWindForce);
            WindAngleDegrees = NetworkObjectParameters.Random.Next(0, 360);
        }
    }
}
