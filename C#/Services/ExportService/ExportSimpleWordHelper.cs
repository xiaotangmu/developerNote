using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ViewModel.Report;

namespace WebApi.Services.ExportService
{
    public class ExportSimpleWordHelper : ExportSimpleReportHelper
    {
        protected override void SaveFile<T>(string fileName, List<T> selectedData, List<TableHeader> columns,string titleText )
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                XWPFDocument doc = new XWPFDocument();
                SetPageSize(doc);
                CreateTitle(doc, titleText);
                CreateTable(doc, selectedData, columns);
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

        private void CreateTable<T>(XWPFDocument doc, List<T> selectedData, List<TableHeader> columns)
        {
            XWPFTable tableContent = doc.CreateTable(selectedData.Count + 1, columns.Count);
            BuildColumnRow(tableContent, columns);
            FillData(tableContent, selectedData, columns);
        }

        private void FillData<T>(XWPFTable tableContent, List<T> selectedData, List<TableHeader> columns)
        {
            for (int roomIndex = 0; roomIndex < selectedData.Count; roomIndex++)
            {
                XWPFTableRow row = tableContent.GetRow(roomIndex + 1);//從第2行開始填充數據
                for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                {
                    XWPFRun run = FillContentCell(row, columnIndex);
                    SetCellValue(run, columns[columnIndex], selectedData[roomIndex]);
                }
            }
        }

        private XWPFRun FillContentCell(XWPFTableRow row, int cellIndex)
        {
            XWPFTableCell cell = row.GetCell(cellIndex);
            XWPFParagraph paragraph = cell.AddParagraph();
            XWPFRun run = CreateContentdRun(paragraph);
            return run;
        }

        private void SetCellValue<T>(XWPFRun run, TableHeader tableHeader, T item)
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

        private void CreateTitle(XWPFDocument doc, string titleText)
        {
            if (string.IsNullOrEmpty(titleText))
            {
                return;
            }
            XWPFParagraph title = doc.CreateParagraph();
            title.Alignment = ParagraphAlignment.CENTER;
            XWPFRun r0 = CreateTitleRun(title);
            r0.SetText(titleText);
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
                XWPFTableCell cell = row.GetCell(i);
                XWPFParagraph paragraph = cell.AddParagraph();
                paragraph.Alignment = ParagraphAlignment.LEFT;
                XWPFRun run = CreateTitleRun(paragraph);
                run.SetText(columns[i].Display);
                cell.SetColor("#BFBFBF");
            }
        }
    }
}
