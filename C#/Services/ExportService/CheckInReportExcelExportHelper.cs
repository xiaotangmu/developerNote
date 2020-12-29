using Common.Converter;
using Common.Office;
using Microsoft.AspNetCore.Mvc;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
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
    public class CheckInReportExcelExportHelper : CheckInReportExportHelper
    {
        protected override void SaveFile(string fileName, List<CheckInReportItemViewModel> selectedData, List<TableHeader> columns, List<CheckInReportAdmissionTypeStatistics> admissionTypeStatistics)
        {
            IWorkbook book = ExcelWorkbookFactory.Singleton.Create(ExcelType.Excel2003);
            ExcelExportTrucker helper = new ExcelExportTrucker(book);

            ISheet firstSheet = book.GetSheetAt(0);
            firstSheet.Autobreaks = true;
            IFont titleFont = CreateTitleFont(book);
            //表頭
            BuildHeaderRow(firstSheet, titleFont, columns.Count);
            //列頭
            BuildColumnRow(book, firstSheet, titleFont, columns);
            //業務決定了數據不會超過Excel的單Sheet最大行數限制，不考慮分表填充數據
            FillData(book, firstSheet, selectedData, columns);
            //每一列自適應寬度
            SetColumnWidth(firstSheet, columns.Count);
            FillAdmissionTypeStatistics(book, firstSheet, admissionTypeStatistics);

            helper.Save(fileName);
        }

        private void FillAdmissionTypeStatistics(IWorkbook book, ISheet firstSheet, List<CheckInReportAdmissionTypeStatistics> admissionTypeStatistics)
        {
            if (admissionTypeStatistics == null)
            {
                return;
            }
            IFont font = CreateFooterFont(book);
            int total = admissionTypeStatistics.Sum(selector => selector.Amount);
            int beginRowIndex = firstSheet.LastRowNum + 2;
            for (int i = 0; i < admissionTypeStatistics.Count; i++)
            {
                CheckInReportAdmissionTypeStatistics statistics = admissionTypeStatistics[i];
                IRow row = firstSheet.CreateRow(beginRowIndex + i);
                ICell colorCell = row.CreateCell(0);
                colorCell.SetCellType(CellType.Blank);
                colorCell.CellStyle = SelectCellStyleByAdmissionType(book, statistics.AdmissionType, false);

                ICell valueCell = row.CreateCell(1);
                valueCell.SetCellType(CellType.String);
                valueCell.CellStyle = book.CreateCellStyle();
                valueCell.CellStyle.SetFont(font);
                valueCell.CellStyle.Alignment = HorizontalAlignment.Left;
                valueCell.SetCellValue(statistics.AdmissionType + " - " + statistics.Amount);
            }
            IRow totalRow = firstSheet.CreateRow(beginRowIndex + admissionTypeStatistics.Count);
            ICell totalCell = totalRow.CreateCell(0);
            totalCell.SetCellValue("Total = " + total);
            totalCell.CellStyle = book.CreateCellStyle();
            totalCell.CellStyle.SetFont(font);
        }

        private void SetColumnWidth(ISheet sheet, int columnCount)
        {
            sheet.SetColumnWidth(0, 10 * 256);
            for (int i = 0; i < columnCount; i++)
            {
                sheet.AutoSizeColumn(i + 1);
            }
            for (int colIndex = 0; colIndex < columnCount; colIndex++)
            {
                int columnWidth = sheet.GetColumnWidth(colIndex) / 280;
                for (int rowIndex = 0; rowIndex < sheet.LastRowNum; rowIndex++)
                {
                    IRow row = sheet.GetRow(rowIndex);
                    ICell cell = row.GetCell(colIndex);
                    if (cell == null)
                    {
                        continue;
                    }
                    int contextLength = Encoding.UTF8.GetBytes(cell.ToString()).Length;//获取当前单元格的内容宽度
                    columnWidth = columnWidth < contextLength ? contextLength : columnWidth;
                }
                sheet.SetColumnWidth(colIndex + 1, columnWidth * 220);
            }
        }

        private void FillData(IWorkbook book, ISheet firstSheet, List<CheckInReportItemViewModel> data, List<TableHeader> columns)
        {
            IFont contentFont = CreateContentFont(book);
            for (int romIndex = 0; romIndex < data.Count; romIndex++)
            {
                CheckInReportItemViewModel item = data[romIndex];
                IRow row = firstSheet.CreateRow(romIndex + 2);//從第3行開始填充數據
                ICell firstCell = row.CreateCell(0);//第一列為序號
                firstCell.SetCellValue(romIndex + 1);
                firstCell.CellStyle = SelectCellStyleByAdmissionType(book, item?.AdmissionType);
                firstCell.CellStyle.SetFont(contentFont);
                for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                {
                    ICell cell = row.CreateCell(columnIndex + 1);//第一列為序號
                    cell.SetCellType(CellType.String);
                    SetCellValue(cell, columns[columnIndex], item);
                    cell.CellStyle = firstCell.CellStyle;
                }
            }
        }

        private void SetCellValue(ICell cell, TableHeader tableHeader, CheckInReportItemViewModel item)
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
                    cell.SetCellValue(prop.GetValue(item)?.ToString());
                    break;
                }
            }
        }

        private ICellStyle SelectCellStyleByAdmissionType(IWorkbook book, string admissionType, bool isBorder = true)
        {
            if (admissionType?.Trim() == AdmissionTypeConverter.ConvertToCode(AdmissionTypeEnum.LOCAL))
            {
                return BuildLocalStyle(book, isBorder);
            }
            if (admissionType?.Trim() == AdmissionTypeConverter.ConvertToCode(AdmissionTypeEnum.NON_LOCAL))
            {
                return BuildNonlocalStyle(book, isBorder);
            }
            if (admissionType?.Trim() == AdmissionTypeConverter.ConvertToCode(AdmissionTypeEnum.MAIN_LAND))
            {
                return BuildMainlandStyle(book, isBorder);
            }
            return BuildDefaultStyle(book, isBorder);
        }

        private ICellStyle CreateHeaderStyle(IWorkbook book, IFont font)
        {
            ICellStyle style = book.CreateCellStyle();
            style.SetFont(font);
            style.Alignment = HorizontalAlignment.Left;
            HSSFPalette palette = ((HSSFWorkbook)book).GetCustomPalette();
            palette.SetColorAtIndex((short)13, 191, 191, 191);
            var color = palette.FindColor(191, 191, 191);
            //下面两行设置单元格背景色
            style.FillPattern = FillPattern.SolidForeground;
            style.FillForegroundColor = color.Indexed;
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;
            style.BorderTop = BorderStyle.Thin;
            style.BorderBottom = BorderStyle.Thin;
            return style;
        }

        private IFont CreateTitleFont(IWorkbook book)
        {
            IFont font = book.CreateFont();
            font.FontHeightInPoints = 11;
            font.FontName = "Arial";
            return font;
        }

        private IFont CreateContentFont(IWorkbook book)
        {
            IFont font = book.CreateFont();
            font.FontHeightInPoints = 9;
            font.FontName = "Arial";
            return font;
        }

        private IFont CreateFooterFont(IWorkbook book)
        {
            IFont font = book.CreateFont();
            font.FontHeightInPoints = 10;
            font.FontName = "Arial";
            return font;
        }

        private ICellStyle BuildDefaultStyle(IWorkbook book, bool isBorder = true)
        {
            ICellStyle style = book.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Left;
            if (isBorder)
            {
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.BorderBottom = BorderStyle.Thin;
            }

            return style;
        }

        private ICellStyle BuildLocalStyle(IWorkbook book, bool isBorder = true)
        {
            ICellStyle style = book.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Left;
            HSSFPalette palette = ((HSSFWorkbook)book).GetCustomPalette();
            palette.SetColorAtIndex((short)9, 248, 203, 173);
            var color = palette.FindColor(248, 203, 173);
            //下面两行设置单元格背景色
            style.FillPattern = FillPattern.SolidForeground;
            style.FillForegroundColor = color.Indexed;
            if (isBorder)
            {
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.BorderBottom = BorderStyle.Thin;
            }

            return style;
        }

        private ICellStyle BuildNonlocalStyle(IWorkbook book, bool isBorder = true)
        {
            ICellStyle style = book.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Left;

            HSSFPalette palette = ((HSSFWorkbook)book).GetCustomPalette();
            palette.SetColorAtIndex((short)10, 198, 224, 180);
            var color = palette.FindColor(198, 224, 180);
            //下面两行设置单元格背景色
            style.FillPattern = FillPattern.SolidForeground;
            style.FillForegroundColor = color.Indexed;
            if (isBorder)
            {
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.BorderBottom = BorderStyle.Thin;
            }
            return style;
        }

        private ICellStyle BuildMainlandStyle(IWorkbook book, bool isBorder = true)
        {
            ICellStyle style = book.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Left;

            HSSFPalette palette = ((HSSFWorkbook)book).GetCustomPalette();
            palette.SetColorAtIndex((short)11, 252, 228, 214);
            var color = palette.FindColor(252, 228, 214);
            //下面两行设置单元格背景色
            style.FillPattern = FillPattern.SolidForeground;
            style.FillForegroundColor = color.Indexed;
            if (isBorder)
            {
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.BorderBottom = BorderStyle.Thin;
            }
            return style;
        }

        private IRow BuildColumnRow(IWorkbook book, ISheet sheet, IFont font, List<TableHeader> columns)
        {
            IRow row = sheet.CreateRow(1);
            ICell indexCell = row.CreateCell(0);
            indexCell.SetCellType(CellType.Blank);
            indexCell.CellStyle = CreateHeaderStyle(book, font);
            for (int i = 0; i < columns.Count; i++)
            {
                ICell cell = row.CreateCell(i + 1);//第一列為序號
                cell.SetCellValue(columns[i].Display);
                cell.SetCellType(CellType.String);
                cell.CellStyle = indexCell.CellStyle;
            }
            return row;
        }

        private IRow BuildHeaderRow(ISheet sheet, IFont font, int columnAmount)
        {
            IRow row = sheet.CreateRow(0);
            ICell cell = row.CreateCell(0);
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, columnAmount));//另外多一列為序號
            cell.SetCellType(CellType.String);
            cell.SetCellValue(string.Format("Rooming List at EAH hostel as at {0} {1} {2}", DateTime.Now.Day, MonthConverter.ConvertToShortEnglish(DateTime.Now.Month), DateTime.Now.Year));
            cell.CellStyle.SetFont(font);
            cell.CellStyle.Alignment = HorizontalAlignment.Center;
            return row;
        }
    }
}
