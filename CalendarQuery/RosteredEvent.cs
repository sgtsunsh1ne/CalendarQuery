using Ical.Net.CalendarComponents;
using System;

namespace CalendarQuery
{
    public class RosteredEvent
    {
        private readonly CalendarEvent _calendarEvent;
        
        public RosteredEvent(CalendarEvent calendarEvent)
        {
            _calendarEvent = calendarEvent;
        }

        public DateTime StartDateLocal => _calendarEvent.Start.AsSystemLocal;
        public DateTime EndDateLocal => _calendarEvent.End.AsSystemLocal;
        public TimeSpan ActualDuration => _calendarEvent.Duration;
        public string Attendees => _calendarEvent.Attendees.SanitiseAttendees();
    }
}
