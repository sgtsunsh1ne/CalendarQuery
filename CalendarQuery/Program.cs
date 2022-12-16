using System;
using System.Threading.Tasks;
using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;

namespace CalendarQuery
{
    public class Program : ConsoleAppBase
    {
        private static async Task Main(string[] args)
        {
            await Host
                .CreateDefaultBuilder()
                .RunConsoleAppFrameworkAsync<Program>(args);
        }
        
        public async Task Run(
            [Option("c", "Calendar(s) - Accepts single URL, or TXT file containing list of URLs")] string c)
        {
            var contents = await c.GetUrlContentsAsync();
            var filePath = DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
            var calendars = contents.GetCalendars();
            
            contents.WriteToDisk(filePath);
        }
    }
}
