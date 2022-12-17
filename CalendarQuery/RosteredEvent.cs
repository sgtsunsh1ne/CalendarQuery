using Ical.Net.CalendarComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using CalendarQuery.Extensions;

namespace CalendarQuery
{
    public class RosteredEvent
    {
        private readonly CalendarEvent _calendarEvent;
        private readonly int _month;
        private readonly IEnumerable<DateTime> _holidays;
        
        public RosteredEvent(CalendarEvent calendarEvent, int month, IEnumerable<DateTime> holidays)
        {
            _calendarEvent = calendarEvent;
            _month = month;
            _holidays = holidays;
        }

        public string CalendarName     => _calendarEvent.Calendar.Name;
        public string Attendees        => _calendarEvent.Attendees.SanitiseAttendees();
        public DateTime StartDateLocal => _calendarEvent.Start.AsSystemLocal;
        public DateTime EndDateLocal   => _calendarEvent.End.AsSystemLocal;
        public TimeSpan ActualDuration => _calendarEvent.Duration;
        
        public DateTime AdjustedStartDateLocal
        {
            get
            {
                var startMonthFallsOutsideOfCurrentMonth =
                    StartDateLocal.Month != _month &&
                    StartDateLocal.Month != EndDateLocal.Month;
                
                if (startMonthFallsOutsideOfCurrentMonth)
                {   
                    var firstDayOfCurrentMonth = new DateTime(
                        EndDateLocal.Year,
                        EndDateLocal.Month,
                        1,
                        StartDateLocal.Hour,
                        StartDateLocal.Minute,
                        StartDateLocal.Second);

                    return firstDayOfCurrentMonth;
                }

                return StartDateLocal;
            }
        }
        
        public DateTime AdjustedEndDateLocal
        {
            get
            {
                var endMonthFallsOutsideOfCurrentMonth =
                    EndDateLocal.Month != _month &&
                    EndDateLocal.Month != StartDateLocal.Month;
                
                if (endMonthFallsOutsideOfCurrentMonth)
                {
                    var nextMonth = StartDateLocal.AddMonths(1);
                    
                    var firstDayOfNextMonth = new DateTime(
                        nextMonth.Year,
                        nextMonth.Month,
                        1,
                        EndDateLocal.Hour,
                        EndDateLocal.Minute,
                        EndDateLocal.Second);

                    return firstDayOfNextMonth;
                }

                return EndDateLocal;
            }
        }

        public TimeSpan AdjustedDuration => AdjustedEndDateLocal.Subtract(AdjustedStartDateLocal).RoundToNearestDay();
        public int WeekdayCount          => Weekdays.Count() - WeekdayHolidays.Count();
        public int WeekendCount          => Weekends.Count() - WeekendHolidays.Count();
        public int PublicHolidayCount    => WeekdayHolidays.Count() + WeekendHolidays.Count();

        private IEnumerable<DateTime> DaysWorked    => AdjustedStartDateLocal.GetDates(AdjustedDuration.Days);
        private IEnumerable<string> Weekdays        => DaysWorked.AreWeekdays();
        private IEnumerable<string> Weekends        => DaysWorked.AreWeekends();
        private IEnumerable<string> WeekdayHolidays => _holidays.AreWeekdays().Intersect(Weekdays);
        private IEnumerable<string> WeekendHolidays => _holidays.AreWeekends().Intersect(Weekends);
    }
}
