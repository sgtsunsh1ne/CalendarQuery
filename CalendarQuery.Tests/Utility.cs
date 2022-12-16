using System;
using System.Globalization;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace CalendarQuery.Tests
{
    public static class Utility
    {
        public static CalendarEvent CalendarEvent(string start, string end)
        {
            const string DateFormat = "dd MMM yy HH:mm tt";
            
            var startDate = DateTime.ParseExact(start, DateFormat, CultureInfo.CurrentCulture);
            var endDate   = DateTime.ParseExact(end, DateFormat, CultureInfo.CurrentCulture);
            
            return new CalendarEvent
            {
                Start = new CalDateTime(startDate),
                End = new CalDateTime(endDate)
            };
        }
    }
}
