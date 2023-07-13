using System;
using Microsoft.Xna.Framework;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Utilities;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Game_Managers
{
    public static class GameCalendar
    {
        //One day goes from 6 AM - 2 AM => 20 hours
        //Each hour should be one minute
        //Every 10 minutes is 10 seconds real time.
        public static SeasonEnum CurrentSeason { get; private set; }
        public static DayEnum DayOfWeek { get; private set; }
        public static int CurrentHour { get; private set; }
        public static int CurrentMin { get; private set; }
        public static int CurrentDay { get; private set; }

        public static int CurrentYear { get; private set; }

        private static int _iBedHour = 0;
        private static int _iBedMinute = 0;

        static RHTimer _timer;

        static bool _bHasNightFallen;

        public static void NewCalendar()
        {
            CurrentYear = 1;
            CurrentDay = 1;
            DayOfWeek = DayEnum.Monday;
            CurrentSeason = SeasonEnum.Spring;
            CurrentHour = Constants.CALENDAR_NEW_DAY_HOUR;
            CurrentMin = Constants.CALENDAR_NEW_DAY_MIN;
            _bHasNightFallen = false;

            _timer = new RHTimer(1);

            MapManager.CheckSpirits();
        }

        public static void Update(GameTime gTime)
        {
            if(CurrentHour >= 26)
            {
                GUIManager.SetScreen(new DayEndScreen());
            }
            if (_timer.TickDown(gTime, true))
            {
                IncrementMinutes();
            }

            if (CurrentHour == 2)
            {
                GUIManager.SetScreen(new DayEndScreen());
            }
        }

        public static void IncrementMinutes()
        {
            CurrentMin += Constants.CALENDAR_MINUTES_PER_SECOND;
            if (CurrentMin > 59)
            {
                CurrentMin = 0;
                CurrentHour++;
            }

            if (!_bHasNightFallen && IsNight())
            {
                _bHasNightFallen = true;
                MapManager.CheckSpirits();
            }
        }

        public static void AddTime(int hours, int minutes)
        {
            int totalMinutes = CurrentMin + minutes;
            int hoursOfMinutes = totalMinutes / 60;

            CurrentMin = totalMinutes % 60;
            CurrentHour += hours + hoursOfMinutes;

            if (!_bHasNightFallen && IsNight())
            {
                _bHasNightFallen = true;
                MapManager.CheckSpirits();
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
            CurrentHour = Constants.CALENDAR_NEW_DAY_HOUR;
            CurrentMin = Constants.CALENDAR_NEW_DAY_MIN;
            if (DayOfWeek == DayEnum.Sunday) { DayOfWeek = DayEnum.Monday; }
            else { DayOfWeek++; }

            if(CurrentDay == Constants.CALENDAR_DAYS_IN_MONTH)
            {
                CurrentDay = 1;
                if (CurrentSeason == SeasonEnum.Winter)
                {
                    CurrentSeason = SeasonEnum.Spring;
                    CurrentYear++;
                }
                else
                {
                    CurrentSeason++;
                }
            }
            else { CurrentDay++; }

            EnvironmentManager.RollForWeatherEffects();
        }

        public static int Nightfall()
        {
            switch (CurrentSeason)
            {
                case SeasonEnum.Summer:
                    return Constants.NIGHTFALL_LATE;
                case SeasonEnum.Winter:
                    return Constants.NIGHTFALL_EARLY;
                default:
                    return Constants.NIGHTFALL_STANDARD;
            }
        }
        public static bool IsNight()
        {
            return CurrentHour >= 18;
        }

        public static void GoToNightfall()
        {
            if (CurrentHour < Nightfall())
            {
                CurrentHour = Nightfall();
                CurrentMin = 0;
            }
        }

        public static string GetTime()
        {
            return CurrentHour + ":" + CurrentMin.ToString("00");
        }

        public static string GetCurrentSeason()
        {
            return Util.GetEnumString(CurrentSeason);
        }

        public static string GetSeason(int val)
        {
            return Util.GetEnumString((SeasonEnum)val);
        }

        public static int GetTotalDays()
        {
            return ((int)(CurrentSeason - 1) * 28) + CurrentDay;
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

            rv = (hoursLeftUntilMidnight + Constants.CALENDAR_NEW_DAY_HOUR) * 60 + minutesToNextHour;

            return rv * Constants.CALENDAR_MINUTES_PER_SECOND;
        }

        public static void LoadCalendar(CalendarData d)
        {
            _timer = new RHTimer(Constants.CALENDAR_MINUTES_PER_SECOND);

            CurrentYear = d.currYear;
            CurrentDay = d.dayOfMonth;
            DayOfWeek = (DayEnum)d.dayOfWeek;
            CurrentSeason = (SeasonEnum)d.currSeason;
            _bHasNightFallen = false;

            CurrentHour = Constants.CALENDAR_NEW_DAY_HOUR;
            CurrentMin = Constants.CALENDAR_NEW_DAY_MIN;
        }

        public static CalendarData SaveCalendar()
        {
            return new CalendarData
            {
                dayOfWeek = (int)DayOfWeek,
                dayOfMonth = CurrentDay,
                currSeason = (int)CurrentSeason,
                currYear = CurrentYear
            };
        }
    }
}
