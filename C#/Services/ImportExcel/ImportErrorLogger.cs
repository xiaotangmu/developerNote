using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Services.ImportExcel
{
    public class ImportErrorLogger
    {
        public static async Task WriteLog(string fileFullName, int dataRow, string error)
        {
            using (FileStream stream = new FileStream(fileFullName, FileMode.Append))
            {
                //读
                //StreamReader reader = new StreamReader(stream);
                //var content = await reader.ReadToEndAsync();
                //写
                string topRow = "\r\n======================" +
                                            string.Format(await Localization.Localizer.GetValueAsync("DataRow"), dataRow) +
                                            "====================\r\n";
                byte[] topByte = Encoding.UTF8.GetBytes(topRow);
                byte[] contentByte = Encoding.UTF8.GetBytes(error);
                //byte[] originByte = Encoding.UTF8.GetBytes(content);

                //await stream.WriteAsync(originByte, 0, originByte.Length);
                await stream.WriteAsync(topByte, 0, topByte.Length);
                await stream.WriteAsync(contentByte, 0, contentByte.Length);
            }
        }
    }
}
