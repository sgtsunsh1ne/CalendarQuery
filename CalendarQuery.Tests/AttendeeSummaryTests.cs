using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CalendarQuery.Extensions;
using CsvHelper;
using Moq;
using Calendar = Ical.Net.Calendar;

namespace CalendarQuery.Tests
{
    public class AttendeeSummaryTests
    {
        [Test]
        public void Notes_WhenAttendeeSummaryContainsConflictingEvents_ThenReturnNoteToIndicateWarning()
        {
            var ev1 = Utility.CalendarEvent("01 Dec 22 00:00 AM", "07 Dec 22 00:00 AM");
            var ev2 = Utility.CalendarEvent("15 Nov 22 00:00 AM", "15 Dec 22 00:00 AM");

            var summary = new AttendeeSummary(
                It.IsAny<string>(),
                new List<RosteredEvent>
                {
                    new (ev1, 12, new List<DateTime>()),
                    new (ev2, 12, new List<DateTime>())
                });

            Assert.That(summary.Notes, Contains.Substring("Counts may be wrong"));
        }
        
        
        [Test]
        public void GenerateConsoleTable_EndToEndTest()
        {
            var month = 11;
            var input = "SampleData/sample-ics-with-valid-events.ics";

            using var calendarReader = new StreamReader(input);

            var calendars = new Dictionary<string, Calendar>
            {
                {"sample.ics", Calendar.Load(calendarReader)}
            };

            var roster = calendars
                .SelectMany(i => i.Value.Events)
                .Select(i => new RosteredEvent(i, month, new List<DateTime>()))
                .GroupBy(i => i.Attendees)
                .Select(i => new AttendeeSummary(i.Key, i));

            var table = roster.GenerateConsoleTable();

            var tableContainsRows = table.Rows.Any();
            var tableColumnCount  = table.Columns.Count;
            
            Assert.IsTrue(tableContainsRows);
            Assert.That(tableColumnCount, Is.EqualTo(9));
        }
        
        [Test]
        [TestCase(ReportType.AttendeeSummary)]
        [TestCase(ReportType.AttendeeSummaryVerbose)]
        public void WriteToCsv_EndToEndTest(ReportType reportType)
        {
            var month = 11;
            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            var filename = $"{reportType}-{monthName}.csv";
            var input = "SampleData/sample-ics-with-valid-events.ics";

            using var calendarReader = new StreamReader(input);

            var calendars = new Dictionary<string, Calendar>
            {
                {"sample.ics", Calendar.Load(calendarReader)}
            };

            var roster = calendars
                .SelectMany(i => i.Value.Events)
                .Where(i => i.FilterByMonth(month))
                .Select(i => new RosteredEvent(i, month, new List<DateTime>()))
                .GroupBy(i => i.Attendees)
                .Select(attendeeEvents => new AttendeeSummary(attendeeEvents.Key, attendeeEvents))
                .ToList();
            
            roster.WriteToCsv(reportType, filename);
            
            Assert.True(File.Exists(filename));
            
            using var reader = new StreamReader(filename);
            using var csv    = new CsvReader(reader, CultureInfo.InvariantCulture);

            var rows = csv.GetRecords<dynamic>();
            
            Assert.True(rows.Any());
            
            File.Delete(filename);
        }

        [Test]
        public static void WriteToCsv_WhenReportTypeIsInvalid_ThenThrowArgumentOutOfRangeException()
        {
            using var calendarReader = new StreamReader("SampleData/sample-ics-with-valid-events.ics");

            var calendars = new Dictionary<string, Calendar>
            {
                {"sample.ics", Calendar.Load(calendarReader)}
            };
            
            var roster = calendars
                .SelectMany(i => i.Value.Events)
                .Select(i => new RosteredEvent(i, 11, new List<DateTime>()))
                .GroupBy(i => i.Attendees)
                .Select(attendeeEvents => new AttendeeSummary(attendeeEvents.Key, attendeeEvents))
                .ToList();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                roster.WriteToCsv((ReportType)int.MaxValue, "some-file.csv");
            });
        }
    }
}
