using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.DataTypes;

namespace CalendarQuery.Extensions
{
    public static class RosteredEventExtensions
    {
        public static string SanitiseAttendees(this ICollection<Attendee> attendees)
        {
            return string.Join(", ", attendees
                .Select(i => i.Value.OriginalString)
                .Select(i => i.Replace("mailto:", string.Empty)));
        }

        public static TimeSpan RoundToNearestDay(this TimeSpan span, MidpointRounding mode = MidpointRounding.AwayFromZero)
        {
            return TimeSpan.FromHours(Math.Round(span.TotalHours / 24, 0, mode) * 24);
        }

        public static IEnumerable<DateTime> GetDates(this DateTime startDate, int days)
        {
            var dates = new List<DateTime>();

            for (var i = 0; i < days; i++)
            {
                var dt = startDate.AddDays(i);
                dates.Add(dt);
            }

            return dates;   
        }

        public static IEnumerable<string> AreWeekdays(this IEnumerable<DateTime> dates)
        {
            return dates
                .Where(d => d.DayOfWeek is
                    DayOfWeek.Monday or
                    DayOfWeek.Tuesday or
                    DayOfWeek.Wednesday or
                    DayOfWeek.Thursday or
                    DayOfWeek.Friday)
                .Select(i => i.ToString("yyyy-MM-dd"));
        }
        
        public static IEnumerable<string> AreWeekends(this IEnumerable<DateTime> dates)
        {
            return dates
                .Where(d => d.DayOfWeek is
                    DayOfWeek.Saturday or
                    DayOfWeek.Sunday)
                .Select(i => i.ToString("yyyy-MM-dd"));
        }
    }
}
