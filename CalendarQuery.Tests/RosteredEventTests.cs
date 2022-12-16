using System;
using System.Collections.Generic;
using System.Globalization;
using Ical.Net.CalendarComponents;
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

            var sut = new RosteredEvent(calendarEvent, It.IsAny<int>());
            
            Assert.That(sut.StartDateLocal, Is.EqualTo(calendarEvent.DtStart.AsSystemLocal));
            Assert.That(sut.EndDateLocal, Is.EqualTo(calendarEvent.DtEnd.AsSystemLocal));
            Assert.That(sut.ActualDuration, Is.EqualTo(calendarEvent.Duration));
            Assert.That(sut.Attendees, Is.EqualTo("user.one@contoso.com"));
        }
        
        [TestCaseSource(nameof(ExpectedAdjustedStartDates))]
        public void AdjustedStartDateLocal_DeterminesTheCorrectStartDate(CalendarEvent calendarEvent, DateTime expectedStartDate, string monthName)
        {
            var month = DateTime.ParseExact(monthName, "MMMM", CultureInfo.CurrentCulture).Month;
            
            var ev = new RosteredEvent(calendarEvent, month);
            
            Assert.That(
                ev.AdjustedStartDateLocal.ToString("yyyy-MM-dd HHmmss"),
                Is.EqualTo(expectedStartDate.ToString("yyyy-MM-dd HHmmss")));
        }

        [TestCaseSource(nameof(ExpectedAdjustedEndDates))]
        public void AdjustedEndDateLocal_DeterminesTheCorrectEndDate(CalendarEvent calendarEvent, DateTime expectedEndDate, string monthName)
        {
            var month = DateTime.ParseExact(monthName, "MMMM", CultureInfo.CurrentCulture).Month;
            
            var ev = new RosteredEvent(calendarEvent, month);
            
            Assert.That(
                ev.AdjustedEndDateLocal.ToString("yyyy-MM-dd HHmmss"),
                Is.EqualTo(expectedEndDate.ToString("yyyy-MM-dd HHmmss")));
        }

        public static IEnumerable<object[]> ExpectedAdjustedStartDates =>
            new List<object[]>
            {
                new object[]
                { 
                    // If event starts from previous month and ends in current month
                    // Then adjust start date to 1st day of current month
                    Utility.CalendarEvent("15 Nov 22 10:00 AM", "24 Dec 22 10:00 AM"),
                    DateTime.Parse("01 Dec 22 10:00 AM"),
                    "December"
                },
                new object[]
                { 
                    // If event starts and ends in current month
                    // Then start date requires no adjustment
                    Utility.CalendarEvent("19 Dec 22 10:30 AM", "24 Dec 22 10:00 AM"),
                    DateTime.Parse("19 Dec 22 10:30 AM"),
                    "December"
                },
                new object[]
                { 
                    // If event starts in current month and ends next month
                    // Then start date requires no adjustment
                    Utility.CalendarEvent("19 Dec 22 10:30 AM", "24 Jan 22 10:00 AM"),
                    DateTime.Parse("19 Dec 22 10:30 AM"),
                    "December"
                },
                new object[]
                {
                    // Midnight test
                    Utility.CalendarEvent("20 Nov 22 00:00 AM", "24 Dec 22 00:00 AM"),
                    DateTime.Parse("01 Dec 22 00:00 AM"),
                    "December"
                },
                new object[]
                {
                    // Leap year test
                    Utility.CalendarEvent("20 Jan 20 08:00 AM", "15 Feb 20 08:30 AM"),
                    DateTime.Parse("01 Feb 20 08:00 AM"),
                    "February"
                }
            };
        
        
        public static IEnumerable<object[]> ExpectedAdjustedEndDates =>
            new List<object[]>
            {
                new object[]
                {
                    // If event starts and ends in current month
                    // Then end date requires no adjustment
                    Utility.CalendarEvent("19 Dec 22 10:00 AM", "24 Dec 22 10:00 AM"),
                    DateTime.Parse("24 Dec 22 10:00 AM"), 
                    "December"
                },
                new object[]
                {
                    // If event starts from previous month and ends in current month
                    // Then end date requires no adjustment
                    Utility.CalendarEvent("20 Nov 22 08:30 AM", "24 Dec 22 08:30 AM"),
                    DateTime.Parse("24 Dec 22 08:30 AM"),
                    "December"
                },
                new object[]
                {
                    // If event starts from current month and ends in next month
                    // Then end date adjusted to end on first day of next month
                    Utility.CalendarEvent("20 Nov 22 08:30 AM", "24 Dec 22 08:30 AM"),
                    DateTime.Parse("01 Dec 22 08:30 AM"),
                    "November"
                },
                new object[]
                {
                    // Midnight test
                    Utility.CalendarEvent("20 Nov 22 00:00 AM", "24 Dec 22 00:00 AM"),
                    DateTime.Parse("01 Dec 22 00:00 AM"),
                    "November"
                },
                new object[]
                {
                    // Leap year test
                    Utility.CalendarEvent("20 Jan 20 08:00 AM", "15 Feb 20 08:30 AM"),
                    DateTime.Parse("01 Feb 20 08:30 AM"),
                    "January"
                }
            };
    }
}
