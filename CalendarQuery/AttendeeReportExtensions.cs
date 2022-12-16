using System.Collections.Generic;
using System.Linq;
using Ical.Net;

namespace CalendarQuery
{
    public static class AttendeeReportExtensions
    {
        public static Dictionary<string, Calendar> GetCalendars(this Dictionary<string, string> contents)
        {
            return contents.ToDictionary(i => i.Key, i => Calendar.Load(i.Value));
        }
        
        public static IEnumerable<RosteredEvent> GetRosteredEvents(this KeyValuePair<string, Calendar> kvp)
        {
            var (_, calendar) = kvp;
            return calendar.Events.Select(ev => new RosteredEvent(ev));
        }
    }
}
