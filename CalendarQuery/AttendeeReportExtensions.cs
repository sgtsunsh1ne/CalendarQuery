using System.Collections.Generic;
using System.Linq;
using Ical.Net;
using Ical.Net.CalendarComponents;

namespace CalendarQuery
{
    public static class AttendeeReportExtensions
    {
        public static Dictionary<string, Calendar> GetCalendars(this Dictionary<string, string> contents)
        {
            return contents.ToDictionary(i => i.Key, i => Calendar.Load(i.Value));
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
