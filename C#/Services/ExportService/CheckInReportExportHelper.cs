using Microsoft.AspNetCore.Mvc;
using Supervisor.Report;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ViewModel.Report;
using ViewModel.Student;
using WebApi.Services.ImportExcel;

namespace WebApi.Services.ExportService
{
    public class CheckInReportExportHelper
    {
        public async Task<FileStreamResult> Export(IHostelCheckInReportSupervisor hostelCheckInReportSupervisor, CheckInReportExportArg arg)
        {
            CheckInReportExportHelper helper = null;
            ExportMethodEnum method = ExportMethodConverter.ConvertToEnum(arg.ExportMethod);
            switch (method)
            {

                case ExportMethodEnum.EXCEL:
                    helper = new CheckInReportExcelExportHelper();
                    break;
                case ExportMethodEnum.WORD:
                    helper = new CheckInReportWordExportHelper();
                    break;
                case ExportMethodEnum.PDF:
                    //helper = new CheckInReportPDFExportHelper();
                    throw new Exception("Not Support");
                    break;
            }
            return await helper.GenerateFile(hostelCheckInReportSupervisor, arg);
        }

        private async Task<FileStreamResult> GenerateFile(IHostelCheckInReportSupervisor hostelCheckInReportSupervisor, CheckInReportExportArg arg)
        {
            CheckInReportViewModel result = await hostelCheckInReportSupervisor.GetCheckInReport(arg);
            List<CheckInReportAdmissionTypeStatistics> admissionTypeStatistics = BuildAdmisstionTypeStatistics(result?.Data);
            //筛选列
            List<CheckInReportItemViewModel> selectedData = result?.Data.ToList();
            string fileName = "Rooming-List-at-EAH-hostel-as-at-" + DateTime.Now.ToString("yyyy-MM-dd") + ExportMethodConverter.GetExtension(arg.ExportMethod);
            string fileFullPath = Path.Combine(Startup.TemporaryFileDirectory, fileName);
            SaveFile(fileFullPath, selectedData, result?.Title.ToList(), admissionTypeStatistics);
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

        protected virtual void SaveFile(string fileName, List<CheckInReportItemViewModel> selectedData, List<TableHeader> columns, List<CheckInReportAdmissionTypeStatistics> admissionTypeStatistics)
        {
            //子类实现
        }

        private List<CheckInReportAdmissionTypeStatistics> BuildAdmisstionTypeStatistics(IEnumerable<CheckInReportItemViewModel> data)
        {
            List<CheckInReportAdmissionTypeStatistics> result = new List<CheckInReportAdmissionTypeStatistics>();
            if (data == null)
            {
                return result;
            }
            int localAmount = data.Count(exp => exp.AdmissionType == AdmissionTypeConverter.ConvertToCode(AdmissionTypeEnum.LOCAL));
            result.Add(new CheckInReportAdmissionTypeStatistics
            {
                AdmissionType = AdmissionTypeConverter.ConvertToCode(AdmissionTypeEnum.LOCAL),
                Amount = localAmount
            });
            int nonLocalAmount = data.Count(exp => exp.AdmissionType == AdmissionTypeConverter.ConvertToCode(AdmissionTypeEnum.NON_LOCAL));
            result.Add(new CheckInReportAdmissionTypeStatistics
            {
                AdmissionType = AdmissionTypeConverter.ConvertToCode(AdmissionTypeEnum.NON_LOCAL),
                Amount = nonLocalAmount
            });
            int mainlandAmount = data.Count(exp => exp.AdmissionType == AdmissionTypeConverter.ConvertToCode(AdmissionTypeEnum.MAIN_LAND));
            result.Add(new CheckInReportAdmissionTypeStatistics
            {
                AdmissionType = AdmissionTypeConverter.ConvertToCode(AdmissionTypeEnum.MAIN_LAND),
                Amount = mainlandAmount
            });
            return result;
        }
    }
}
