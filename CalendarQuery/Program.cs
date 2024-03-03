using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CalendarQuery.Extensions;
using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;

namespace CalendarQuery
{
    public enum ReportType
    {
        AttendeeSummary,
        AttendeeSummaryVerbose
    }
    
    public class Program : ConsoleAppBase
    {
        private static async Task Main(string[] args)
        {
            await Host
                .CreateDefaultBuilder()
                .RunConsoleAppFrameworkAsync<Program>(args);
        }

        public async Task Run(
            [Option("c", "Calendar(s)   - Accepts single URL, or TXT file containing list of URLs")] string c,
            [Option("m", "Month         - Accepts month, or if none provided, the current month will be used.")] int m,
            [Option("h", "Holiday(s)    - Accepts multiple dates (yyyy-MM-dd) as comma-separated values, or TXT file containing list of dates (yyyy-MM-dd)")] string h,
            [Option("t", "Timezone      - Accepts standard Timezone Names i.e. \"New Zealand Standard Time\", \"Central Standard Time\"")] string t,
            [Option("a", "Attendee(s)   - Accepts single email or TXT file contains list of emails")] string a = "",
            [Option("r", "ReportType    - AttendeeSummary | AttendeeSummaryVerbose")] ReportType r = ReportType.AttendeeSummary,
            [Option("refresh", "Refresh calendars - retrieve *ICS files from URLs again")] bool refresh = false)
        {
            // retrieve data
            var icsFiles = await c.GetUrlContentsAsync();
            var attendees = await a.GetAttendeesAsync();
            var holidays = await h.GetHolidaysAsync();
            var month = m == 0 ? DateTime.Today.Month : m;
            var filePath = GetFilePath(month);
            var timezone = t;
            
            // save data
            await icsFiles.WriteToDiskAsync(filePath, refresh);
            
            // read data
            var fileContents = await filePath.GetFileContentsAsync();
            
            // process data
            var calendars = fileContents.GetCalendars();

            Console.WriteLine($"{calendars.Count} calendar(s) found.");

            foreach (var calendar in calendars)
            {
                var calendarEvents = calendar.Value.Events.Where(i => i.FilterByMonth(month)).ToList();
                
                Console.WriteLine($"{calendar.Value.Name}");
            
                var rosteredEvents = calendarEvents
                    .Select(i => new RosteredEvent(i, month, holidays, timezone));
                
                var midnightsTotal = 0;
            
                foreach (var re in rosteredEvents)
                {
                    if (re.Midnights.Count == 0) continue;
                    
                    Console.WriteLine(
                        $"{re.AdjustedStartDateLocal:dd MMM yyyy HH:mm:ss}, " +
                        $"{re.AdjustedEndDateLocal:dd MMM yyyy HH:mm:ss}, " +
                        $"{re.WeekdayCount} + " +
                        $"{re.WeekendCount} + " +
                        $"{re.PublicHolidayCount} = " +
                        $"{re.WeekdayCount + re.WeekendCount + re.PublicHolidayCount}, " +
                        $"{re.Attendees}");
            
                    midnightsTotal += re.Midnights.Count;
                }
                
                Console.WriteLine($"{midnightsTotal} Total Days");
                Console.WriteLine(string.Empty);
            }
            
            var result = calendars.SelectMany(i => i.Value.Events)
                .Where(i => i.FilterByMonth(month))
                .Where(i => i.FilterByAttendees(attendees))
                .Select(i => new RosteredEvent(i, month, holidays, timezone))
                .GroupBy(i => i.Attendees)
                .Select(attendeeEvents => new AttendeeSummary(attendeeEvents.Key, attendeeEvents))
                .OrderBy(i => i.CalendarName)
                .ThenBy(i => i.Attendee)
                .ToList();
            
            // report data
            result.WriteToCsv(r, $"{filePath}/{r}-{DateTime.Now:yyyyMMddHHmmss}.csv");
        }

        private static string GetFilePath(int month)
        {
            return $"{DateTime.Today.Year}-{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)}";
        }
    }
}
