using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.EnvironmentalEffect;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Game_Managers
{
    public static class EnvironmentManager
    {
        

        static AnimationEnum[] ListWeather = { AnimationEnum.None, AnimationEnum.Rain, AnimationEnum.Snow }; //Thunderstorm?
        static WeatherEnum _eCurrentWeather = WeatherEnum.Sunny;

        private static int _iSeasonPrecipDays = 0;
        private static List<EnvironmentalEffect> _liEnvironmentalEffects;
        private static List<WorldActor> _liCritters;

        public static void Initialize()
        {
            _liEnvironmentalEffects = new List<EnvironmentalEffect>();
            _liCritters = new List<WorldActor>();
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

                for (int i = 0; i < 10; i++)
                {
                    Critter actor = DataManager.CreateCritter(int.Parse(DataManager.Config[20]["NPC_ID"]));
                    actor.Position = Util.GetRandomItem(validTiles).Position;
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
            foreach(WorldActor actor in _liCritters)
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
            Color rv = Color.White;

            float darkPercent = MapManager.CurrentMap.Lighting;

            if (MapManager.CurrentMap.Lighting == 1)
            {
                float totalMinutes = 180;
                float timeModifier = GameCalendar.CurrentMin + ((GameCalendar.CurrentHour - 18f) * 60f);  //Total number of minutes since 6 P.M.
                darkPercent = Math.Min(0.9f, timeModifier / totalMinutes);
            }

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
            if (roll > 2 || (_iSeasonPrecipDays < Constants.MINIMUM_DAYS_OF_PRECIPITATION && GameCalendar.CurrentDay + _iSeasonPrecipDays - 1 == Constants.CALENDAR_DAYS_IN_MONTH))
            {
                _iSeasonPrecipDays++;
                if (GameCalendar.CurrentSeason == 0) { _eCurrentWeather = WeatherEnum.Raining; }
                else if (GameCalendar.CurrentSeason == 3) { _eCurrentWeather = WeatherEnum.Snowing; }
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
            _sprBody.Draw(spriteBatch, Constants.MAX_LAYER_DEPTH);
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
                _sprBody.AddAnimation(AnimationEnum.Action1, 0, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2);
                _sprBody.AddAnimation(AnimationEnum.Action_Finished, Constants.TILE_SIZE, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.1f, false, true);
                _sprBody.PlayAnimation(AnimationEnum.Action1);

                int top = (0 - 400);
                int bottom = (mapWidth * Constants.TILE_SIZE) + _iFallDistance;
                int left = (0 - _iFallDistance);
                int right = (mapWidth * Constants.TILE_SIZE) + _iFallDistance;
                Vector2 pos = new Vector2(RHRandom.Instance().Next(0, (mapWidth * Constants.TILE_SIZE) + 300), RHRandom.Instance().Next(-400, mapHeight * Constants.TILE_SIZE));
                //Vector2 pos = new Vector2(300, -300);
                _sprBody.Position = pos;
            }

            public override void Update(GameTime gTime)
            {
                if (_sprBody.IsCurrentAnimation(AnimationEnum.Action1))
                {
                    Vector2 landingPos = _sprBody.Position + new Vector2(0, Constants.TILE_SIZE);
                    RHTile landingTile = MapManager.CurrentMap.GetTileByPixelPosition(landingPos);
                    //if (landingTile == null) { _sprBody.PlayAnimation(CombatAnimationEnum.Action_Finished); }//_sprBody.Drawing = false; }
                    if (_iFallDistance <= 0 && (landingTile == null || landingTile.WorldObject == null || landingTile.WorldObject.CompareType(ObjectTypeEnum.Structure)))
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
                _sprBody.AddAnimation(AnimationEnum.Action1, 0, 0, Constants.TILE_SIZE, Constants.TILE_SIZE, 3, frameLength, true);
                _sprBody.PlayAnimation(AnimationEnum.Action1);

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
