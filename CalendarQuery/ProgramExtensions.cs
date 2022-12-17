using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using CsvHelper;
using EmailValidation;
using Spectre.Console;

namespace CalendarQuery
{   
    public static class ProgramExtensions
    {
        public static async Task<Dictionary<string, string>> GetUrlContentsAsync(this string input)
        {   
            var urls = await input.GetUrlsAsync();
            
            var contents = new Dictionary<string, string>();

            foreach (var url in urls)
            {
                var urlResponse = await url.GetAsync();
                var content = await urlResponse.GetStringAsync();
                var fileName = GetFileName(url, urlResponse);
                
                contents.Add(fileName, content);
            }

            return contents;
        }
        
        private static async Task<IEnumerable<string>> GetUrlsAsync(this string input)
        {
            if (Uri.IsWellFormedUriString(input, UriKind.Absolute))
            {
                return new[] { input };
            }

            if (File.Exists(input))
            {
                var lines = await File.ReadAllLinesAsync(input);
                return lines.Where(url => Uri.IsWellFormedUriString(url, UriKind.Absolute));
            }

            return Array.Empty<string>();
        }

        private static string GetFileName(string url, IFlurlResponse response)
        {
            if (response.TryGetFileNameFromResponseHeaders(out var filename))
            {
                return filename;
            }

            if (url.TryGetFileNameFromUri(out filename))
            {
                return filename;
            }
            
            throw new ArgumentException($"Unable to determine filename from {url}");
        }

        private static bool TryGetFileNameFromResponseHeaders(this IFlurlResponse response, out string filename)
        {
            filename = string.Empty;
            
            var (_, value) = response.Headers.FirstOrDefault(i => i.Name == "Content-Disposition");

            if (value is null)
            {
                return false;
            }

            var contentDisposition = new ContentDisposition(value);
            filename = contentDisposition.FileName ?? string.Empty;
            return !string.IsNullOrEmpty(filename);
        }

        private static bool TryGetFileNameFromUri(this string url, out string filename)
        {
            filename = string.Empty;

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                filename = Path.GetFileName(uri.LocalPath);
            }       
            
            return !string.IsNullOrEmpty(filename);
        }
        
        public static void WriteToDisk(this Dictionary<string, string> contents, string path)
        {
            Directory.CreateDirectory(path);
            foreach (var (filename, content) in contents)
            {
                File.WriteAllTextAsync($"{path}/{filename}", content);
            }
        }
        
        
        public static async Task<IList<string>> GetAttendeesAsync(this string input)
        {   
            if (EmailValidator.Validate(input))
            {
                return new[] { input };
            }

            if (!File.Exists(input))
            {
                return Array.Empty<string>();
            }

            var lines = await File.ReadAllLinesAsync(input);
            
            return lines
                .Where(email => EmailValidator.Validate(email))
                .ToList();
        }

        public static async Task<IList<DateTime>> GetHolidaysAsync(this string input)
        {
            if (!File.Exists(input))
            {
                return input.ConvertToDates();
            }

            var lines = await File.ReadAllLinesAsync(input);

            var dates = new List<DateTime>();
            
            foreach (var i in lines)
            {
                if (DateTime.TryParse(i.Trim(), out var date))
                {
                    dates.Add(date);
                }
            }

            return dates;
        }

        public static IList<DateTime> ConvertToDates(this string input)
        {
            var inputArray = input.Split(",");

            var dates = new List<DateTime>();

            foreach (var i in inputArray)
            {
                if (DateTime.TryParse(i.Trim(), out var date))
                {
                    dates.Add(date);
                }
            }

            return dates;
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
                    e.WeekdayCount.ToString(),
                    e.WeekendCount.ToString(),
                    e.HolidayCount.ToString(),
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
        
        public static void WriteToCsv(this IEnumerable<AttendeeSummary> items, string path)
        {
            using var writer = new StreamWriter(path);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            var records = items.ToList();
            csv.WriteRecords(records);   
        }
    }
}
