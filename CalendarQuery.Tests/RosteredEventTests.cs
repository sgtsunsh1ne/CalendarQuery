using System.Collections.Generic;
using Ical.Net.DataTypes;
using Moq;

namespace CalendarQuery.Tests
{
    public class RosteredEventTests
    {
        [Test]
        public void RosteredEvent_ExtractsDataFromCalendarEventCorrectly()
        {
            var calendarEvent = Utility.CalendarEvent("01 Dec 22 00:00 AM", "10 Dec 22 00:00 AM");
            calendarEvent.Attendees = new List<Attendee> {new("mailto:user.one@contoso.com")};

            var sut = new RosteredEvent(calendarEvent);
            
            Assert.AreEqual(calendarEvent.DtStart.AsSystemLocal, sut.StartDateLocal);
            Assert.AreEqual(calendarEvent.DtEnd.AsSystemLocal, sut.EndDateLocal);
            Assert.AreEqual(calendarEvent.Duration, sut.ActualDuration);
            Assert.AreEqual("user.one@contoso.com", sut.Attendees);
        }
    }
}
