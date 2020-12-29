using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ViewModel.Student;
using WebApi.Services.ImportExcel;

namespace WebApi.Services
{
    public class FileUploader
    {
        private readonly IDataProtectionProvider _provider;
        private readonly IConfiguration _configuration;

        public FileUploader(IDataProtectionProvider dataProvider, IConfiguration configuration)
        {
            _provider = dataProvider;
            _configuration = configuration;
        }

        public async Task<List<FileViewModel>> UploadFile(HttpRequest request)
        {
            var files = request.Form.Files;
            long size = files.Sum(f => f.Length);
            //2Mb限制
            //if (size > 2097152)
            //{
            //    throw new Exception(await Localization.Localizer.GetValueAsync("FileSizeIsOver"));
            //}
            List<FileViewModel> fileGroup = new List<FileViewModel>();
            foreach (var formFile in files)
            {
                if (formFile.Length <= 0)
                {
                    continue;
                }
                using (var stream = formFile.OpenReadStream())
                {
                    byte[] bytes = new byte[formFile.Length];
                    await stream.ReadAsync(bytes, 0, (int)formFile.Length);
                    FileViewModel file = new FileViewModel
                    {
                        Name = formFile.FileName,
                        Url = SaveFileToLocation(bytes, formFile.FileName)
                    };
                    fileGroup.Add(file);
                }
            }
            return fileGroup;
        }

        private string SaveFileToLocation(byte[] bytes, string tempFileName)
        {
            if (!Directory.Exists(Startup.TemporaryFileDirectory))
            {
                Directory.CreateDirectory(Startup.TemporaryFileDirectory);
            }
            string fileName = Path.GetFileNameWithoutExtension(tempFileName);
            string fileExtension = Path.GetExtension(tempFileName);
            string storageFileName =fileName+"-"+ Guid.NewGuid().ToString()+fileExtension;
            string tempFilePath = Path.Combine(Startup.TemporaryFileDirectory, storageFileName);
            File.WriteAllBytesAsync(tempFilePath, bytes);

            return DownloadTemplateHelper.GetDataProtectLimitedTime(storageFileName, _provider, _configuration);
        }
    }
}
