using Flurl.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using EmailValidation;

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
    }
}
