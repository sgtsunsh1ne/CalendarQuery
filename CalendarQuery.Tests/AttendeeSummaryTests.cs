using System;
using System.Collections.Generic;
using Moq;

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
    }
}
