using System;
using System.Collections.Generic;
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
    }
}
