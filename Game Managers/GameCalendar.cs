using RiverHollow.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace RiverHollow
{
    public static class GameCalendar
    {
        //One day goes from 6 AM - 2 AM => 20 hours
        //Each hour should be one minute
        //Every 10 minutes is 10 seconds real time.
        enum Seasons { Spring, Summer, Winter, Fall};
        private static List<string> ListDays = new List<string>() { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"};
        private static int _dayOfWeek;
        private static int _currHour;
        public static int CurrentHour { get => _currHour; }

        private static int _currMin;
        public static int CurrentMin { get => _currMin; }
        static int _currDay;
        //static Seasons _currSeason;
        static SpriteFont _calendarFont;
        static Vector2 _timePosition;

        static double _lastUpdateinSeconds;

        public static void NewCalendar()
        {
            _dayOfWeek = 0;
            _currDay = 1;
            //_currSeason = Seasons.Spring;
            _currHour = 6;
            _currMin = 0;

            _lastUpdateinSeconds = 0;

            _calendarFont = GameContentManager.GetFont(@"Fonts\Font");
            _timePosition = new Vector2(1760, 800);
        }

        public static void Update(GameTime gameTime)
        {
            _lastUpdateinSeconds += gameTime.ElapsedGameTime.TotalSeconds;
            if(_currHour == 26)
            {
                GUIManager.SetScreen(GUIManager.Screens.DayEnd);
            }
            if (_lastUpdateinSeconds >= 1)
            {
                _lastUpdateinSeconds = 0;
                if (_currMin >= 60)
                {
                    _currMin = 0;
                    _currHour++;
                }
                else
                {
                    _currMin++;
                }
            }

            if (GameCalendar.CurrentHour == 2)
            {
                GUIManager.SetScreen(GUIManager.Screens.DayEnd);
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            int minToFifteen = _currMin / 15;
            string mins = "00";
            string hours = _currHour.ToString();
            if (_currHour > 12 && _currHour < 25)
            {
                hours = (_currHour - 12).ToString();
            }
            else if (_currHour >= 25)
            {
                hours = (_currHour - 24).ToString();
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
            spriteBatch.DrawString(_calendarFont, String.Format("Day {0}, {1}:{2}", _currDay, hours, mins), _timePosition, Color.Black);
        }

        public static void NextDay()
        {
            _currHour = 6;
            _currMin = 0;
            _currDay++;
            if(_dayOfWeek < ListDays.Count - 1) { _dayOfWeek++; }
            else { _dayOfWeek = 0; }

        }

        public static string GetTime()
        {
            return string.Format("{0}:{1}", _currHour, _currMin);
        }

        public static string GetDayAndMods()
        {
            return ListDays[_dayOfWeek];
        }
    }
}
