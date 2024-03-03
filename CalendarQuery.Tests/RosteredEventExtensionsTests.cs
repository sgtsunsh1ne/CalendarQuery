using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CalendarQuery.Extensions;
using Ical.Net.DataTypes;

namespace CalendarQuery.Tests
{
    public class RosteredEventExtensionsTests
    {
        [TestCaseSource(nameof(AttendeeData))]
        public void SanitiseAttendees_RemovesMailToPrefix_AndConvertsMultipleAttendeesIntoCommaSeparatedList(
            ICollection<Attendee> attendees, string expectedAttendees)
        {
            var result = attendees.SanitiseAttendees();
            Assert.That(result, Is.EquivalentTo(expectedAttendees));
        }
        
        public static IEnumerable<object[]> AttendeeData =>
            new List<object[]>
            {
                new object[]
                {
                    new List<Attendee>
                    {
                        new("mailto:user.one@contoso.com"),
                        new("mailto:user.two@contoso.com")
                    },
                    "user.one@contoso.com, user.two@contoso.com"
                },
                new object[]
                {
                    new List<Attendee>
                    {
                        new("mailto:user.one@contoso.com")
                    },
                    "user.one@contoso.com"
                }
            };

        [TestCaseSource(nameof(DurationTestData))]
        public void RoundToNearestDay_RoundsCorrectly(TimeSpan actualTimeSpan, TimeSpan expectedTimeSpan)
        {
            var adjustedTimeSpan = actualTimeSpan.RoundToNearestDay();
            Assert.That(adjustedTimeSpan, Is.EqualTo(expectedTimeSpan));
        }

        public static IEnumerable<object[]> DurationTestData =>
            new List<object[]>
            {
                // 6 days 23 hours = 7 days
                new object[] {new TimeSpan(6, 23, 0, 0), new TimeSpan(7, 0, 0, 0)},
                
                // 6 days 2 hours = 6 days
                new object[] {new TimeSpan(6, 02, 0, 0), new TimeSpan(6, 0, 0, 0)},
                
                // 3 days 12 hours = 4 days
                new object[] {new TimeSpan(3, 12, 0, 0), new TimeSpan(4, 0, 0, 0)},
                
                // 3 days 11 hours = 3 days
                new object[] {new TimeSpan(3, 11, 0, 0), new TimeSpan(3, 0, 0, 0)},
            };
        

        [Test]
        [TestCase("2022-11-21", 07, 2)]    // Start Mon + 07 days = End Sun
        [TestCase("2022-11-21", 14, 4)]    // Start Mon + 14 days = End Sun
        [TestCase("2022-11-21", 13, 3)]    // Start Mon + 13 days = End Sat
        [TestCase("2022-11-30", 04, 1)]    // Start Wed + 04 days = End Sat
        [TestCase("2022-12-03", 04, 2)]    // Start Sat + 04 days = End Tue
        [TestCase("2022-12-02", 01, 0)]    // Start Fri + 01 days = End Sat
        public void AreWeekends_CountWeekendsCorrectly(string dt, int days, int expectedWeekendCount)
        {
            var dates = DateTime.Parse(dt).GetDates(days);
            var weekendCount = dates.AreWeekends().Count();
            Assert.That(weekendCount, Is.EqualTo(expectedWeekendCount));
        }
        
        [Test]
        [TestCase("2022-11-21", 07, 05)]    // Start Mon + 07 days = End Sun
        [TestCase("2022-11-21", 14, 10)]    // Start Mon + 14 days = End Sun
        [TestCase("2022-11-21", 13, 10)]    // Start Mon + 13 days = End Sat
        [TestCase("2022-11-30", 04, 03)]    // Start Wed + 04 days = End Sat
        [TestCase("2022-12-03", 04, 02)]    // Start Sat + 04 days = End Tue
        [TestCase("2022-12-02", 01, 01)]    // Start Fri + 01 days = End Sat
        public void AreWeekdays_CountWeekdaysCorrectly(string dt, int days, int expectedWeekdayCount)
        {
            var dates = DateTime.Parse(dt).GetDates(days);
            var weekdayCount = dates.AreWeekdays().Count();
            Assert.That(weekdayCount, Is.EqualTo(expectedWeekdayCount));
        }

        [TestCase("23 Feb 2024 13:00","24 Feb 2024 10:00", 0, 1)]
        [TestCase("24 Feb 2024 10:00","26 Feb 2024 00:00", 0, 1)]
        [TestCase("19 Feb 2024 00:00","23 Feb 2024 13:00", 5, 0)]
        [TestCase("19 Feb 2024 00:00","26 Feb 2024 00:00", 5, 2)]
        [TestCase("26 Feb 2024 10:00","01 Mar 2024 10:00", 4, 0)]
        public void GetMidnights_CalculatesWeekdaysAndWeekendsCorrectly(
            string start, string end, 
            int expectedWeekDays, int expectedWeekends)
        {
            var startDate = DateTime.ParseExact(start, "dd MMM yyyy HH:mm", new CultureInfo("en-NZ"));
            var endDate = DateTime.ParseExact(end, "dd MMM yyyy HH:mm", new CultureInfo("en-NZ"));
            
            var result = RosteredEventExtensions.GetMidnights(startDate, endDate);

            var weekdays = result.AreWeekdays().Count();
            var weekends = result.AreWeekends().Count();
            
            Assert.That(weekdays, Is.EqualTo(expectedWeekDays));
            Assert.That(weekends, Is.EqualTo(expectedWeekends));
        }
    }
}
