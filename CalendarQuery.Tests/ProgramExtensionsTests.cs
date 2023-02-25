using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CalendarQuery.Extensions;
using Flurl.Http.Testing;
using Flurl.Util;

namespace CalendarQuery.Tests
{
    public class ProgramExtensionsTests
    {
        [Test]
        public async Task GetUrlContentsAsync_WhenFileContainsValidUrls_ThenGetContentsFromUrl()
        {
            var input = "SampleData/sample-calendars.txt";
            var contents  = await input.GetUrlContentsAsync();
            Assert.That(contents.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task GetUrlContentsAsync_WhenRequestHeaderIsMissingFilenameInfo_ThenDetermineFilenameFromUrl()
        {
            var url = "https://raw.githubusercontent.com/sohnemann/New-Zealand-Public-Holidays/main/2022-2032-public-holidays-nz-all.ics";
            var contents = await url.GetUrlContentsAsync();
            Assert.That(contents.First().Key, Is.EqualTo("2022-2032-public-holidays-nz-all.ics"));
        }
        
        [Test]
        public async Task GetUrlContentsAsync_WhenRequestHeaderContainsFilenameInfo_ThenUseFilenameFromRequestHeader()
        {
            var sampleIcs = await File.ReadAllTextAsync("SampleData/sample-ics.ics");
            var expectedFileName = "some-filename.txt";
            var headers = new NameValueList<string>(false)
            {
                { "content-disposition", $"attachment; filename=\"{expectedFileName}\"" }
            };
         
            using var httpTest = new HttpTest();   
            httpTest.RespondWith(sampleIcs, 200, headers);
            
            var url = "https://random-url.com";
            var contents = await url.GetUrlContentsAsync();
            
            Assert.That(contents.First().Key, Is.EqualTo(expectedFileName));
        }
        
        [Test]
        public async Task GetUrlContentsAsync_WhenUnableToDetermineFilename_ThenThrowArgumentException()
        {
            var sampleIcs = await File.ReadAllTextAsync("SampleData/sample-ics.ics");
            var url = "https://random-url.com";
            
            using var httpTest = new HttpTest();
            httpTest.RespondWith(sampleIcs, 200);
            
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await url.GetUrlContentsAsync();
            });
            
            Assert.That(ex?.Message, Is.EqualTo($"Unable to determine filename from {url}"));
        }

        [Test]
        public async Task GetUrlContentsAsync_WhenFileDoesNotExist_TheReturnEmptyList()
        {
            var url = "random-string";
            var contents = await url.GetUrlContentsAsync();
            CollectionAssert.IsEmpty(contents);
        }
        
        [Test]
        public async Task WriteToDisk_RetrievedUrlContentsAreSavedToDisk()
        {
            var url = "https://raw.githubusercontent.com/sohnemann/New-Zealand-Public-Holidays/main/2022-2032-public-holidays-nz-all.ics";            
            var contents = await url.GetUrlContentsAsync();
            var path = "test-dir";
            var filename = contents.First().Key;
            var filePath = $"{path}/{filename}";
            
            await contents.WriteToDiskAsync(path, false);
            
            Assert.IsTrue(File.Exists(filePath));
            
            Directory.Delete(path, true);
        }
        
        [Test]
        public async Task GetAttendeesAsync_WhenInputIsNeitherEmailNorFile_ThenReturnEmptyList()
        {
            var input = "neither_email_nor_file";
            
            var users = await input.GetAttendeesAsync();
            
            Assert.IsEmpty(users);
        }

        [Test]
        public void GetAttendeesAsync_WhenInputIsEmail_ThenListContainsOneEmail()
        {
            var input = "random.user@loremipsum.com";
            
            var users = input.GetAttendeesAsync().Result.ToList();
            
            Assert.That(users.Count, Is.EqualTo(1));
            CollectionAssert.Contains(users, input);
        }

        [Test]
        public async Task GetAttendeesAsync_WhenInputIsFile_ThenRetrieveUsersFromFile()
        {
            var input = "SampleData/sample-users.txt";
            
            var users = await input.GetAttendeesAsync();
            
            Assert.That(users.ToList().Count, Is.EqualTo(3));
            CollectionAssert.Contains(users, "user1@some.random.email.com");
            CollectionAssert.Contains(users, "user2@some.random.email.com");
            CollectionAssert.Contains(users, "user3@hello.com");
        }

        [Test]
        [TestCase("2022-11-20", 1)]
        [TestCase("2022-11-20, 2022-12-01", 2)]
        [TestCase("gobbledygook", 0)]
        [TestCase("2022-11-20,gobbledygook", 1)]
        public void ConvertToDates_WhenInputAreValidDates_ThenReturnDatesAsList(string input, int expectedListCount)
        {
            var dates = input.ConvertToDates();
            
            Assert.That(dates.Count, Is.EqualTo(expectedListCount));

            foreach (var d in dates)
            {
                Assert.IsInstanceOf<DateTime>(d);
            }
        }

        [Test]
        public async Task GetHolidaysAsync_WhenProvidedWithCommaSeparatedDates_ThenReturnDates()
        {
            var input    = "2022-12-25, 2022-12-26, 2022-12-27, 2023-01-01, 2023-01-02, 2023-01-03";
            var holidays = await input.GetHolidaysAsync();
            Assert.That(holidays.Count, Is.EqualTo(6));
        }

        [Test]
        public async Task GetHolidaysAsync_WhenInputIsValidFile_ThenRetrieveHolidaysFromFile()
        {
            var input = "SampleData/sample-holidays.txt";
            var holidays = await input.GetHolidaysAsync();
            Assert.That(holidays.Count, Is.EqualTo(7));
        }
    }
}
