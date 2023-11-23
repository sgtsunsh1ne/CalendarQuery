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
        public void RosteredEvent_CalculatesDaysCorrectly(
            CalendarEvent calendarEvent, 
            string expectedAttendee,
            string expectedAdjustedStartDate,
            string expectedAdjustedEndDate,
            string expectedAdjustedDuration,
            int expectedWeekdayCount,
            int expectedWeekendCount,
            int expectedHolidayCount,
            string monthName,
            IEnumerable<DateTime> holidays)
        {
            var month = DateTime.ParseExact(monthName, "MMMM", CultureInfo.CurrentCulture).Month;
            
            var sut = new RosteredEvent(calendarEvent, month, holidays);

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
            
            Assert.That(sut.WeekdayCount, Is.EqualTo(expectedWeekdayCount));
            Assert.That(sut.WeekendCount, Is.EqualTo(expectedWeekendCount));
            Assert.That(sut.PublicHolidayCount, Is.EqualTo(expectedHolidayCount));
            
            Assert.That(
                sut.WeekdayCount + sut.WeekendCount + sut.PublicHolidayCount, 
                Is.EqualTo(sut.AdjustedDuration.Days));
        }

        public static IEnumerable<object[]> ExpectedRosteredEvents =>
            new List<object[]>
            {
                // Standard 7-day scenario
                new object[]
                {
                    Utility.CalendarEvent("01 Dec 22 00:00 AM", "08 Dec 22 00:00 AM", "standard7days@contoso.com"),
                    "standard7days@contoso.com",
                    "01 Dec 22 00:00 AM",
                    "08 Dec 22 00:00 AM",
                    "7 days",
                    5,
                    2,
                    0,
                    "December",
                    new List<DateTime>()
                },
                
                // If event starts from previous month and ends in current month
                // Then AdjustedDuration is 4 days because StartDateLocal has been adjusted to 1st day of month
                new object[]
                {
                    Utility.CalendarEvent("28 Nov 22 08:30 AM", "05 Dec 22 08:30 AM", "user1@contoso.com"),
                    "user1@contoso.com",
                    "01 Dec 22 00:00 AM",
                    "05 Dec 22 08:30 AM",
                    "4 days",
                    2,
                    2,
                    0,
                    "December",
                    new List<DateTime>()
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
                    5,
                    1,
                    0,
                    "December",
                    new List<DateTime>()
                },
                
                // If attendee worked 6 days 23 hours
                // Then AdjustedDuration is 7 days
                new object[]
                {
                    Utility.CalendarEvent("19 Dec 22 00:00 AM", "25 Dec 22 23:00 PM", "worked_6_days_23_hours@contoso.com"),
                    "worked_6_days_23_hours@contoso.com",
                    "19 Dec 22 00:00 AM",
                    "25 Dec 22 23:00 PM",
                    "7 days",
                    5,
                    2,
                    0,
                    "December",
                    new List<DateTime>()
                },
                
                // If attendee worked 6 days 12 hours
                // Then AdjustedDuration is 7 days
                new object[]
                {
                    Utility.CalendarEvent("19 Dec 22 00:00 AM", "25 Dec 22 12:00 PM", "worked_6_days_12_hours@contoso.com"),
                    "worked_6_days_12_hours@contoso.com",
                    "19 Dec 22 00:00 AM",
                    "25 Dec 22 12:00 PM",
                    "7 days",
                    5,
                    2,
                    0,
                    "December",
                    new List<DateTime>()
                },
                
                // If attendee worked 6 days 11 hours
                // Then AdjustedDuration is 6 days
                new object[]
                {
                    Utility.CalendarEvent("19 Dec 22 00:00 AM", "25 Dec 22 11:00 AM", "worked_6_days_11_hours@contoso.com"),
                    "worked_6_days_11_hours@contoso.com",
                    "19 Dec 22 00:00 AM",
                    "25 Dec 22 11:00 AM",
                    "6 days",
                    5,
                    1,
                    0,
                    "December",
                    new List<DateTime>()
                },
                
                // If attendee worked 11 hours
                // Then AdjustedDuration is 0 days
                new object[]
                {
                    Utility.CalendarEvent("19 Dec 22 00:00 AM", "19 Dec 22 11:59 AM", "worked_11_hours@contoso.com"),
                    "worked_11_hours@contoso.com",
                    "19 Dec 22 00:00 AM", 
                    "19 Dec 22 11:59 AM", 
                    "0 days",
                    0,
                    0,
                    0,
                    "December",
                    new List<DateTime>()
                },
                
                // Public Holiday Tests
                new object[]
                {
                    Utility.CalendarEvent("26 Dec 22 00:00 AM", "12 Jan 23 00:00 AM", "worked_holidays@contoso.com"),
                    "worked_holidays@contoso.com",
                    "26 Dec 22 00:00 AM", 
                    "01 Jan 23 00:00 AM", 
                    "6 days",
                    3,
                    1,
                    2,
                    "December",
                    new List<DateTime>
                    {
                        new(2022, 12, 25),
                        new(2022, 12, 26),
                        new(2022, 12, 27),
                        new(2023, 01, 01),
                        new(2023, 01, 02),
                        new(2023, 01, 03)
                    }
                },
                
                // If attendee started late at night, don't include that night as a DayWorked.
                new object[]
                {
                    Utility.CalendarEvent("17 Jan 23 20:00 PM", "23 Jan 23 00:00 AM", "user1@contoso.com"),
                    "user1@contoso.com",
                    "17 Jan 23 20:00 PM",
                    "23 Jan 23 00:00 AM",
                    "5 days",
                    3,
                    2,
                    0,
                    "January",
                    new List<DateTime>()
                },
                
                // If attendee started late at night _and_ the shift overlaps between two months
                // Then adjust to begin on 1st of the month at midnight
                new object[]
                {
                    Utility.CalendarEvent("28 Nov 22 20:30 PM", "05 Dec 22 08:30 AM", "user1@contoso.com"),
                    "user1@contoso.com",
                    "01 Dec 22 00:00 AM",
                    "05 Dec 22 08:30 AM",
                    "4 days",
                    2,
                    2,
                    0,
                    "December",
                    new List<DateTime>()
                },
                
                // If attendee started just after 12pm, that day doesn't count
                // Reason -- attendee must work >12 hours to qualify for DayWorked
                // Scenario 1
                new object[]
                {
                    Utility.CalendarEvent("17 Jan 23 12:01 PM", "23 Jan 23 00:00 AM", "user1@contoso.com"),
                    "user1@contoso.com",
                    "17 Jan 23 12:01 PM",
                    "23 Jan 23 00:00 AM",
                    "5 days",
                    3,
                    2,
                    0,
                    "January",
                    new List<DateTime>()
                },
                
                // If attendee started just after 12pm, that day doesn't count
                // Reason -- attendee must work >12 hours to qualify for DayWorked
                // Scenario 2
                new object[]
                {
                    Utility.CalendarEvent("17 Jan 23 21:00 PM", "18 Jan 23 03:00 AM", "user1@contoso.com"),
                    "user1@contoso.com",
                    "17 Jan 23 21:00 PM",
                    "18 Jan 23 03:00 AM",
                    "0 days",
                    0,
                    0,
                    0,
                    "January",
                    new List<DateTime>()
                },
                
                // If attendee started just after 12pm, that day doesn't count
                // Reason -- attendee must work >12 hours to qualify for DayWorked
                // Scenario 3
                new object[]
                {
                    Utility.CalendarEvent("17 Jan 23 21:00 PM", "19 Jan 23 00:00 AM", "user1@contoso.com"),
                    "user1@contoso.com",
                    "17 Jan 23 21:00 PM",
                    "19 Jan 23 00:00 AM",
                    "1 day",
                    1,
                    0,
                    0,
                    "January",
                    new List<DateTime>()
                },
                 
                // Midday Test
                new object[]
                {
                    Utility.CalendarEvent("22 Sep 23 12:00 PM", "23 Sep 23 00:00 AM", "midday@contoso.com"),
                    "midday@contoso.com",
                    "22 Sep 23 12:00 PM",
                    "23 Sep 23 00:00 AM",
                    "1 day",
                    1,
                    0,
                    0,
                    "September",
                    new List<DateTime>()
                },
        };
    }
}
