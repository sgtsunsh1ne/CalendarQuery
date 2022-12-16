using System;
using System.Collections.Generic;
using System.Globalization;
using Humanizer;
using Humanizer.Localisation;
using Ical.Net.CalendarComponents;

namespace CalendarQuery.Tests
{
    public class RosteredEventTests
    {
        [TestCaseSource(nameof(ExpectedRosteredEvents))]
        public void RosteredEvent_ExtractsDataFromCalendarEventCorrectly(
            CalendarEvent calendarEvent, 
            string expectedAttendee,
            string expectedAdjustedStartDate,
            string expectedAdjustedEndDate,
            string expectedAdjustedDuration,
            string monthName)
        {
            var month = DateTime.ParseExact(monthName, "MMMM", CultureInfo.CurrentCulture).Month;
            
            var sut = new RosteredEvent(calendarEvent, month);

            Assert.That(sut.StartDateLocal, Is.EqualTo(calendarEvent.DtStart.AsSystemLocal));
            Assert.That(sut.EndDateLocal, Is.EqualTo(calendarEvent.DtEnd.AsSystemLocal));
            Assert.That(sut.ActualDuration, Is.EqualTo(calendarEvent.Duration));
            Assert.That(sut.Attendees, Is.EqualTo(expectedAttendee));
            
            Assert.That(
                sut.AdjustedStartDateLocal.ToString("dd MMM yy HH:mm tt"),  
                Is.EqualTo(expectedAdjustedStartDate));
            
            Assert.That(
                sut.AdjustedEndDateLocal.ToString("dd MMM yy HH:mm tt"),  
                Is.EqualTo(expectedAdjustedEndDate));

            Assert.That(
                sut.AdjustedDuration.Humanize(maxUnit:TimeUnit.Day, minUnit:TimeUnit.Day),
                Is.EqualTo(expectedAdjustedDuration));
        }

        public static IEnumerable<object[]> ExpectedRosteredEvents =>
            new List<object[]>
            {
                // Standard 7-day scenario
                new object[]
                {
                    Utility.CalendarEvent("01 Dec 22 00:00 AM", "08 Dec 22 00:00 AM", "user1@contoso.com"),
                    "user1@contoso.com",
                    "01 Dec 22 00:00 AM",
                    "08 Dec 22 00:00 AM",
                    "7 days",
                    "December"
                },
                
                // If event starts from previous month and ends in current month
                // Then AdjustedDuration is 4 days because StartDateLocal has been adjusted to 1st day of month
                new object[]
                {
                    Utility.CalendarEvent("28 Nov 22 08:30 AM", "05 Dec 22 08:30 AM", "user1@contoso.com"),
                    "user1@contoso.com",
                    "01 Dec 22 08:30 AM",
                    "05 Dec 22 08:30 AM",
                    "4 days",
                    "December"
                },
                
                // If event starts in current month and ends in next month
                // Then AdjustedDuration is 6 days because EndDateLocal has been adjusted to 1st day of month
                new object[]
                {
                    Utility.CalendarEvent("26 Dec 22 08:30 AM", "02 Jan 23 08:30 AM", "user1@contoso.com"),
                    "user1@contoso.com",
                    "26 Dec 22 08:30 AM",
                    "01 Jan 23 08:30 AM",
                    "6 days",
                    "December"
                },
                
                // If attendee worked 6 days 23 hours
                // Then AdjustedDuration is 7 days
                new object[]
                {
                    Utility.CalendarEvent("19 Dec 22 00:00 AM", "25 Dec 22 23:00 PM", "user1@contoso.com"),
                    "user1@contoso.com",
                    "19 Dec 22 00:00 AM",
                    "25 Dec 22 23:00 PM",
                    "7 days",
                    "December"
                },
                
                // If attendee worked 6 days 12 hours
                // Then AdjustedDuration is 7 days
                new object[]
                {
                    Utility.CalendarEvent("19 Dec 22 00:00 AM", "25 Dec 22 12:00 PM", "user1@contoso.com"),
                    "user1@contoso.com",
                    "19 Dec 22 00:00 AM",
                    "25 Dec 22 12:00 PM",
                    "7 days",
                    "December"
                },
                
                // If attendee worked 6 days 11 hours
                // Then AdjustedDuration is 6 days
                new object[]
                {
                    Utility.CalendarEvent("19 Dec 22 00:00 AM", "25 Dec 22 11:00 AM", "user1@contoso.com"),
                    "user1@contoso.com",
                    "19 Dec 22 00:00 AM",
                    "25 Dec 22 11:00 AM",
                    "6 days",
                    "December"
                },
                
                // If attendee worked 11 hours
                // Then AdjustedDuration is 0 days
                new object[]
                {
                    Utility.CalendarEvent("19 Dec 22 00:00 AM", "19 Dec 22 11:00 AM", "user1@contoso.com"),
                    "user1@contoso.com",
                    "19 Dec 22 00:00 AM",
                    "19 Dec 22 11:00 AM",
                    "0 days",
                    "December"
                }
            };
    }
}
