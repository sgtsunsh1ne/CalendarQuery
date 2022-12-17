using System.Collections.Generic;
using System.Linq;
using Humanizer;
using Humanizer.Localisation;

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
        public string CalendarName           => RosteredEvents.Aggregate(string.Empty, (c, i) => c + $"{i.CalendarName}\n");
        public string ActualStartDateLocal   => RosteredEvents.Aggregate(string.Empty, (c, i) => c + $"{i.StartDateLocal:ddd dd MMM yy HH:mm tt}\n");
        public string ActualEndDateLocal     => RosteredEvents.Aggregate(string.Empty, (c, i) => c + $"{i.EndDateLocal:ddd dd MMM yy HH:mm tt}\n");
        public string ActualDuration         => RosteredEvents.Aggregate(string.Empty, (c, i) => c + $"{i.ActualDuration.Humanize(maxUnit:TimeUnit.Day, precision: 3)}\n");
        public string AdjustedStartDateLocal => RosteredEvents.Aggregate(string.Empty, (c, i) => c + $"{i.AdjustedStartDateLocal:ddd dd MMM yy HH:mm tt}\n");
        public string AdjustedEndDateLocal   => RosteredEvents.Aggregate(string.Empty, (c, i) => c + $"{i.AdjustedEndDateLocal:ddd dd MMM yy HH:mm tt}\n");
        public string AdjustedDuration       => RosteredEvents.Aggregate(string.Empty, (c, i) => c + $"{i.AdjustedDuration.Humanize(maxUnit: TimeUnit.Day, minUnit:TimeUnit.Day, precision: 3)}\n");
        public int WeekdayCount              => RosteredEvents.Sum(i => i.WeekdayCount);
        public int WeekendCount              => RosteredEvents.Sum(i => i.WeekendCount);
        public int HolidayCount              => RosteredEvents.Sum(i => i.PublicHolidayCount);
        public int TotalDays                 => WeekdayCount + WeekendCount + HolidayCount;

        public string Notes
        {
            get
            {
                var notes = string.Empty;
                
                if (RosteredEventsContainsConflicts)
                {
                    notes = "Events overlap.  Counts may be wrong.\n";
                }

                return notes;
            }
        }
        
        public string ApprovedBy => string.Empty;
        public string ApprovedOn => string.Empty;
        
        private IList<RosteredEvent> RosteredEvents { get; }
        
        private bool RosteredEventsContainsConflicts
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
    }
}
