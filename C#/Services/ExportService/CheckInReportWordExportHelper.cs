using Common.Converter;
using Common.Office;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ViewModel.Report;
using ViewModel.Student;

namespace WebApi.Services.ExportService
{
    public class CheckInReportWordExportHelper : CheckInReportExportHelper
    {
        protected override void SaveFile(string fileName, List<CheckInReportItemViewModel> selectedData, List<TableHeader> columns, List<CheckInReportAdmissionTypeStatistics> admissionTypeStatistics)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                XWPFDocument doc = new XWPFDocument();
                SetPageSize(doc);
                CreateTitle(doc);
                CreateTable(doc, selectedData, columns, admissionTypeStatistics);
                doc.Write(fs);
            }
        }

        private void SetPageSize(XWPFDocument doc)
        {
            CT_SectPr m_SectPr = new CT_SectPr();       //实例一个尺寸类的实例
            m_SectPr.pgSz.w = 16838;        //设置宽度（这里是一个ulong类型）
            m_SectPr.pgSz.h = 11906;        //设置高度（这里是一个ulong类型）
            doc.Document.body.sectPr = m_SectPr;          //设置页面的尺寸
        }

        private void CreateTable(XWPFDocument doc, List<CheckInReportItemViewModel> selectedData, List<TableHeader> columns, List<CheckInReportAdmissionTypeStatistics> admissionTypeStatistics)
        {
            XWPFTable tableContent = doc.CreateTable(selectedData.Count + 1, columns.Count + 1);
            BuildColumnRow(tableContent, columns);
            FillData(tableContent, selectedData, columns, admissionTypeStatistics);
            //換行
            NextParagraph(doc);
            //製作統計表數據
            FillAdmissionTypeStatistics(doc, admissionTypeStatistics);
        }

        private void NextParagraph(XWPFDocument doc)
        {
            XWPFParagraph emptyParagraph = doc.CreateParagraph();
            XWPFRun emptyRun = emptyParagraph.CreateRun();
            emptyRun.AddCarriageReturn();
        }

        private void FillAdmissionTypeStatistics(XWPFDocument doc, List<CheckInReportAdmissionTypeStatistics> admissionTypeStatistics)
        {
            if (admissionTypeStatistics == null)
            {
                return;
            }
            //创建一个无边框的表格
            XWPFTable table = doc.CreateTable(admissionTypeStatistics.Count + 1, 1);
            FillAdmissionTypeTotal(table, admissionTypeStatistics);
            FillTotal(table, admissionTypeStatistics);
        }

        private void FillAdmissionTypeTotal(XWPFTable table, List<CheckInReportAdmissionTypeStatistics> admissionTypeStatistics)
        {
            for (int i = 0; i < admissionTypeStatistics.Count; i++)
            {
                CheckInReportAdmissionTypeStatistics statistics = admissionTypeStatistics[i];
                string fillColor = SelectFillColorByAdmissionType(statistics?.AdmissionType);
                XWPFTableRow row = table.GetRow(i);
                XWPFTableCell firstCell = row.GetCell(0);
                XWPFParagraph firstParagraph = firstCell.AddParagraph();
                XWPFRun firstRun = CreateStatisticsContentRun(firstParagraph);
                firstRun.SetText(statistics.AdmissionType + " - " + statistics.Amount.ToString());
                firstCell.SetColor(fillColor);
            }
        }

        private void FillTotal(XWPFTable table, List<CheckInReportAdmissionTypeStatistics> admissionTypeStatistics)
        {
            int total = admissionTypeStatistics.Sum(selector => selector.Amount);
            XWPFTableRow totalRow = table.GetRow(admissionTypeStatistics.Count);
            XWPFTableCell totalCell = totalRow.GetCell(0);
            XWPFParagraph paragraph = totalCell.AddParagraph();
            XWPFRun run = CreateStatisticsContentRun(paragraph);
            run.SetText("Total = " + total);
        }

        private void FillData(XWPFTable tableContent, List<CheckInReportItemViewModel> selectedData, List<TableHeader> columns, List<CheckInReportAdmissionTypeStatistics> admissionTypeStatistics)
        {
            for (int romIndex = 0; romIndex < selectedData.Count; romIndex++)
            {
                CheckInReportItemViewModel item = selectedData[romIndex];
                string fillColor = SelectFillColorByAdmissionType(item?.AdmissionType);
                XWPFTableRow row = tableContent.GetRow(romIndex + 1);//從第2行開始填充數據

                FillContentCell(row, 0, fillColor, (romIndex + 1).ToString());
                for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                {
                    XWPFRun run = FillContentCell(row, columnIndex + 1, fillColor);
                    SetCellValue(run, columns[columnIndex], item);
                }
            }
        }

        private XWPFRun FillContentCell(XWPFTableRow row, int cellIndex, string fillColor, string text = null)
        {
            XWPFTableCell cell = row.GetCell(cellIndex);//第一列為序號
            cell.SetColor(fillColor);
            XWPFParagraph paragraph = cell.AddParagraph();
            XWPFRun run = CreateContentdRun(paragraph);
            if (!string.IsNullOrEmpty(text))
            {
                run.SetText(text);
            }
            return run;
        }

        private void SetCellValue(XWPFRun run, TableHeader tableHeader, CheckInReportItemViewModel item)
        {
            if (item == null)
            {
                return;
            }
            PropertyInfo[] properties = item.GetType().GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                if (prop.Name.Trim() == tableHeader.Code.Trim())
                {
                    string value = prop.GetValue(item)?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        run.SetText(value);
                    }
                    break;
                }
            }
        }

        private string SelectFillColorByAdmissionType(string admissionType)
        {
            if (admissionType?.Trim() == AdmissionTypeConverter.ConvertToCode(AdmissionTypeEnum.LOCAL))
            {
                return "#F8CBAD";
            }
            if (admissionType?.Trim() == AdmissionTypeConverter.ConvertToCode(AdmissionTypeEnum.NON_LOCAL))
            {
                return "#C6E0B4";
            }
            if (admissionType?.Trim() == AdmissionTypeConverter.ConvertToCode(AdmissionTypeEnum.MAIN_LAND))
            {
                return "#FCE4D6";
            }
            return string.Empty;
        }

        private XWPFRun CreateTitleRun(XWPFParagraph paragraph)
        {
            XWPFRun run = paragraph.CreateRun();
            run.FontFamily = "Arial";
            run.FontSize = 11;
            return run;
        }

        private XWPFRun CreateContentdRun(XWPFParagraph paragraph)
        {
            XWPFRun run = paragraph.CreateRun();
            run.FontFamily = "Arial";
            run.FontSize = 9;
            return run;
        }

        private XWPFRun CreateStatisticsContentRun(XWPFParagraph paragraph)
        {
            XWPFRun run = paragraph.CreateRun();
            run.FontFamily = "Arial";
            run.FontSize = 10;
            return run;
        }

        private void CreateTitle(XWPFDocument doc)
        {
            XWPFParagraph title = doc.CreateParagraph();
            title.Alignment = ParagraphAlignment.CENTER;
            XWPFRun r0 = CreateTitleRun(title);
            r0.SetText(string.Format("Rooming List at EAH hostel as at {0} {1} {2}", DateTime.Now.Day, MonthConverter.ConvertToShortEnglish(DateTime.Now.Month), DateTime.Now.Year));
        }

        private void BuildColumnRow(XWPFTable table, List<TableHeader> columns)
        {
            XWPFTableRow row = table.GetRow(0);
            XWPFTableCell indexCell = row.GetCell(0);
            XWPFParagraph indexParagraph = indexCell.AddParagraph();
            indexParagraph.Alignment = ParagraphAlignment.LEFT;
            XWPFRun indexRun = CreateTitleRun(indexParagraph);
            indexRun.SetText(string.Empty);
            indexCell.SetColor("#BFBFBF");
            for (int i = 0; i < columns.Count; i++)
            {
                XWPFTableCell cell = row.GetCell(i + 1);//第一列為序號
                XWPFParagraph paragraph = cell.AddParagraph();
                paragraph.Alignment = ParagraphAlignment.LEFT;
                XWPFRun run = CreateTitleRun(paragraph);
                run.SetText(columns[i].Display);
                cell.SetColor("#BFBFBF");
            }
        }
    }
}
