using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Adventure
{
    public static class GameCalendar
    {
        //One day goes from 6 AM - 2 AM => 20 hours
        //Each hour should be one minute
        //Every 10 minutes is 10 seconds real time.
        enum Seasons { Spring, Summer, Winter, Fall};
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
                AdventureGame.ChangeGameState(AdventureGame.GameState.EndOfDay);
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
        }

    }
}
