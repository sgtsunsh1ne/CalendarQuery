﻿using System;
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
            [Option("c", "Calendar(s) - Accepts single URL, or TXT file containing list of URLs")] string c,
            [Option("m", "Month       - Accepts month, or if none provided, the current month will be used.")] int m = 0,
            [Option("a", "Attendee(s) - Accepts single email or TXT file contains list of emails")] string a = "",
            [Option("h", "Holiday(s)  - Accepts multiple dates (yyyy-MM-dd) as comma-separated values")] string h = "",
            [Option("r", "ReportType  - AttendeeSummary | AttendeeSummaryVerbose")] ReportType r = ReportType.AttendeeSummary,
            [Option("refresh", "Refresh calendars - retrieve *ICS files from URLs again")] bool refresh = false)
        {
            // retrieve data
            var icsFiles = await c.GetUrlContentsAsync();
            var attendees = await a.GetAttendeesAsync();
            var holidays = await h.GetHolidaysAsync();
            var month = m == 0 ? DateTime.Today.Month : m;
            var filePath = GetFilePath(month);
            
            // save data
            await icsFiles.WriteToDiskAsync(filePath, refresh);
            
            // read data
            var fileContents = await filePath.GetFileContentsAsync();
            
            // process data
            var attendeeSummaryReport = fileContents
                .GetCalendars()
                .SelectMany(i => i.Value.Events)
                .Where(i => i.FilterByMonth(month))
                .Where(i => i.FilterByAttendees(attendees))
                .Select(i => new RosteredEvent(i, month, holidays))
                .GroupBy(i => i.Attendees)
                .Select(attendeeEvents => new AttendeeSummary(attendeeEvents.Key, attendeeEvents))
                .ToList();
            
            // report data
            attendeeSummaryReport.WriteToConsole();
            attendeeSummaryReport.WriteToCsv(r, $"{filePath}/{r}-{DateTime.Now:yyyyMMddHHmmss}.csv");
        }

        private static string GetFilePath(int month)
        {
            return $"{DateTime.Today.Year}-{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)}";
        }
    }
}
