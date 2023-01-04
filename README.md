# CalendarQuery

```
Options:
  -c <String>        Calendar(s) - Accepts single URL, or TXT file containing list of URLs (Required)
  -m <Int32>         Month       - Accepts month, or if none provided, the current month will be used. (Default: 0)
  -a <String>        Attendee(s) - Accepts single email or TXT file contains list of emails (Default: )
  -h <String>        Holiday(s)  - Accepts multiple dates (yyyy-MM-dd) as comma-separated values (Default: )
  -r <ReportType>    ReportType  - AttendeeSummary | AttendeeSummaryVerbose (Default: AttendeeSummary)
```

<br/>

## Usage

**Multiple calendars, multiple attendees, multiple holidays, current month**

```
./CalendarQuery -c calendars.txt -a attendees.txt -h holidays.txt
```

**Multiple calendars, multiple attendees, multiple holidays, current month, verbose report**

```
./CalendarQuery -c calendars.txt -a attendees.txt -h holidays.txt -r AttendeeSummaryVerbose
```

**Multiple calendars, single attendee, singe holiday, January**
```
./CalendarQuery -c calendars.txt -a john.doe@example.com -h 2023-01-01 -m 1
```

**Single calendar, all attendees, multiple holidays, January**
```
./CalendarQuery -c https://example.com/some-calendar.ics -h 2023-01-01, 2023-01-02 -m 1
```


Have a look at sample text files in the [SampleData](https://github.com/sgtsunsh1ne/CalendarQuery/tree/master/CalendarQuery.Tests/SampleData) folder
* [sample-calendars.txt](https://github.com/sgtsunsh1ne/CalendarQuery/tree/master/CalendarQuery.Tests/SampleData/sample-calendars.txt)
* [sample-users.txt](https://github.com/sgtsunsh1ne/CalendarQuery/tree/master/CalendarQuery.Tests/SampleData/sample-users.txt)
* [sample-holidays.txt](https://github.com/sgtsunsh1ne/CalendarQuery/tree/master/CalendarQuery.Tests/SampleData/sample-holidays.txt)

<br/>

## Output
Upon running the command above:
* Report is saved as CSV file
* Report also appears as console output
* `.ics` calendars are saved to local disk for posterity and as source of truth

Have a look at sample CSVs in the [SampleData](https://github.com/sgtsunsh1ne/CalendarQuery/tree/master/CalendarQuery.Tests/SampleData) folder
* [AttendeeSummary-December.csv](https://github.com/sgtsunsh1ne/CalendarQuery/blob/master/CalendarQuery.Tests/SampleData/AttendeeSummary-December.csv)
* [AttendeeSummaryVerbose-December.csv](https://github.com/sgtsunsh1ne/CalendarQuery/blob/master/CalendarQuery.Tests/SampleData/AttendeeSummaryVerbose-December.csv) 


