using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Spectre.Console;
using Calendar = Ical.Net.Calendar;
using CalendarEvent = Ical.Net.CalendarComponents.CalendarEvent;

namespace CalendarQuery.Extensions
{
    public static class AttendeeSummaryExtensions
    {        
        public static Dictionary<string, Calendar> GetCalendars(this Dictionary<string, string> contents)
        {
            return contents.ToDictionary(i => i.Key, i =>
            {
                var calendar = Calendar.Load(i.Value);
                calendar.Name = i.Key;
                return calendar;
            });
        }
        
        public static bool FilterByMonth(this CalendarEvent ev, int month)
        {
            return ev.Start.AsSystemLocal.Month == month || ev.End.AsSystemLocal.Month == month;
        }

        public static bool FilterByAttendees(this CalendarEvent ev, IList<string> attendees)
        {
            return attendees.Count == 0 || 
                   attendees
                       .Select(i => i.ToLowerInvariant())
                       .Contains(ev.Attendees.SanitiseAttendees());
        }

        public static Table GenerateConsoleTable(this IEnumerable<AttendeeSummary> items)
        {
            var scheduleTable = new Table();

            scheduleTable.AddColumns(
                "Attendees",
                "AdjustedStartDate",
                "AdjustedEndDate",
                "AdjustedDuration",
                "Weekdays",
                "Weekends",
                "Holidays",
                "TotalDays",
                "Notes");

            scheduleTable.Columns[4].RightAligned();            
            scheduleTable.Columns[5].RightAligned();            
            scheduleTable.Columns[6].RightAligned();            
            scheduleTable.Columns[7].RightAligned();
            
            foreach (var e in items)
            {
                scheduleTable.AddRow(
                    e.Attendee,
                    e.AdjustedStartDateLocal,
                    e.AdjustedEndDateLocal,
                    e.AdjustedDuration,
                    e.WeekdayTotal.ToString(),
                    e.WeekendTotal.ToString(),
                    e.HolidayTotal.ToString(),
                    e.TotalDays.ToString(),
                    e.Notes
                );
            }

            return scheduleTable;
        }
        
        public static void WriteToConsole(this IEnumerable<AttendeeSummary> items)
        {
            var table = items.GenerateConsoleTable();
            AnsiConsole.Write(table);
        }
        
        private static void WriteAttendeeSummaryToCsv(this IEnumerable<AttendeeSummary> items, string path)
        {
            using var writer = new StreamWriter(path);
            using var csv    = new CsvWriter(writer, CultureInfo.InvariantCulture);

            var records = new List<dynamic>();

            foreach (var e in items)
            {
                dynamic record = new ExpandoObject();

                record.Attendee          = e.Attendee;
                record.CalendarName      = e.CalendarName;
                record.AdjustedStartDate = e.AdjustedStartDateLocal;
                record.AdjustedEndDate   = e.AdjustedEndDateLocal;
                record.Duration          = e.AdjustedDuration;
                record.DurationInDays    = e.NoOfDaysWorked;
                record.Breakdown         = e.Breakdown;
                record.WeekdayTotal      = e.WeekdayTotal;
                record.WeekendTotal      = e.WeekendTotal;
                record.HolidayTotal      = e.HolidayTotal;
                record.TotalDays         = e.WeekdayTotal + e.WeekendTotal + e.HolidayTotal;
                record.Notes             = e.Notes;

                records.Add(record);                
            }

            csv.WriteRecords(records);   
        }
        
        private static void WriteAttendeeSummaryToCsvVerbose(this IEnumerable<AttendeeSummary> items, string path)
        {
            using var writer  = new StreamWriter(path);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            var records = items.ToList();
            csv.WriteRecords(records);   
        }
        
        public static void WriteToCsv(this IEnumerable<AttendeeSummary> items, ReportType reportType, string path)
        {
            switch (reportType)
            {
                case ReportType.AttendeeSummary:
                    items.WriteAttendeeSummaryToCsv(path);
                    break;
                case ReportType.AttendeeSummaryVerbose:
                    items.WriteAttendeeSummaryToCsvVerbose(path);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reportType), reportType, null);
            }
        }

    }
}
