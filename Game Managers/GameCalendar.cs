using RiverHollow.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using RiverHollow.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Misc;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIObjects.GUIObject;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow
{
    public static class GameCalendar
    {
        static int DAYS_IN_MONTH = 28;
        static int MIN_PRECIPITATION_DAYS = 6;

        //One day goes from 6 AM - 2 AM => 20 hours
        //Each hour should be one minute
        //Every 10 minutes is 10 seconds real time.
        static string[] ListSeasons = { "Spring", "Summer", "Fall", "Winter" };
        static string[] ListDays = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        static AnimationEnum[] ListWeather = { AnimationEnum.None, AnimationEnum.Rain, AnimationEnum.Snow }; //Thunderstorm?

        static int _iCurrSeason;
        public static int CurrentSeason { get => _iCurrSeason; }
        static int _iDayOfWeek;
        public static int DayOfWeek { get => _iDayOfWeek; }
        static int _iCurrHour;
        public static int CurrentHour { get => _iCurrHour; }
        static int _iCurrWeather;
        public static int CurrentWeather { get => _iCurrWeather; }

        private static int _iCurrMin;
        public static int CurrentMin { get => _iCurrMin; }
        static int _iCurrDay;
        public static int CurrentDay { get => _iCurrDay; }

        static GUIWindow _displayWindow;
        static GUIText _text;

        static double _dLastUpdateinSeconds;
        static int _iSeasonPrecipDays = 0;

        static bool _bNightfall;

        public static void NewCalendar()
        {
            _iDayOfWeek = 0;
            _iCurrDay = 1;
            _iCurrSeason = 0;
            _iCurrHour = 6;
            _iCurrMin = 0;
            _bNightfall = false;

            _dLastUpdateinSeconds = 0;

            SetDisplayWindow();

            _iCurrWeather = 0;
            //RollForWeatherEffects();

            MapManager.CheckSpirits();
        }

        private static void SetDisplayWindow()
        {
            _text = new GUIText("Day XX, XX:XX", DataManager.GetFont(@"Fonts\Font"));

            Vector2 boxSize = _text.MeasureString() + new Vector2(GUIWindow.BrownWin.Edge * 2, GUIWindow.BrownWin.Edge * 2);
            _displayWindow = new GUIWindow(GUIWindow.BrownWin, (int)boxSize.X, (int)boxSize.Y);
            _displayWindow.AnchorToScreen(SideEnum.TopRight, 10);

            _text.CenterOnWindow(_displayWindow);
        }

        public static void Update(GameTime gTime)
        {
            _dLastUpdateinSeconds += gTime.ElapsedGameTime.TotalSeconds;
            if(_iCurrHour == 26)
            {
                GUIManager.SetScreen(new DayEndScreen());
            }
            if (_dLastUpdateinSeconds >= 1)
            {
                _dLastUpdateinSeconds = 0;
                IncrementMinutes();
            }

            if (GameCalendar.CurrentHour == 2)
            {
                GUIManager.SetScreen(new DayEndScreen());
            }
        }

        public static void IncrementMinutes()
        {
            if (_iCurrMin  > 59)
            {
                _iCurrMin = 0;
                _iCurrHour++;

                if (!_bNightfall && IsNight())
                {
                    _bNightfall = true;
                    MapManager.CheckSpirits();
                }
            }
            else
            {
                _iCurrMin ++;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            int minToFifteen = _iCurrMin / 15;
            string mins = "00";
            string hours = _iCurrHour.ToString();
            if (_iCurrHour > 12 && _iCurrHour < 25)
            {
                hours = (_iCurrHour - 12).ToString();
            }
            else if (_iCurrHour >= 25)
            {
                hours = (_iCurrHour - 24).ToString();
            }
            switch (minToFifteen)
            {
                case 1:
                    mins = "15";
                    break;
                case 2:
                    mins = "30";
                    break;
                case 3:
                    mins = "45";
                    break;
                case 4:
                    mins = "00";
                    hours = (int.Parse(hours) + 1).ToString();
                    break;
            }
            _displayWindow.Draw(spriteBatch);
            _text.Draw(spriteBatch, String.Format("Day {0}, {1}:{2}", _iCurrDay, hours, mins));
            //spriteBatch.DrawString(_calendarFont, String.Format("Day {0}, {1}:{2}", _currDay, hours, mins), _displayWindow.InnerTopLeft(), Color.Black);
        }

        public static void NextDay()
        {
            _bNightfall = false;
            _iCurrHour = 6;
            _iCurrMin = 0;
            if(_iDayOfWeek < ListDays.Length - 1) { _iDayOfWeek++; }
            else { _iDayOfWeek = 0; }

            if(_iCurrDay == DAYS_IN_MONTH)
            {
                _iCurrDay = 1;
                if (_iCurrSeason == 3) { _iCurrSeason = 0; }
                else { _iCurrSeason++; }
            }
            else { _iCurrDay++; }

            //RollForWeatherEffects();
        }

        private static void RollForWeatherEffects()
        {
            int roll = RHRandom.Instance.Next(1, 5);
            if(roll > 2 || (_iSeasonPrecipDays < MIN_PRECIPITATION_DAYS && _iCurrDay + _iSeasonPrecipDays - 1 == DAYS_IN_MONTH))
            {
                _iSeasonPrecipDays++;
                if (_iCurrSeason == 0) { _iCurrWeather = 1; }
                else if(_iCurrSeason == 3) { _iCurrWeather = 2; }

                MapManager.SetWeather(ListWeather[_iCurrWeather]);
            }
            else { _iCurrWeather = 0; }
        }

        public static bool IsSunny()
        {
            return _iCurrWeather == 0;
        }
        public static bool IsRaining()
        {
            return _iCurrWeather == 1;
        }
        public static bool IsSnowing()
        {
            return _iCurrWeather == 2;
        }

        public static bool IsNight()
        {
            return _iCurrHour >= 18;
        }

        public static string GetTime()
        {
            return _iCurrHour + ":" + _iCurrMin.ToString("00");
        }

        public static string GetDayOfWeek()
        {
            return ListDays[_iDayOfWeek];
        }

        public static string GetSeason()
        {
            return ListSeasons[_iCurrSeason];
        }

        public static string GetWeatherString()
        {
            return Util.GetEnumString(ListWeather[_iCurrWeather]);
        }

        public static void LoadCalendar(CalendarData d)
        {
            SetDisplayWindow();
            _iCurrDay = d.dayOfMonth;
            _iDayOfWeek = d.dayOfWeek;
            _iCurrSeason = d.currSeason;
            _iCurrWeather = d.currWeather;
            _iSeasonPrecipDays = d.currSeasonPrecipDays;
            _bNightfall = false;

            _iCurrHour = 6;
            _iCurrMin = 0;
        }

        /// <summary>
        /// Using the time of day, calculate how dark it should be
        /// </summary>
        /// <returns>The Color of thenight-time darkness mask</returns>
        public static Color GetLightColor()
        {
            Color rv = Color.White;
            if(_iCurrHour >= 18)
            {
                int totalMinutes = 360;
                float timeModifier = _iCurrMin + ((_iCurrHour - 18) * 60);  //Total number of minutes since 6 P.M.
                float darkPercent = timeModifier / totalMinutes;

                //Subtract the percent of darkness we currently have from the max then subtract
                // it from the max value of 255 to find our relative number. Since new Color takes
                //a float between 0 and 1, we need to divide our relative number by the max
                float value = (255 - (255 *  (0.5f * darkPercent))) / 255;  
                rv = new Color(value, value, value);
            }

            return rv;
        }

        public static CalendarData SaveCalendar()
        {
            return new CalendarData
            {
                dayOfWeek = _iDayOfWeek,
                dayOfMonth = _iCurrDay,
                currSeason = _iCurrSeason,
                currWeather = _iCurrWeather,
                currSeasonPrecipDays = _iSeasonPrecipDays
            };
        }
    }
}
