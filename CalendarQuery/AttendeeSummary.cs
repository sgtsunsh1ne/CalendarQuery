using System.Collections.Generic;
using System.Linq;

namespace CalendarQuery
{
    public class AttendeeSummary
    {
        public AttendeeSummary(string attendee, IEnumerable<RosteredEvent> rosteredEvents)
        {
            Attendee = attendee;
            RosteredEvents = rosteredEvents.OrderBy(i => i.AdjustedStartDateLocal).ToList();
        }

        public string Attendee { get; }
        public IList<RosteredEvent> RosteredEvents { get; }
        public int WeekdayCount => RosteredEvents.Sum(i => i.WeekdayCount);
        public int WeekendCount => RosteredEvents.Sum(i => i.WeekendCount);
        public int PublicHolidayCount => RosteredEvents.Sum(i => i.PublicHolidayCount);
        public int TotalDays => WeekdayCount + WeekendCount + PublicHolidayCount;

        public bool RosteredEventsContainsConflicts
        {
            get
            {
                for (var i = 0; i < RosteredEvents.Count; i++)
                {
                    if (i + 1 >= RosteredEvents.Count) break;
                
                    if (RosteredEvents[i].AdjustedStartDateLocal < RosteredEvents[i + 1].AdjustedEndDateLocal &&
                        RosteredEvents[i].AdjustedEndDateLocal > RosteredEvents[i + 1].AdjustedStartDateLocal)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public string Notes
        {
            get
            {
                var notes = string.Empty;
                
                if (RosteredEventsContainsConflicts)
                {
                    notes = "Events overlap.  Counts may be wrong.";
                }

                return notes;
            }
        }
    }
}
