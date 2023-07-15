using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;
using RiverHollow.Misc;
using static RiverHollow.Misc.EnvironmentalEffect;

namespace RiverHollow.Game_Managers
{
    public static class EnvironmentManager
    {
        static AnimationEnum[] ListWeather = { AnimationEnum.None, AnimationEnum.Rain, AnimationEnum.Snow }; //Thunderstorm?
        static WeatherEnum _eCurrentWeather = WeatherEnum.Sunny;

        private static int _iSeasonPrecipDays = 0;
        private static List<EnvironmentalEffect> _liEnvironmentalEffects;
        private static List<Actor> _liCritters;

        public static void Initialize()
        {
            _liEnvironmentalEffects = new List<EnvironmentalEffect>();
            _liCritters = new List<Actor>();
        }

        public static void Update(GameTime gTime)
        {
            List<EnvironmentalEffect> finished = new List<EnvironmentalEffect>();
            foreach (EnvironmentalEffect e in _liEnvironmentalEffects)
            {
                e.Update(gTime);
                if (e.IsFinished())
                {
                    finished.Add(e);
                }
            }

            foreach (EnvironmentalEffect e in finished)
            {
                _liEnvironmentalEffects.Remove(e);
                AddNewEnvironmentalEffect();
            }
            finished.Clear();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (EnvironmentalEffect e in _liEnvironmentalEffects) {
                e.Draw(spriteBatch);
            }
        }

        public static void LoadEnvironment(RHMap map)
        {
            if ((IsRaining() || IsSnowing()) && map.IsOutside)
            {
                for (int i = 0; i < GetEnvironmentalDensity(); i++)
                {
                    AddNewEnvironmentalEffect();
                }
            }

            if (MapManager.CurrentMap.IsOutside)
            {
                List<RHTile> validTiles = MapManager.CurrentMap.GetAllTiles(true);
                MapManager.CurrentMap.RemoveTilesNearTravelPoints(ref validTiles);

                int num = 0;
                int percent = RHRandom.Instance().Next(100);
                if (percent > 50)
                {
                    if (percent < 70) { num = 1; }
                    if (percent < 85) { num = 2; }
                    if (percent < 95) { num = 3; }
                    if (percent < 100) { num = 4; }
                }

                for (int i = 0; i < num; i++)
                {
                    Critter actor = DataManager.CreateCritter(int.Parse(DataManager.Config[20]["NPC_ID"]));
                    actor.SetPosition(Util.GetRandomItem(validTiles).Position);
                    map.AddActor(actor);
                    _liCritters.Add(actor);
                }
            }
        }

        private static int GetEnvironmentalDensity()
        {
            float tileDensity = IsRaining() ? 0.2f : 1;
            int tiles = MapManager.CurrentMap.MapWidthTiles * MapManager.CurrentMap.MapHeightTiles;

            return (int)(tiles * tileDensity);
        }

        public static void UnloadEnvironment()
        {
            _liEnvironmentalEffects.Clear();
            foreach(Actor actor in _liCritters)
            {
                actor.CurrentMap.RemoveActor(actor);
            }
        }

        public static void AddNewEnvironmentalEffect()
        {
            int width = MapManager.CurrentMap.MapWidthTiles;
            int height = MapManager.CurrentMap.MapHeightTiles;

            if (IsRaining())
            {
                _liEnvironmentalEffects.Add(new Raindrop(width, height));
            }

            if (IsSnowing())
            {
                _liEnvironmentalEffects.Add(new Snowflake(width, height));
            }
        }

        public static bool LightingActive()
        {
            return MapManager.CurrentMap.Lighting < 1 || GameCalendar.CurrentHour >= 18 && MapManager.CurrentMap.IsOutside;
        }

        /// <summary>
        /// Using the time of day, calculate how dark it should be
        /// </summary>
        /// <returns>The Color of the night-time darkness mask</returns>
        public static Color GetAmbientLight()
        {
            float darkPercent = MapManager.CurrentMap.Lighting;

            if (MapManager.CurrentMap.Lighting == 1)
            {
                float totalMinutes = 180;
                float timeModifier = GameCalendar.CurrentMin + ((GameCalendar.CurrentHour - 18f) * 60f);  //Total number of minutes since 6 P.M.
                darkPercent = Math.Min(0.9f, timeModifier / totalMinutes);
            }
            else
            {
                darkPercent = 1 - darkPercent;
            }

            //Subtract the percent of darkness we currently have from the max then subtract
            // it from the max value of 255 to find our relative number. Since new Color takes
            //a float between 0 and 1, we need to divide our relative number by the max
            float value = (255 - (255 * darkPercent)) / 255;
            return new Color(value, value, value);
        }

        #region Weather handling
        public static void RollForWeatherEffects()
        {
            if (RHRandom.Instance().RollPercent(20) || (_iSeasonPrecipDays < Constants.MINIMUM_DAYS_OF_PRECIPITATION && GameCalendar.CurrentDay + _iSeasonPrecipDays - 1 == Constants.CALENDAR_DAYS_IN_MONTH))
            {
                _iSeasonPrecipDays++;
                if (GameCalendar.CurrentSeason == SeasonEnum.Winter)
                {
                    _eCurrentWeather = WeatherEnum.Snowing;
                }
                else
                {
                    _eCurrentWeather = WeatherEnum.Raining;
                }
            }
            else { _eCurrentWeather = WeatherEnum.Sunny; }
        }

        public static bool IsSunny() { return _eCurrentWeather == WeatherEnum.Sunny; }
        public static bool IsRaining() { return _eCurrentWeather == WeatherEnum.Raining; }
        public static bool IsSnowing() { return _eCurrentWeather == WeatherEnum.Snowing; }

        public static string GetWeatherString()
        {
            return Util.GetEnumString(_eCurrentWeather);
        }
        #endregion

        public static void LoadEnvironment(EnvironmentData d)
        {
            _eCurrentWeather = (WeatherEnum)d.currWeather;
            _iSeasonPrecipDays = d.currSeasonPrecipDays;
        }

        public static EnvironmentData SaveEnvironment()
        {
            return new EnvironmentData
            {
                currWeather = (int)_eCurrentWeather,
                currSeasonPrecipDays = _iSeasonPrecipDays
            };
        }
    }
}
