using Microsoft.AspNetCore.Mvc;
using Supervisor.Report;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ViewModel.Report;
using WebApi.Services.ImportExcel;

namespace WebApi.Services.ExportService
{
    public class ExportSimpleReportHelper
    {
        public async Task<FileStreamResult> Export<T>(string method, string fileName, List<T> data, List<TableHeader> columns, string titleText = null)
        {
            ExportMethodEnum exportMethod = ExportMethodConverter.ConvertToEnum(method);
            ExportSimpleReportHelper helper = null;
            switch (exportMethod)
            {
                case ExportMethodEnum.EXCEL:
                    helper = new ExportSimpleExcelHelper();
                    break;
                case ExportMethodEnum.WORD:
                    helper = new ExportSimpleWordHelper();
                    break;
                case ExportMethodEnum.PDF:
                    helper = new ExportSimplePDFHelper();
                    break;
            }
            if (!Path.HasExtension(fileName))
            {
                fileName = fileName + ExportMethodConverter.GetExtension(method);
            }
            return await helper.GenerateFile(fileName, data, columns, titleText);
        }

        private async Task<FileStreamResult> GenerateFile<T>(string fileName, List<T> data, List<TableHeader> columns, string titleText = null)
        {
            string fileFullPath = Path.Combine(Startup.TemporaryFileDirectory, fileName);
            SaveFile(fileFullPath, data, columns, titleText);
            try
            {
                using (var file = new FileStream(fileFullPath, FileMode.Open))
                {
                    file.Close();
                    file.Dispose();
                }
                DownloadModel model = await DownloadHelper.GetDownloadModel(fileName);
                return new FileStreamResult(model.FileBytes, model.ContentType);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected virtual void SaveFile<T>(string fileName, List<T> selectedData, List<TableHeader> columns, string titleText = null)
        {
            //子类实现
        }
    }
}
