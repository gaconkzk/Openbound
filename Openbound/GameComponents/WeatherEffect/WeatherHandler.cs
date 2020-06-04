using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBound.GameComponents.WeatherEffect
{
    public enum WeatherEffectType
    {
        Electricity,
        Force,
        LightningSE,
        Mirror,
        Random,
        Tornado,
        Weakness
    }

    public class WeatherHandler
    {
        public List<Weather> WeatherList;
        List<Weather> unusedWeatherList;

        public WeatherHandler()
        {
            WeatherList = new List<Weather>();
            unusedWeatherList = new List<Weather>();
        }

        public void Add(WeatherEffectType weatherEffectType, Vector2 position)
        {
            Weather weather = null;

            switch (weatherEffectType)
            {
                case WeatherEffectType.Tornado:
                    weather = new Tornado(position, 1);
                    break;
            }

            CheckAndMergeWeatherEffects(ref weather);
        }

        public void CheckAndMergeWeatherEffects(ref Weather weather)
        {
            bool hasMerged;

            do
            {
                hasMerged = false;
                foreach (Weather w in WeatherList.Except(unusedWeatherList))
                {
                    if (w.GetType() == weather.GetType() && w.Intersects(weather) && w != weather)
                    {
                        weather = w.Merge(weather);
                        unusedWeatherList.Add(w);
                        hasMerged = true;
                    }
                }
            } while (hasMerged);

            WeatherList.Add(weather);
        }

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
