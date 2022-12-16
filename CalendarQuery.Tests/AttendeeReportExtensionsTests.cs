using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ical.Net;

namespace CalendarQuery.Tests
{
    public class AttendeeReportExtensionsTests
    {
        [Test]
        public async Task GetCalendars_WhenFileContainsValidUrls_ThenRetrieveCalendarsFromFile()
        {
            var input    = "SampleData/sample-calendars.txt";
            var contents = await input.GetUrlContentsAsync();

            var calendars = contents.GetCalendars();

            foreach (var (_, calendar) in calendars)
            {
                Assert.IsInstanceOf<Calendar>(calendar);
            }
        }
        
        [Test]
        public async Task FilterByMonth_WhenMonthProvided_ThenOnlyReturnEventsWithinExpectedMonth()
        {
            var sampleIcs = await File.ReadAllTextAsync("SampleData/sample-ics-with-valid-events.ics");
            var calendar  = Calendar.Load(sampleIcs);
            var calendars = new Dictionary<string, Calendar> {{"some-file-name.ics", calendar}};

            var events = calendars
                .SelectMany(i => i.Value.Events)
                .Where(i => i.FilterByMonth(11))
                .ToList();
            
            Assert.That(calendars.First().Value.Events.Count, Is.EqualTo(6)); // Before filter
            Assert.That(events.Count, Is.EqualTo(4)); // After filter
        }
        
        [Test]
        public async Task FilterByAttendees_WhenAttendeeListProvided_ThenOnlyReturnEventsAttendedByAttendee()
        {
            var sampleIcs = await File.ReadAllTextAsync("SampleData/sample-ics-with-valid-events.ics");
            var calendars = new Dictionary<string, Calendar>
            {
                { "some-file.ics", Calendar.Load(sampleIcs) }
            };

            var events = calendars
                .SelectMany(i => i.Value.Events)
                .Where(e => e.FilterByAttendees(new List<string> {"user.one@contoso.com"}))
                .ToList();
            
            Assert.That(calendars.First().Value.Events.Count, Is.EqualTo(6)); // Before filter
            Assert.That(events.Count, Is.EqualTo(2)); // After filter
        }
    }
}
