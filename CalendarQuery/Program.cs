using System;
using System.Linq;
using System.Threading.Tasks;
using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;

namespace CalendarQuery
{
    public class Program : ConsoleAppBase
    {
        private static async Task Main(string[] args)
        {
            await Host
                .CreateDefaultBuilder()
                .RunConsoleAppFrameworkAsync<Program>(args);
        }
        
        public async Task Run(
            [Option("c", "Calendar(s) - Accepts single URL, or TXT file containing list of URLs")] string c,
            [Option("m", "Month       - Accepts month, or if none provided, the current month will be used.")] int m = 0,
            [Option("a", "Attendee(s) - Accepts single email or TXT file contains list of emails")] string a = "",
            [Option("h", "Holiday(s)  - Accepts multiple dates (yyyy-MM-dd) as comma-separated values")] string h = "")
        {
            var contents = await c.GetUrlContentsAsync();
            var month = m == 0 ? DateTime.Today.Month : m;
            var attendees = await a.GetAttendeesAsync();
            var holidays = await h.GetHolidaysAsync();
            var filePath = DateTime.Now.ToString("yyyy-MM-dd-HHmmss");

            var rosteredEvents = contents.GetCalendars()
                .SelectMany(i => i.Value.Events)
                .Where(i => i.FilterByMonth(month))
                .Where(i => i.FilterByAttendees(attendees))
                .Select(i => new RosteredEvent(i, month, holidays));
            
            contents.WriteToDisk(filePath);
            rosteredEvents.WriteToConsole();
        }
    }
}
