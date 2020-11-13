using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace GarfieldDownloader
{
   class Program
   {
      static async Task Main(string[] args) {
         List<Task> tasks = new List<Task>();
         for (int i = 0; i <= DateTime.Now.Year - _baseDate.Year; i++) {
            int year = _baseDate.Year + i;
            tasks.Add(Task.Run(() => DownloadComic(year)));
         }
         Task.WaitAll(tasks.ToArray());

         Compress();
      }

      private static void Compress() {
         if (!Directory.Exists(_baseDirArchive))
            Directory.CreateDirectory(_baseDirArchive);

         string[] years = Directory.GetDirectories(_baseDirImg);
         foreach (var y in years) {
            string year = new DirectoryInfo(y).Name;
            string[] months = Directory.GetDirectories(Path.Combine(_baseDirImg, year));
            foreach (var m in months) {
               string destFile = Path.Combine(_baseDirArchive, $"{year}-{new DirectoryInfo(m).Name}.zip");
               if (!File.Exists(destFile))
                  ZipFile.CreateFromDirectory(m, destFile);
            }
         _logger.Info("[{0:d2}] Finished compressing year {1}", Task.CurrentId, year);
         }
      }

      private static async Task DownloadComic(int year) {
         try {
            DateTime date = _baseDate;
            if (_baseDate.Year < year)
               date = new DateTime(year, 1, 1);

            DateTime dateEnd = DateTime.Now.Date;
            if (DateTime.Now.Year > year)
               dateEnd = new DateTime(year + 1, 1, 1).AddDays(-1);

            while (date <= dateEnd) {
               //http://images.ucomics.com/comics/ga/1998/ga980101.gif
               string url = $"{_baseUrl}/{date.Year}/ga{date.ToString("yyMMdd")}.gif";
               string dirPath = Path.Combine(_baseDirImg, $@"{date.Year:d4}\{date.Month:d2}");
               if (!Directory.Exists(dirPath))
                  Directory.CreateDirectory(dirPath);

               string filePath = Path.Combine(dirPath, Path.GetFileName(url));
               if (File.Exists(filePath)) {
                  date = date.AddDays(1);
                  continue;
               }

               //_logger.Debug("[{0}] Downloading {1}", Task.CurrentId, url);
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
            _logger.Info("[{0:d2}] Finished downloading year {1}", Task.CurrentId, year);
         } catch (HttpRequestException e1) {
            _logger.Error(e1, "Http Request Exception");
         } catch (Exception e2) {
            _logger.Error(e2, "General Exception");
         }
      }

      static readonly HttpClient _client = new HttpClient();
      static string _baseDirImg = @"D:\Temp\Garfield";
      static string _baseDirArchive = @"D:\Temp\Garfield-zip";
      static string _baseUrl = @"http://images.ucomics.com/comics/ga/";
      static DateTime _baseDate = new DateTime(1978, 6, 19);
      private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
   }
}
