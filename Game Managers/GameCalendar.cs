using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Adventure
{
    public class GameCalendar
    {
        private static GameCalendar _instance;
        //One day goes from 6 AM - 2 AM => 20 hours
        //Each hour should be one minute
        //Every 10 minutes is 10 seconds real time.
        enum Seasons { Spring, Summer, Winter, Fall};
        private static int _currHour;
        public static int CurrentHour { get => _currHour; }

        private static int _currMin;
        public static int CurrentMin { get => _currMin; }
        static int _currDay;
        static Seasons _currSeason;
        static SpriteFont _calendarFont;
        static Vector2 _timePosition;

        static double _lastUpdateinSeconds;

        private GameCalendar()
        {
            _currDay = 1;
            _currSeason = Seasons.Spring;
            _currHour = 6;
            _currMin = 0;

            _lastUpdateinSeconds = 0;

            _calendarFont = GameContentManager.GetInstance().GetFont(@"Fonts\Font");
            _timePosition = new Vector2(1760, 800);
        }

        public static GameCalendar GetInstance()
        {
            if (_instance == null)
            {
                _instance = new GameCalendar();
            }
            return _instance;
        }

        public static void Update(GameTime gameTime)
        {
            _lastUpdateinSeconds += 5;// gameTime.ElapsedGameTime.TotalSeconds;
            if(_currHour == 26)
            {
                _currHour = 6;
                _currMin = 0;
                _currDay++;
            }
            if (_lastUpdateinSeconds >= 1)
            {
                _lastUpdateinSeconds = 0;
                if (_currMin >= 60)
                {
                    _currMin = 0;
                    if (_currMin == 12) { _currMin = 1; }
                    else { _currHour++; }
                }
                else
                {
                    _currMin++;
                }
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            int minToFifteen = _currMin / 15;
            string mins = "00";
            string hours = (_currHour > 12) ? (_currHour-12).ToString() : _currHour.ToString();
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
                    hours = (_currHour + 1).ToString();
                    break;
            }
            spriteBatch.DrawString(_calendarFont, String.Format("Day {0}, {1}:{2}", _currDay, hours, mins), _timePosition, Color.Black);
        }

    }
}
