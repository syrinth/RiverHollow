using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.EnvironmentalEffect;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow.Game_Managers
{
    public static class EnvironmentManager
    {
        static int MIN_PRECIPITATION_DAYS = 6;

        static AnimationEnum[] ListWeather = { AnimationEnum.None, AnimationEnum.Rain, AnimationEnum.Snow }; //Thunderstorm?
        static WeatherEnum _eCurrentWeather = WeatherEnum.Sunny;

        private static int _iSeasonPrecipDays = 0;
        private static List<EnvironmentalEffect> _liEnvironmentalEffects;

        public static void Initialize()
        {
            _liEnvironmentalEffects = new List<EnvironmentalEffect>();
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
                for (int i = 0; i < 2000; i++)
                {
                    AddNewEnvironmentalEffect();
                }
            }
        }

        public static void UnloadEnvironment()
        {
            _liEnvironmentalEffects.Clear();
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

        /// <summary>
        /// Using the time of day, calculate how dark it should be
        /// </summary>
        /// <returns>The Color of the night-time darkness mask</returns>
        public static Color GetAmbientLight()
        {
            Color rv = Color.White;
            float totalMinutes = 180;
            float timeModifier = GameCalendar.CurrentMin + ((GameCalendar.CurrentHour - 18f) * 60f);  //Total number of minutes since 6 P.M.
            float darkPercent = Math.Min(0.9f, timeModifier / totalMinutes);

            //Subtract the percent of darkness we currently have from the max then subtract
            // it from the max value of 255 to find our relative number. Since new Color takes
            //a float between 0 and 1, we need to divide our relative number by the max
            float value = (255 - (255 * darkPercent)) / 255;
            rv = new Color(value, value, value);
            return rv;
        }

        #region Weather handling
        public static void RollForWeatherEffects()
        {
            int roll = RHRandom.Instance().Next(1, 5);
            if (roll > 2 || (_iSeasonPrecipDays < MIN_PRECIPITATION_DAYS && GameCalendar.CurrentDay + _iSeasonPrecipDays - 1 == GameCalendar.DAYS_IN_MONTH))
            {
                _iSeasonPrecipDays++;
                if (GameCalendar.CurrentSeason == 0) { _eCurrentWeather = WeatherEnum.Raining; }
                else if (GameCalendar.CurrentSeason == 3) { _eCurrentWeather = WeatherEnum.Snowing; }

                MapManager.ApplyWeather();
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

    public abstract class EnvironmentalEffect
    {
        protected AnimatedSprite _sprBody;

        public virtual void Update(GameTime gTime) { }
        public void Draw(SpriteBatch spriteBatch)
        {
            _sprBody.Draw(spriteBatch, 999999999);
        }

        public virtual bool IsFinished()
        {
            return false;
        }

        public class Raindrop : EnvironmentalEffect
        {
            int _iFallDistance = 0;
            public Raindrop(int mapWidth, int mapHeight)
            {
                _iFallDistance = RHRandom.Instance().Next(50, 100);
                _sprBody = new AnimatedSprite(DataManager.FOLDER_ENVIRONMENT + "Rain");
                _sprBody.AddAnimation(AnimationEnum.Action_One, 0, 0, TileSize, TileSize * 2);
                _sprBody.AddAnimation(AnimationEnum.Action_Finished, TileSize, 0, TileSize, TileSize * 2, 2, 0.1f, false, true);
                _sprBody.PlayAnimation(AnimationEnum.Action_One);

                int screenBuffer = TileSize * 3;
                Vector2 pos = new Vector2(RHRandom.Instance().Next(0 - screenBuffer, (mapWidth * TileSize) + screenBuffer), RHRandom.Instance().Next(0 - screenBuffer, (mapHeight * TileSize) + screenBuffer));
                _sprBody.Position = pos;
            }

            public override void Update(GameTime gTime)
            {
                if (_sprBody.IsCurrentAnimation(AnimationEnum.Action_One))
                {
                    Vector2 landingPos = _sprBody.Position + new Vector2(0, TileSize);
                    RHTile landingTile = MapManager.CurrentMap.GetTileByPixelPosition(landingPos);
                    if (landingTile == null) { _sprBody.PlayAnimation(AnimationEnum.Action_Finished); }//_sprBody.Drawing = false; }
                    else if (_iFallDistance <= 0 && landingTile.WorldObject == null)
                    {
                        _sprBody.PlayAnimation(AnimationEnum.Action_Finished);
                    }
                    else
                    {
                        int modifier = RHRandom.Instance().Next(2, 3);
                        _iFallDistance -= modifier;
                        _sprBody.Position += new Vector2(-2 * modifier, 3 * modifier);
                    }
                }

                _sprBody.Update(gTime);
            }

            public override bool IsFinished()
            {
                return !_sprBody.Drawing;
            }
        }

        public class Snowflake : EnvironmentalEffect
        {
            readonly int _iMaxHeight = 0;
            public Snowflake(int mapWidth, int mapHeight)
            {
                float frameLength = RHRandom.Instance().Next(3, 5) / 10f;
                _iMaxHeight = mapHeight * ScaledTileSize;
                _sprBody = new AnimatedSprite(DataManager.FOLDER_ENVIRONMENT + "Snow");
                _sprBody.AddAnimation(AnimationEnum.Action_One, 0, 0, TileSize, TileSize, 3, frameLength, true);
                _sprBody.PlayAnimation(AnimationEnum.Action_One);

                Vector2 pos = new Vector2(RHRandom.Instance().Next(0, mapWidth * ScaledTileSize), RHRandom.Instance().Next(0, mapHeight * ScaledTileSize));
                _sprBody.Position = pos;
            }

            public override void Update(GameTime gTime)
            {
                _sprBody.Position += new Vector2(0, 2);

                _sprBody.Update(gTime);
            }

            public override bool IsFinished()
            {
                return _sprBody.Position.Y >= _iMaxHeight;
            }
        }
    }
}
