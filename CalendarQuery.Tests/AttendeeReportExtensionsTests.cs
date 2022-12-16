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
            var input = "SampleData/sample-calendars.txt";
            var contents = await input.GetUrlContentsAsync();
            
            var calendars = contents.GetCalendars();
    
            foreach (var (_, calendar) in calendars)
            {
                Assert.IsInstanceOf<Calendar>(calendar);
            }
        }

        [Test]
        public async Task GetRosteredEvents_ConvertsCalendarEventsToRosteredEvents()
        {
            var input = "SampleData/sample-calendars.txt";
            var contents = await input.GetUrlContentsAsync();
            
            var rosteredEvents = contents
                .GetCalendars()
                .SelectMany(i => i.GetRosteredEvents())
                .ToList();
            
            foreach (var ev in rosteredEvents)
            {
                Assert.IsInstanceOf<RosteredEvent>(ev);
            }
        }
    }
}
