using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http.Testing;
using Flurl.Util;

namespace CalendarQuery.Tests
{
    public class Tests
    {
        [Test]
        public async Task GetUrlContentsAsync_WhenFileContainsValidUrls_ThenGetContentsFromUrl()
        {
            var input = "SampleData/sample-calendars.txt";
            var contents  = await input.GetUrlContentsAsync();
            Assert.AreEqual(3, contents.Count);
        }

        [Test]
        public async Task GetUrlContentsAsync_WhenRequestHeaderIsMissingFilenameInfo_ThenDetermineFilenameFromUrl()
        {
            var url = "https://raw.githubusercontent.com/sohnemann/New-Zealand-Public-Holidays/main/2022-2032-public-holidays-nz-all.ics";
            var contents = await url.GetUrlContentsAsync();
            Assert.AreEqual("2022-2032-public-holidays-nz-all.ics", contents.First().Key);
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
            
            Assert.AreEqual(expectedFileName, contents.First().Key);
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
            
            Assert.AreEqual($"Unable to determine filename from {url}", ex?.Message);
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
            
            contents.WriteToDisk(path);
            
            Assert.True(File.Exists(filePath));
            
            File.Delete($"{path}/{filename}");
        }
    }
}
