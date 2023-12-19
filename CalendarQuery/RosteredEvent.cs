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
        private readonly string _timezone;
        private readonly int _shiftHour;
        
        public RosteredEvent(
            CalendarEvent calendarEvent, 
            int month, 
            IEnumerable<DateTime> holidays,
            string timezone,
            int shiftHour)
        {
            _calendarEvent = calendarEvent;
            _month = month;
            _holidays = holidays;
            _timezone = timezone;
            _shiftHour = shiftHour;
        }

        public string CalendarName     => _calendarEvent.Calendar.Name;
        public string Attendees        => _calendarEvent.Attendees.SanitiseAttendees();

        public DateTime StartDateLocal => TimeZoneInfo
            .ConvertTimeFromUtc(_calendarEvent.Start.AsUtc, TimeZoneInfo.FindSystemTimeZoneById(_timezone));

        public DateTime EndDateLocal => TimeZoneInfo
            .ConvertTimeFromUtc(_calendarEvent.End.AsUtc, TimeZoneInfo.FindSystemTimeZoneById(_timezone));

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
                    // If shift overlaps between two months, then
                    // Only start counting from first day of the month
                    var firstDayOfCurrentMonth = new DateTime(
                        EndDateLocal.Year,
                        EndDateLocal.Month,
                        1,
                        _shiftHour,
                        0,
                        0);

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

                    var startHour = EndDateLocal.Hour < _shiftHour ? EndDateLocal.Hour : _shiftHour;
                    
                    var firstDayOfNextMonth = new DateTime(
                        nextMonth.Year,
                        nextMonth.Month,
                        1,
                        startHour,
                        0,
                        0);

                    return firstDayOfNextMonth;
                }

                return EndDateLocal;
            }
        }

        public TimeSpan AdjustedDuration => AdjustedEndDateLocal.Subtract(AdjustedStartDateLocal);
        public TimeSpan AdjustedDurationToNearestDay => AdjustedDuration.RoundToNearestDay();
        public int WeekdayCount          => Weekdays.Count() - WeekdayHolidays.Count();
        public int WeekendCount          => Weekends.Count() - WeekendHolidays.Count();
        public int PublicHolidayCount    => WeekdayHolidays.Count() + WeekendHolidays.Count();
            
        public bool StartedAfterMidday   => AdjustedStartDateLocal > new DateTime(
                                                AdjustedStartDateLocal.Year,
                                                AdjustedStartDateLocal.Month,
                                                AdjustedStartDateLocal.Day,
                                                12, 
                                                0, 
                                                0);

        private IEnumerable<DateTime> DaysWorked
        {
            get
            {
                // Bug Fix to handle scenario where people start their shift after 12pm
                //
                // To qualify for working on a given day, they have to work at least 12 hours.
                //
                // E.g. If person starts working anytime _after_ Sunday 12 PM, then
                //      (1) Exclude Sunday as their DayWorked (they don't get the weekend)
                //      (2) Start counting from Monday
                //
                // E.g. If person's shift starts at 10PM, that day is excluded from DaysWorked.
                
                if (StartedAfterMidday)
                {
                    var newStartDate = AdjustedStartDateLocal.AddDays(1);
                    var dt = new DateTime(
                        newStartDate.Year,
                        newStartDate.Month,
                        newStartDate.Day,
                        0, 
                        0, 
                        0);
                    
                    return dt.GetDates(AdjustedDurationToNearestDay.Days);
                }
                
                return AdjustedStartDateLocal.GetDates(AdjustedDurationToNearestDay.Days);
            }
        }
        private IEnumerable<string> Weekdays        => DaysWorked.AreWeekdays();
        private IEnumerable<string> Weekends        => DaysWorked.AreWeekends();
        private IEnumerable<string> WeekdayHolidays => _holidays.AreWeekdays().Intersect(Weekdays);
        private IEnumerable<string> WeekendHolidays => _holidays.AreWeekends().Intersect(Weekends);
    }
}
