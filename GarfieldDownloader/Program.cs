using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace GarfieldDownloader
{
   class Program
   {
      static async Task Main(string[] args) {
         try {
            DateTime date = _baseDate;

            while (date <= DateTime.Now.Date) {
               //http://images.ucomics.com/comics/ga/1998/ga980101.gif
               string url = $"{_baseUrl}/{date.Year}/ga{date.ToString("yyMMdd")}.gif";
               string dirPath = Path.Combine(_baseDir, date.Year.ToString());
               if (!Directory.Exists(dirPath))
                  Directory.CreateDirectory(dirPath);

               string filePath = Path.Combine(dirPath, Path.GetFileName(url));
               if (File.Exists(filePath)) {
                  date = date.AddDays(1);
                  continue;
               }
               
               Console.WriteLine("Pobieram {0}", url);
               using (HttpResponseMessage response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead)) {
                  if (response.IsSuccessStatusCode)
                     using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync()) {
                        using (Stream streamToWriteTo = File.Open(filePath, FileMode.Create)) {
                           await streamToReadFrom.CopyToAsync(streamToWriteTo);
                           date = date.AddDays(1);
                        }
                     }
                  else {
                     date = date.AddDays(1);
                     continue;
                  }
               }
            }
         } catch (HttpRequestException e) {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message:{0} ", e.Message);
         }
      }

      static readonly HttpClient _client = new HttpClient();
      static string _baseDir = @"D:\Garfield";
      static string _baseUrl = @"http://images.ucomics.com/comics/ga/";
      static DateTime _baseDate = new DateTime(1978, 6, 19);
   }
}
