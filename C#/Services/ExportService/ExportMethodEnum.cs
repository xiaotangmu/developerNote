using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.ExportService
{
    public enum ExportMethodEnum
    {
        EXCEL, WORD, PDF
    }
    public class ExportMethodConverter
    {
        public static ExportMethodEnum ConvertToEnum(string method)
        {
            ExportMethodEnum result = ExportMethodEnum.EXCEL;
            switch (method)
            {
                case "xlxs":
                    result = ExportMethodEnum.EXCEL;
                    break;
                case "word":
                    result = ExportMethodEnum.WORD;
                    break;
                case "pdf":
                    result = ExportMethodEnum.PDF;
                    break;
                default:
                    break;
            }
            return result;
        }

        public static string GetExtension(string method)
        {
            string result = string.Empty;
            switch (method)
            {
                case "xlsx":
                    result = ".xls";
                    break;
                case "word":
                    result = ".doc";
                    break;
                case "pdf":
                    result = ".pdf";
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}
