using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi.Services.ImportExcel
{
    public class DownloadTemplateHelper
    {
        public async static Task<DownloadModel> DownloadProtectLimitedFile(string rawFileName, IDataProtectionProvider provider)
        {
            ITimeLimitedDataProtector protector = provider.CreateProtector("fileProtector").ToTimeLimitedDataProtector();
            string fileName = protector.Unprotect(rawFileName);
            return await DownloadHelper.GetDownloadModel(fileName);
        }

        public static string GetDataProtectLimitedTime(string templateFileName, IDataProtectionProvider provider, IConfiguration configuration)
        {
            int downloadIdValidTime = Convert.ToInt32(configuration.GetSection("DownloadIdValidTime").Value);
            ITimeLimitedDataProtector protector = provider.CreateProtector("fileProtector").ToTimeLimitedDataProtector();
            if (downloadIdValidTime == 0)
            {
                return protector.Protect(templateFileName);
            }
            else
            {
                return protector.Protect(templateFileName, TimeSpan.FromSeconds(downloadIdValidTime));
            }
        }

        //public static DownloadModel UnProtectLimited(string rawFileName, IDataProtectionProvider provider)
        //{
        //    ITimeLimitedDataProtector protector = provider.CreateProtector("fileProtector").ToTimeLimitedDataProtector();
        //    string fileName = protector.Unprotect(rawFileName);
        //    return DownloadHelper.GetDownloadModel(fileName);
        //}

        private async static Task<string> GetFileName(HttpContext httpContext, string fileName)
        {
            string culture = Thread.CurrentThread.CurrentCulture.Name;
            //是否使用cookie记录语言
            if (httpContext.Request.Cookies.ContainsKey("_culture"))
            {
                culture = httpContext.Request.Cookies["_culture"];
            }
            //是否使用header记录语言
            else if (httpContext.Request.Headers.ContainsKey("Culture"))
            {
                culture = httpContext.Request.Headers["Culture"];
            }
            switch (culture)
            {
                case "zh-CN":
                    fileName = fileName + ".xlsx";
                    break;
                case "zh-MO":
                    fileName = fileName + "_zh-MO.xlsx";
                    break;
                case "en-GB":
                    fileName = fileName + "_en-GB.xlsx";
                    break;
                case "en-US":
                    fileName = fileName + "_en-GB.xlsx";
                    break;
            }
            //源模板路径
            string originFilePath = Path.Combine(Startup.CurrentContentPath, "Services\\ImportExcel\\Template", fileName);
            //临时下载文件路径，避免源模板文件被删除
            string tempFilePath = Path.Combine(Startup.TemporaryFileDirectory, fileName);
            if (!Directory.Exists(Startup.TemporaryFileDirectory))
            {
                Directory.CreateDirectory(Startup.TemporaryFileDirectory);
            }
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
            if (!File.Exists(originFilePath))
            {
                throw new Exception(await Localization.Localizer.GetValueAsync("TemplateLost"));
            }
            //Copy到临时文件夹
            File.Copy(originFilePath, tempFilePath);

            return fileName;
        }
    }
}
