using System.Collections.Generic;
using System.Linq;
using Ical.Net;
using Ical.Net.CalendarComponents;

namespace CalendarQuery.Extensions
{
    public static class AttendeeSummaryExtensions
    {        
        public static Dictionary<string, Calendar> GetCalendars(this Dictionary<string, string> contents)
        {
            return contents.ToDictionary(i => i.Key, i =>
            {
                var calendar = Calendar.Load(i.Value);
                calendar.Name = i.Key;
                return calendar;
            });
        }
        
        public static bool FilterByMonth(this CalendarEvent ev, int month)
        {
            return ev.Start.AsSystemLocal.Month == month || ev.End.AsSystemLocal.Month == month;
        }

        public static bool FilterByAttendees(this CalendarEvent ev, IList<string> attendees)
        {
            return attendees.Count == 0 || attendees.Contains(ev.Attendees.SanitiseAttendees());
        }
    }
}