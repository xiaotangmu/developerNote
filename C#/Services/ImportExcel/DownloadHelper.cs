using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WebApi.Services.ImportExcel
{
    public class DownloadHelper
    {
        public async static Task<DownloadModel> GetDownloadModel(string fileName)
        {
            try
            {
                string filePath = Path.Combine(TempFileManager.TempFileDirectory, fileName);
                var memoryStream = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memoryStream);
                }
                memoryStream.Seek(0, SeekOrigin.Begin);
                return new DownloadModel
                {
                    FileBytes = memoryStream,
                    ContentType = System.Net.Mime.MediaTypeNames.Application.Octet,
                    DownloadFileName = fileName
                };
            }
            catch (Exception ex)
            {
                throw new Exception(await Localization.Localizer.GetValueAsync("下載文件失敗")) ;
            }
        }
    }

    public class DownloadModel
    {
        public Stream FileBytes { get; set; }

        public string ContentType { get; set; }

        public string DownloadFileName { get; set; }
    }
}
