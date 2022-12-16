using System.Collections.Generic;
using System.Linq;
using Ical.Net.DataTypes;

namespace CalendarQuery
{
    public static class RosteredEventExtensions
    {
        public static string SanitiseAttendees(this ICollection<Attendee> attendees)
        {
            return string.Join(", ", attendees
                .Select(i => i.Value.OriginalString)
                .Select(i => i.Replace("mailto:", string.Empty)));
        }
    }
}
