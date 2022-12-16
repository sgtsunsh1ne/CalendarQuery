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
            Assert.AreEqual(expectedAttendees, result);
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
    }
}
