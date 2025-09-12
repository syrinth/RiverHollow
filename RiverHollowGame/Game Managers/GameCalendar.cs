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

        public static int CurrentWeek => (CurrentDay / 8) + 1;

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
            int minuteBreakdown = (int)(Math.Round(CurrentMin / Constants.CALENDER_MINUTE_INCREMENT) * Constants.CALENDER_MINUTE_INCREMENT);
            if (minuteBreakdown == 60)
            {
                minuteBreakdown = 0;
            }

            string hours = CurrentHour.ToString("00");
            if (CurrentHour > 12 && CurrentHour < 25)
            {
                hours = (CurrentHour - 12).ToString("00");
            }
            else if (CurrentHour >= 25)
            {
                hours = (CurrentHour - 24).ToString("00");
            }

            return String.Format("Day {0}, {1}:{2}", CurrentDay.ToString("00"), hours, minuteBreakdown.ToString("00"));
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

        public static bool IsNight()
        {
            int nightFall = 0;
            switch (CurrentSeason)
            {
                case SeasonEnum.Summer:
                    nightFall = Constants.NIGHTFALL_LATE;
                    break;
                case SeasonEnum.Winter:
                    nightFall = Constants.NIGHTFALL_EARLY;
                    break;
                default:
                    nightFall = Constants.NIGHTFALL_STANDARD;
                    break;
            }
            return CurrentHour >= nightFall;
        }

        public static string GetTime()
        {
            return CurrentHour.ToString("00") + ":" + CurrentMin.ToString("00");
        }

        public static bool TimeBetween(int min, int max)
        {
            return CurrentHour >= min && CurrentHour <= max;
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

        public static bool CompareDate(int year, int season, int day)
        {
            bool rv = false;
            if (CalendarDataEquals(CurrentYear, year, ref rv))
            {
                if (CalendarDataEquals((int)CurrentSeason, season, ref rv))
                {
                    if (CurrentDay >= day)
                    {
                        rv = true;
                    }
                }
            }

            return rv;
        }

        /// <summary>
        /// Compares any Calendar integer data against a given comparator. Used to simplify the if statement checks above
        /// </summary>
        /// <param name="data">The Calendar data to check</param>
        /// <param name="valueToCompare">The value to compare against</param>
        /// <param name="returnValue">The value to return for whether the value is greater or less than</param>
        /// <returns>True if the two values are equal</returns>
        private static bool CalendarDataEquals(int data, int valueToCompare, ref bool returnValue)
        {
            bool rv = false;

            if (data > valueToCompare) { returnValue = true; }
            else if (data < valueToCompare) { returnValue = false; }
            else
            {
                rv = true;
            }

            return rv;
        }

        public static void LoadCalendar(CalendarData d)
        {
            _timer = new RHTimer(1);

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
