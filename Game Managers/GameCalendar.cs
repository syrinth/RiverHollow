using RiverHollow.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using RiverHollow.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;

namespace RiverHollow
{
    public static class GameCalendar
    {
        //One day goes from 6 AM - 2 AM => 20 hours
        //Each hour should be one minute
        //Every 10 minutes is 10 seconds real time.
        static string[] ListSeasons = { "Spring", "Summer", "Fall", "Winter" };
        static string[] ListDays = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        static string[] ListWeather = { "Sunny", "Raining" };
        static int _currSeason;
        public static int CurrentSeason { get => _currSeason; }
        static int _dayOfWeek;
        public static int DayOfWeek { get => _dayOfWeek; }
        static int _currWeather;
        public static int CurrentWeather { get => _currWeather; }
        static int _currHour;
        public static int CurrentHour { get => _currHour; }

        private static int _currMin;
        public static int CurrentMin { get => _currMin; }
        static int _currDay;
        public static int CurrentDay { get => _currDay; }
        //static Seasons _currSeason;

        static GUIWindow _displayWindow;
        static GUIText _text;


        static double _lastUpdateinSeconds;

        public static void NewCalendar()
        {
            _dayOfWeek = 0;
            _currDay = 1;
            //_currSeason = Seasons.Spring;
            _currHour = 6;
            _currMin = 0;

            _lastUpdateinSeconds = 0;

            _text = new GUIText("Day XX, XX:XX", GameContentManager.GetFont(@"Fonts\Font"));
            Vector2 boxSize = _text.MeasureString() + new Vector2(GUIWindow.BrownWin.Edge*2, GUIWindow.BrownWin.Edge*2);

            _displayWindow = new GUIWindow(GUIWindow.BrownWin, (int)boxSize.X, (int)boxSize.Y);
            _displayWindow.AnchorToScreen(GUIObject.SideEnum.TopRight, 10);
            
            _text.CenterOnWindow(_displayWindow);
        }

        public static void Update(GameTime gameTime)
        {
            _lastUpdateinSeconds += gameTime.ElapsedGameTime.TotalSeconds;
            if(_currHour == 26)
            {
                GUIManager.SetScreen(new DayEndScreen());
            }
            if (_lastUpdateinSeconds >= 1)
            {
                _lastUpdateinSeconds = 0;
                IncrementMinutes();
            }

            if (GameCalendar.CurrentHour == 2)
            {
                GUIManager.SetScreen(new DayEndScreen());
            }
        }

        public static void IncrementMinutes()
        {
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
            _displayWindow.Draw(spriteBatch);
            _text.Draw(spriteBatch, String.Format("Day {0}, {1}:{2}", _currDay, hours, mins));
            //spriteBatch.DrawString(_calendarFont, String.Format("Day {0}, {1}:{2}", _currDay, hours, mins), _displayWindow.InnerTopLeft(), Color.Black);
        }

        public static void NextDay()
        {
            _currHour = 6;
            _currMin = 0;
            _currDay++;
            if(_dayOfWeek < ListDays.Length - 1) { _dayOfWeek++; }
            else { _dayOfWeek = 0; }

        }

        public static string GetTime()
        {
            return string.Format("{0}:{1}", _currHour, _currMin);
        }

        public static string GetDay()
        {
            return ListDays[_dayOfWeek];
        }

        public static string GetSeason()
        {
            return ListSeasons[_currSeason];
        }

        public static string GetWeather()
        {
            return ListWeather[_currWeather];
        }

        public static void RollOver()
        {
            _dayOfWeek++;
            if(_dayOfWeek == 7)
            {
                _dayOfWeek = 0;
            }
        }

        public static void LoadCalendar(GameManager.CalendarData d)
        {
            _currDay = d.dayOfMonth;
            _dayOfWeek = d.dayOfWeek;
            _currSeason = d.currSeason;
            _currWeather = d.currWeather;
        }
    }
}
