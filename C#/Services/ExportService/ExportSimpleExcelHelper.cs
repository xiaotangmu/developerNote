using Microsoft.AspNetCore.Http;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common.Office;
using ViewModel.Report;
using Microsoft.AspNetCore.Mvc;
using WebApi.Services.ExportService;
using NPOI.HSSF.UserModel;
using NPOI.SS.Util;

namespace WebApi.Services
{
    /// <summary>
    /// 导出服务
    /// </summary>
    public class ExportSimpleExcelHelper : ExportSimpleReportHelper
    {
        protected override void SaveFile<T>(string fileName, List<T> data, List<TableHeader> columns, string titleText = null)
        {
            IWorkbook book = ExcelWorkbookFactory.Singleton.Create(ExcelType.Excel2003);
            ExcelExportTrucker helper = new ExcelExportTrucker(book);

            IFont headerFont = CreateTitleFont(book);
            IFont contentFont = CreateContentFont(book);
            ICellStyle headerStyle = CreateHeaderStyle(book, headerFont);
            ICellStyle contentStyle = CreateContentStyle(book, contentFont);
            //填充数据
            int sheetCount = data.Count / 65530 + 1;
            for (int i = 0; i < sheetCount; i++)
            {
                ISheet sheet = book.GetSheetAt(i);
                if (sheet == null)
                {
                    sheet = book.CreateSheet();
                }
                int headerIndex = 0;
                //表頭
                if (!string.IsNullOrEmpty(titleText))
                {
                    headerIndex = 1;
                    BuildTitle(sheet, headerFont, columns.Count, titleText);
                }
                IRow headerRow = sheet.CreateRow(headerIndex);
                BuildHeaderRow(headerRow, columns, headerStyle);
                FillData(data, columns, sheet, contentStyle, headerIndex + 1);
                SetColumnWidth(sheet, columns.Count);
            }
            helper.Save(fileName);
        }

        private void BuildTitle(ISheet sheet, IFont headerFont, int columnCount, string titleText)
        {
            IRow row = sheet.CreateRow(0);
            ICell cell = row.CreateCell(0);
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, columnCount));//另外多一列為序號
            cell.SetCellType(CellType.String);
            cell.SetCellValue(titleText);
            cell.CellStyle.SetFont(headerFont);
            cell.CellStyle.Alignment = HorizontalAlignment.Center;
        }

        private ICellStyle CreateContentStyle(IWorkbook book, IFont contentFont)
        {
            ICellStyle style = book.CreateCellStyle();
            style.SetFont(contentFont);
            style.Alignment = HorizontalAlignment.Left;
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;
            style.BorderTop = BorderStyle.Thin;
            style.BorderBottom = BorderStyle.Thin;
            return style;
        }

        private void SetColumnWidth(ISheet sheet, int columnCount)
        {
            for (int i = 0; i < columnCount; i++)
            {
                sheet.AutoSizeColumn(i);
            }
            for (int colIndex = 0; colIndex < columnCount; colIndex++)
            {
                int columnWidth = sheet.GetColumnWidth(colIndex) / 256;
                sheet.SetColumnWidth(colIndex, columnWidth * 300);
            }
        }

        /// <summary>
        /// 填充数据，默认从第2行开始填充
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="columns"></param>
        /// <param name="sheet"></param>
        private void FillData<T>(List<T> data, List<TableHeader> columns, ISheet sheet, ICellStyle cellStyle, int startIndex)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            for (int cellIndex = 0; cellIndex < columns.Count; cellIndex++)
            {
                for (int rowIndex = startIndex; rowIndex < data.Count + startIndex; rowIndex++)
                {
                    IRow row = GetRow(sheet, rowIndex);
                    ICell cell = GetCell(row, cellIndex);
                    if (properties.Any(property => property.Name.Trim() == columns[cellIndex].Code.Trim()))
                    {
                        PropertyInfo property = properties.FirstOrDefault(pro => pro.Name.Trim() == columns[cellIndex].Code.Trim());
                        object value = property.GetValue(data[rowIndex - startIndex]);
                        SetCellValue(cell, value, property.PropertyType);
                    }
                    cell.CellStyle = cellStyle;
                }
            }
        }

        private void SetCellValue(ICell cell, object value, Type propertyType)
        {
            if (value == null)
            {
                return;
            }
            if (propertyType == typeof(string))
            {
                cell.SetCellType(CellType.String);
                cell.SetCellValue(value.ToString());
            }
            else if (propertyType == typeof(int) || propertyType == typeof(float) ||
                propertyType == typeof(double) || propertyType == typeof(decimal) || propertyType == typeof(long))
            {
                cell.SetCellType(CellType.Numeric);
                cell.SetCellValue(double.Parse(value.ToString()));
            }
            else if (propertyType == typeof(bool))
            {
                cell.SetCellType(CellType.Boolean);
                cell.SetCellValue((bool)value);
            }
        }

        private IRow GetRow(ISheet sheet, int rowIndex)
        {
            IRow row = sheet.GetRow(rowIndex);
            if (row == null)
            {
                row = sheet.CreateRow(rowIndex);
            }
            return row;
        }

        private ICell GetCell(IRow row, int cellIndex)
        {
            ICell cell = row.GetCell(cellIndex);
            if (cell == null)
            {
                cell = row.CreateCell(cellIndex);
            }
            return cell;
        }

        private void BuildHeaderRow(IRow headerRow, List<TableHeader> columns, ICellStyle headerStyle)
        {
            //设置列头
            for (int i = 0; i < columns.Count; i++)
            {
                ICell cell = GetCell(headerRow, i);
                cell.SetCellValue(columns[i].Display);
                cell.CellStyle = headerStyle;
            }
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
    }

    /// <summary>
    /// 表格的列模型
    /// </summary>
    public class TableColumnModel
    {
        public string Code { get; set; }

        public string Name { get; set; }

    }
}
