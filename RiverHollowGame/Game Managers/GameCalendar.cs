using System;
using Microsoft.Xna.Framework;
using RiverHollow.GUIComponents.Screens;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow.Game_Managers
{
    public static class GameCalendar
    {
        public const int DAYS_IN_MONTH = 28;
        const int MINUTES_PER_SECOND = 1;
        const int NEW_DAY_HOUR = 6;
        const int NEW_DAY_MIN = 0;

        //One day goes from 6 AM - 2 AM => 20 hours
        //Each hour should be one minute
        //Every 10 minutes is 10 seconds real time.
        static string[] ListSeasons = { "Spring", "Summer", "Fall", "Winter" };
        static string[] ListDays = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        public static int CurrentSeason { get; private set; }
        public static int DayOfWeek { get; private set; }
        public static int CurrentHour { get; private set; }
        public static int CurrentMin { get; private set; }
        public static int CurrentDay { get; private set; }

        private static int _iBedHour = 0;
        private static int _iBedMinute = 0;

        static double _dLastUpdateinSeconds;

        static bool _bHasNightFallen;

        public static void NewCalendar()
        {
            DayOfWeek = 0;
            CurrentDay = 1;
            CurrentSeason = 0;
            CurrentHour = NEW_DAY_HOUR;
            CurrentMin = NEW_DAY_MIN;
            _bHasNightFallen = false;

            _dLastUpdateinSeconds = 0;

            MapManager.CheckSpirits();
        }

        public static void Update(GameTime gTime)
        {
            _dLastUpdateinSeconds += gTime.ElapsedGameTime.TotalSeconds;
            if(CurrentHour == 26)
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
            if (CurrentMin  > 59)
            {
                CurrentMin = 0;
                CurrentHour++;

                if (!_bHasNightFallen && IsNight())
                {
                    _bHasNightFallen = true;
                    MapManager.CheckSpirits();
                }
            }
            else
            {
                CurrentMin += MINUTES_PER_SECOND;
            }
        }

        public static string GetCalendarString()
        {
            int minToFifteen = CurrentMin / 15;
            string mins = "00";
            string hours = CurrentHour.ToString();
            if (CurrentHour > 12 && CurrentHour < 25)
            {
                hours = (CurrentHour - 12).ToString("00");
            }
            else if (CurrentHour >= 25)
            {
                hours = (CurrentHour - 24).ToString("00");
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
            return String.Format("Day {0}, {1}:{2}", CurrentDay.ToString("00"), hours, mins);
        }

        public static void NextDay()
        {
            _iBedHour = CurrentHour;
            _iBedMinute = CurrentMin;

            _bHasNightFallen = false;
            CurrentHour = NEW_DAY_HOUR;
            CurrentMin = NEW_DAY_MIN;
            if(DayOfWeek < ListDays.Length - 1) { DayOfWeek++; }
            else { DayOfWeek = 0; }

            if(CurrentDay == DAYS_IN_MONTH)
            {
                CurrentDay = 1;
                if (CurrentSeason == 3) { CurrentSeason = 0; }
                else { CurrentSeason++; }
            }
            else { CurrentDay++; }

            EnvironmentManager.RollForWeatherEffects();
        }

        public static bool IsNight()
        {
            return CurrentHour >= 18;
        }

        public static string GetTime()
        {
            return CurrentHour + ":" + CurrentMin.ToString("00");
        }

        public static string GetDayOfWeek()
        {
            return ListDays[DayOfWeek];
        }

        public static string GetSeason(int val)
        {
            return ListSeasons[val];
        }

        public static string GetSeason()
        {
            return ListSeasons[CurrentSeason];
        }

        /// <summary>
        /// Returns the number of minutes between the current time and dawn of the next day.
        /// Currently, one minute in game is equal to one second in real time. We are future proofed
        /// in case this changes
        /// </summary>
        public static int GetMinutesToNextMorning()
        {
            int rv = 0;

            int hoursLeftUntilMidnight = 24 > _iBedHour ? (24 - _iBedHour) : 0;
            int minutesToNextHour = 60 - _iBedMinute;

            rv = (hoursLeftUntilMidnight + NEW_DAY_HOUR) * 60 + minutesToNextHour;

            return rv * MINUTES_PER_SECOND;
        }

        public static void LoadCalendar(CalendarData d)
        {
            CurrentDay = d.dayOfMonth;
            DayOfWeek = d.dayOfWeek;
            CurrentSeason = d.currSeason;
            _bHasNightFallen = false;

            CurrentHour = NEW_DAY_HOUR;
            CurrentMin = NEW_DAY_MIN;
        }

        public static CalendarData SaveCalendar()
        {
            return new CalendarData
            {
                dayOfWeek = DayOfWeek,
                dayOfMonth = CurrentDay,
                currSeason = CurrentSeason
            };
        }
    }
}
