using iTextSharp.text;
using iTextSharp.text.pdf;
using Localization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ViewModel.Report;
using ViewModel.Visitor;

namespace WebApi.Services.ExportService
{
    /// <summary>
    /// 通过PageEvent 生成页眉，页脚，页码
    /// </summary>
    public class ITextEvents : PdfPageEventHelper
    {
        // This is the contentbyte object of the writer
        PdfContentByte cb;
        // we will put the final number of pages in a template
        PdfTemplate headerTemplate, footerTemplate;
        // this is the BaseFont we are going to use for the header / footer
        BaseFont bf = null;
        // This keeps track of the creation time
        DateTime PrintTime = DateTime.Now;
        #region Fields
        private string _header;
        #endregion

        #region Properties
        public string Header
        {
            get { return _header; }
            set { _header = value; }
        }
        #endregion
        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            try
            {
                PrintTime = DateTime.Now;
                bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                cb = writer.DirectContent;
                headerTemplate = cb.CreateTemplate(100, 100);
                footerTemplate = cb.CreateTemplate(50, 50);
            }
            catch (DocumentException de)
            {

            }
            catch (System.IO.IOException ioe)
            {

            }
        }
        public override void OnEndPage(iTextSharp.text.pdf.PdfWriter writer, iTextSharp.text.Document document)
        {
            base.OnEndPage(writer, document);

            iTextSharp.text.Font baseFontNormal = new iTextSharp.text.Font(Font.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.Black);

            iTextSharp.text.Font baseFontBig = new iTextSharp.text.Font(Font.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.Black);

            // 构造页眉
            GeneratorHeader(document, writer, baseFontNormal);
            // 构造页脚
            GeneratorFooter(document, writer, baseFontNormal);

            // 直接通过pageContent 和模板设置页面内容（水印）
            string text = "Page No. " + writer.PageNumber + " of   ";
            //Add paging to header
            {
                cb.BeginText();
                cb.SetFontAndSize(bf, 12);
                cb.SetTextMatrix(document.PageSize.GetRight(200), document.PageSize.GetTop(35));
                cb.ShowText(text);
                cb.EndText();
                float len = bf.GetWidthPoint(text, 12);
                //Adds "12" in Page 1 of 12
                cb.AddTemplate(headerTemplate, document.PageSize.GetRight(200) + len, document.PageSize.GetTop(35));
            }
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            headerTemplate.BeginText();
            headerTemplate.SetFontAndSize(bf, 12);
            headerTemplate.SetTextMatrix(0, 0);
            headerTemplate.ShowText((writer.PageNumber - 1).ToString());
            headerTemplate.EndText();

            footerTemplate.BeginText();
            footerTemplate.SetFontAndSize(bf, 12);
            footerTemplate.SetTextMatrix(0, 0);
            footerTemplate.ShowText((writer.PageNumber - 1).ToString());
            footerTemplate.EndText();
        }
        /// <summary>
        /// 生成自定义页眉
        /// </summary>
        public void GeneratorHeader(Document docPDF, PdfWriter write, Font font)
        {
            Image png = Image.GetInstance(new Uri(Path.Combine(ExportSimplePDFHelper.currentContentPath, ExportSimplePDFHelper.ImgPath, "tourism2.png")));
            // 2. 比例缩放
            png.ScalePercent(19);
            png.SetAbsolutePosition(0, (PageSize.A4.Height - 75));
            docPDF.Add(png);//将图片添加到pdf文档中
            PdfPTable tabFot = new PdfPTable(new float[] { 25, 30 });
            tabFot.TotalWidth = 200F;
            PdfPCell cellf = new PdfPCell(new Phrase("Date / Time", font));
            cellf.Border = 0;
            tabFot.AddCell(cellf);
            cellf = new PdfPCell(new Phrase($"{PrintTime.ToShortDateString()} {string.Format("{0:t}", DateTime.Now)}", font));
            cellf.Border = 0;
            tabFot.AddCell(cellf);
            tabFot.WriteSelectedRows(0, -1, 393, PageSize.A4.Height - 35, write.DirectContent);
        }
        /// <summary>
        /// 生成自定义页脚
        /// </summary>
        public void GeneratorFooter(Document docPDF, PdfWriter write, Font font)
        {
            Image png = Image.GetInstance(new Uri(Path.Combine(ExportSimplePDFHelper.currentContentPath, ExportSimplePDFHelper.ImgPath, "tourism3.png")));
            png.SetAbsolutePosition(0, 5);
            png.ScalePercent(15);
            png.SpacingAfter = 5;   // 下外邊距
            docPDF.Add(png);
            // 自设计页脚
            //参数rowStart是你想开始的行的数目，参数rowEnd是你想显示的最后的行（如果你想显示所有的行，用 - 1），xPos和yPos是表格的坐标，canvas是一个PdfContentByte对象。在示例代码1009中，我们添加了一个表在(100, 600)处：
            PdfPTable tabFot = new PdfPTable(new float[] { 1F });
            tabFot.TotalWidth = 500F;
            PdfPCell cellf = new PdfPCell(new Phrase("Colina de Mong-Ha, Macau, China Tel: (853) 2851 5222 Fax: (853) 2855 6925 www.ift.edu.mo/pousada", font));
            cellf.Border = 0;
            tabFot.AddCell(cellf);
            tabFot.WriteSelectedRows(0, -1, 66, 25, write.DirectContent);
        }
    }
    public class ExportSimplePDFHelper : ExportSimpleReportHelper
    {
        protected override void SaveFile<T>(string fileName, List<T> selectedData, List<TableHeader> columns, string titleText = null)
        {
            base.SaveFile(fileName, selectedData, columns, titleText);
        }

        public const string basePath = "Services\\Printer\\Fonts";
        public const string ImgPath = "Services\\Printer\\Img";
        public static string currentContentPath = Startup.CurrentContentPath;

        /// <summary>
        /// 构建表格，生成PDF
        /// </summary>
        /// <param name="fileName">保存文件名称，不包括路径</param>
        /// <returns>FileContentResult格式的文件流</returns>
        public static async Task<IActionResult> GeneratePDF(string fileName, SubmitParamViewModel submitParamViewModel, DebitHomeViewModel debitHomeViewModel)
        {
            #region 简单例子
            ExportSimplePDFHelper eph = new ExportSimplePDFHelper();
            if (!Directory.Exists(Startup.TemporaryFileDirectory))
            {
                Directory.CreateDirectory(Startup.TemporaryFileDirectory);
            }
            string filePath = Path.Combine(Startup.TemporaryFileDirectory, fileName);
            using (FileStream wfs = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                Document docPDF = new Document(PageSize.A4, 10, 10, 80, 250);  // 左右上下
                PdfWriter write = PdfWriter.GetInstance(docPDF, wfs);   // 默认字体显示不了中文
                write.PageEvent = new ITextEvents();
                docPDF.Open();

                #region 字体
                // 字体
                // 1. 用到window 本地字体，可以显示中文，第一个参数为本地字体路径，可以复制window 的字体
                BaseFont bsFont = BaseFont.CreateFont(Path.Combine(currentContentPath, basePath, "msyh.ttc,0"), BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                //标题字体
                Font fontTitle = new Font(bsFont, 18);
                fontTitle.SetStyle("bold");
                //文本字体
                Font fontText = new Font(bsFont, 10);
                fontText.SetStyle("bold");
                //填充值字体
                Font fontValue = new Font(bsFont, 10);
                #endregion
                #region 用户信息和出单基本信息表格
                PdfPTable table2 = new PdfPTable(2);
                table2.WidthPercentage = 100;
                table2.SpacingAfter = 10;
                // 左边显示人名和地区
                PdfPCell cell = new PdfPCell();
                cell.MinimumHeight = 5f;
                cell.Border = 0;
                PdfPTable tChildNameAndZone = new PdfPTable(1);
                for (int i = 0; i < 2; i++)
                {
                    if (i == 0)
                        cell = new PdfPCell(new Paragraph(submitParamViewModel.FirstName +
                                        " " + submitParamViewModel.LastName, fontValue));
                    else
                        cell = new PdfPCell(new Paragraph("MaCau", fontValue));
                    cell.MinimumHeight = 5f;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.Border = 0;
                    tChildNameAndZone.AddCell(cell);
                }
                cell = new PdfPCell(tChildNameAndZone);
                cell.Border = 0;
                // 右边表格
                #region 子表格
                PdfPTable tableChild = new PdfPTable(3);
                // 基本信息数据
                string[] tHeaderList = { "Conf. No. ", "Cashier No./Name" ,
                    "No. of Guest ", "Invoice No. "};
                string[] strValueList = { ":  ", ":  ", ":  ", ":  " };
                for (int i = 0; i < tHeaderList.Length; i++)
                {
                    PdfPCell c = new PdfPCell();
                    c.Border = 0;
                    tableChild.AddCell(c);
                    c = new PdfPCell(new Paragraph(tHeaderList[i], new Font(bsFont, 10)));
                    c.Border = 0;
                    c.MinimumHeight = 5f;
                    tableChild.AddCell(c);
                    c = new PdfPCell(new Paragraph(strValueList[i], new Font(bsFont, 10)));
                    c.Border = 0;
                    c.MinimumHeight = 5f;
                    tableChild.AddCell(c);
                }

                #endregion
                PdfPCell cell2 = new PdfPCell(tableChild);
                cell2.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell2.HorizontalAlignment = Element.ALIGN_CENTER;
                cell2.Border = 0;
                table2.AddCell(cell);
                table2.AddCell(cell2);
                docPDF.Add(table2);
                #endregion
                #region 房间列表
                // 标题
                Paragraph pRoom = new Paragraph("ROOM LIST ", fontValue);
                pRoom.Alignment = Element.ALIGN_MIDDLE;
                pRoom.Alignment = Element.ALIGN_CENTER;
                docPDF.Add(pRoom);
                PdfPTable tableRoom = new PdfPTable(6);
                tableRoom.SpacingBefore = 5;
                tableRoom.SpacingAfter = 15;
                tableRoom.WidthPercentage = 100;
                string[] tRHeaderList = { "RoomType ", "Amount ", "Arrival ", "Departure ", "Night ", "Rate" };
                List<List<string>> strRValueList = new List<List<string>>();
                // 构造值数组
                for (int i = 0; i < submitParamViewModel.BookingRoomGroup.Count(); i++)
                {
                    var item = submitParamViewModel.BookingRoomGroup[i];
                    List<string> tValueList = new List<string>();
                    //tValueList.Add(""); // RoomNo
                    tValueList.Add(item.RoomType); // RoomType
                    tValueList.Add(item.Amount.ToString()); // Amount
                    tValueList.Add(item.ArrivalDate);    // Arrival
                    tValueList.Add(item.DepartureDate);  // Departure
                    // 计算天数差
                    TimeSpan span = Convert.ToDateTime(item.DepartureDate).Subtract(Convert.ToDateTime(item.ArrivalDate));
                    int dayDiff = span.Days;
                    tValueList.Add(dayDiff.ToString());
                    tValueList.Add(item.Rate);  // Rate
                    strRValueList.Add(tValueList);
                }
                // 房间表格表头
                for (int i = 0; i < tRHeaderList.Length; i++)
                {
                    PdfPCell cell3_1 = new PdfPCell(new Paragraph(tRHeaderList[i], new Font(bsFont, 10)));
                    cell3_1.Border = 0;
                    cell3_1.PaddingBottom = 5;
                    cell3_1.BorderWidthBottom = 1;
                    cell3_1.BorderWidthTop = 1;
                    cell3_1.BackgroundColor = new BaseColor(235, 235, 235);
                    cell3_1.MinimumHeight = 5f;
                    tableRoom.AddCell(cell3_1);
                }
                // 房间表格数据
                for (int i = 0; i < strRValueList.Count(); i++)
                {
                    foreach (var item in strRValueList[i])
                    {
                        PdfPCell pc = new PdfPCell(new Paragraph(item, fontValue));
                        pc.Border = 0;
                        // 是否加下边框
                        if (i == strRValueList.Count() - 1)
                        {
                            pc.BorderWidthBottom = 1;
                            pc.PaddingBottom = 5;
                        }
                        tableRoom.AddCell(pc);
                    }
                }
                docPDF.Add(tableRoom);
                #endregion
                Paragraph p = new Paragraph("INFORMATION INVOICE", fontValue);
                p.Alignment = Element.ALIGN_MIDDLE;
                p.Alignment = Element.ALIGN_CENTER;
                docPDF.Add(p);
                #region 客户模板表格
                //float[] left = { 25, 150, 25, 25 };
                //PdfPTable table3 = new PdfPTable(left);
                //table3.WidthPercentage = 100;
                //// 外边距
                //table3.SpacingAfter = 5;
                //table3.SpacingBefore = 5;
                //string[] titles = { "Date", "Description", "Debit(+)", "Credit(-)" };
                //// 表头
                //for (int k = 0; k < titles.Length; k++)
                //{
                //    PdfPCell cell3_1 = new PdfPCell(new Paragraph(titles[k], new Font(bsFont, 10)));
                //    cell3_1.Border = 0;
                //    cell3_1.PaddingBottom = 5;
                //    cell3_1.BorderWidthBottom = 1;
                //    cell3_1.BorderWidthTop = 1;
                //    cell3_1.BackgroundColor = new BaseColor(235, 235, 235);
                //    cell3_1.MinimumHeight = 5f;
                //    table3.AddCell(cell3_1);
                //}
                //List<List<string>> contents = new List<List<string>>();
                //#region 添加一条测试数据
                ////debitHomeViewModel.DebitNoteGroup.Add(new DebitNote() { 
                ////    OperateCode = "",
                ////    NoteCode = "",
                ////    BookingCode = "",
                ////    Balance = "-330.00",
                ////    PaymentMethod = "xxx",
                ////    PaymentType = "xxx"
                ////});
                //#endregion
                //// 构造表单数据
                //for (int i = 0; i < debitHomeViewModel.DebitNoteGroup.Count(); i++)
                //{
                //    List<string> strList = new List<string>();
                //    var item = debitHomeViewModel.DebitNoteGroup[i];
                //    //strList.Add(string.Format("{0:yy/MM/dd}", DateTime.Now));    // Date
                //    strList.Add("");    // Date
                //    strList.Add("");  // Description 单据种类：Hostel Fee
                //    strList.Add(item.Balance.First().Equals('-') ? "" : item.Balance);  // Debit: Balance 为正
                //    strList.Add(item.Balance.First().Equals('-') ? item.Balance.Substring(1) : "");  // Crebit: Balance 为负
                //    //strList.Add($"{item.NoteCode} : {item.OperateCode} : {item.PaymentMethod} : {item.PaymentType}");  // Crebit: Balance 为负
                //    contents.Add(strList);
                //}
                //for (int j = 0; j < contents.Count(); j++)
                //{
                //    for (int i = 0; i < contents[j].Count(); i++)
                //    {
                //        PdfPCell cell3_1 = new PdfPCell();
                //        //cell3_1.MinimumHeight = 5f;
                //        cell3_1.Border = 0;
                //        //if (i == contents[j].Count() - 1) // 有最后一行总体介绍时开启，没有数据暂时不开启
                //        //{
                //        if (j == contents.Count() - 1)
                //        {
                //            cell3_1.PaddingBottom = 5;
                //            cell3_1.BorderWidthBottom = 1;
                //        }
                //        //    table3.AddCell(cell3_1);
                //        //    cell3_1.Colspan = 3;
                //        //    cell3_1.PaddingLeft = 20;
                //        //}
                //        cell3_1.AddElement(new Paragraph(contents[j][i], fontValue));
                //        table3.AddCell(cell3_1);
                //    }
                //}
                //docPDF.Add(table3);
                #endregion
                #region 页面格式表格
                float[] tPageModelFormat = { 40, 30, 30, 16, 16 };
                PdfPTable tPageModel = new PdfPTable(tPageModelFormat);
                tPageModel.WidthPercentage = 100;
                // 前后占位
                tPageModel.SpacingAfter = 5;
                tPageModel.SpacingBefore = 5;
                // 单据编号，开单类型（暂时没有），收费方式，Balance：拆分为Debit(+) 和 Credit(-)
                string[] tPageModelHeader = { "NoteCode", "NoteType", "PayMethod", "Debit", "Credit" };
                // 表头
                for (int k = 0; k < tPageModelHeader.Length; k++)
                {
                    PdfPCell cell3_1 = new PdfPCell(new Paragraph(tPageModelHeader[k], fontValue));
                    cell3_1.Border = 0;
                    cell3_1.PaddingBottom = 5;
                    cell3_1.BorderWidthBottom = 1;
                    cell3_1.BorderWidthTop = 1;
                    cell3_1.BackgroundColor = new BaseColor(235, 235, 235);
                    cell3_1.MinimumHeight = 5f;
                    tPageModel.AddCell(cell3_1);
                }
                List<List<string>> cPageModelList = new List<List<string>>();
                // 构造表单数据
                for (int i = 0; i < debitHomeViewModel.DebitNoteGroup.Count(); i++)
                {
                    List<string> strList = new List<string>();
                    var item = debitHomeViewModel.DebitNoteGroup[i];
                    if (string.IsNullOrEmpty(item.NoteCode))
                        continue;
                    strList.Add(item.NoteCode);    // NoteCode
                    strList.Add(item.PaymentType);  // NoteType
                    strList.Add(item.PaymentMethod); // PayType
                    strList.Add(item.Balance.First().Equals('-') ? "" : Convert.ToDecimal(item.Balance).ToString("#0.00"));  // Debit: Balance 为正
                    strList.Add(item.Balance.First().Equals('-') ? Convert.ToDecimal(item.Balance.Substring(1)).ToString("#0.00") : "");  // Crebit: Balance 为负
                    cPageModelList.Add(strList);
                }
                for (int j = 0; j < cPageModelList.Count(); j++)
                {
                    for (int i = 0; i < cPageModelList[j].Count(); i++)
                    {
                        PdfPCell cell3_1 = new PdfPCell();
                        //cell3_1.MinimumHeight = 5f;
                        cell3_1.Border = 0;
                        if (j == cPageModelList.Count() - 1)
                        {
                            cell3_1.PaddingBottom = 5;
                            cell3_1.BorderWidthBottom = 1;
                        }
                        cell3_1.AddElement(new Paragraph(cPageModelList[j][i], fontValue));
                        tPageModel.AddCell(cell3_1);
                    }
                }
                docPDF.Add(tPageModel);
                #endregion
                #region 签名表格，绝对位置
                PdfPTable tableSign = new PdfPTable(2);
                // 绝对位置,不能设置为宽度比例要直接确定的值
                tableSign.TotalWidth = 600F;
                #region 
                // 左列
                PdfPCell cellS1 = new PdfPCell();
                cellS1.Border = 0;
                tableSign.AddCell(cellS1);
                // 右列 - 子列表
                #region 子列表
                PdfPCell cellS2 = new PdfPCell();
                cellS2.Border = 0;
                PdfPTable tSignChild = new PdfPTable(new float[] { 100f, 180f });
                tSignChild.WidthPercentage = 95;
                tSignChild.HorizontalAlignment = Element.ALIGN_LEFT;
                string[] tsTitle = { "Total", "Balance" };
                // Debit(+) Credit(-)
                #region 计算总数值, 过滤掉未提交
                decimal b = new decimal(0); // balance 总数
                decimal Debit = new decimal(0);
                decimal Credit = new decimal(0);
                foreach (var item in debitHomeViewModel.DebitNoteGroup)
                {
                    if (string.IsNullOrEmpty(item.NoteCode))
                        continue;
                    decimal balance = Convert.ToDecimal(item.Balance);
                    if (balance < 0)
                    {
                        Credit += balance;
                    }
                    else
                    {
                        Debit += balance;
                    }
                    b += balance;
                }
                #endregion
                for (int i = 0; i < tsTitle.Length; i++)
                {
                    PdfPCell cellS21 = new PdfPCell(new Phrase(tsTitle[i], fontValue));
                    cellS21.PaddingBottom = 5;
                    cellS21.PaddingTop = 10;
                    cellS21.Border = 0;
                    tSignChild.AddCell(cellS21);
                    PdfPTable tGrandChild = new PdfPTable(new float[] { 40, 30 });
                    tGrandChild.WidthPercentage = 100;
                    // 子表格
                    for (int t = 0; t < 2; t++)
                    {
                        if (t == 0) // 第一格
                        {
                            if (i == 0)
                                cellS21 = new PdfPCell(new Paragraph(Debit.ToString("#0.00"), fontValue));
                            else
                                cellS21 = new PdfPCell(new Paragraph(b.ToString("#0.00"), fontValue));
                        }
                        else // 第二格
                        {
                            cellS21 = new PdfPCell();
                            if (i == 0)  // Credit
                                cellS21 = new PdfPCell(new Paragraph((Credit * (-1)).ToString("#0.00"), fontValue));
                        }
                        cellS21.PaddingBottom = 5;
                        cellS21.PaddingTop = 10;
                        cellS21.Border = 0;
                        cellS21.HorizontalAlignment = Element.ALIGN_RIGHT;
                        tGrandChild.AddCell(cellS21);
                    }
                    cellS21 = new PdfPCell(tGrandChild);
                    cellS21.Border = 0;
                    cellS21.BorderWidthBottom = 1;
                    tSignChild.AddCell(cellS21);
                }

                // 第二行
                PdfPCell cellS3 = new PdfPCell(new Phrase("I agree that my liability for this account is not waived and I agree to be held personally liable " +
                    "in the event that the indicated person, company or association fails to pay all or part of " +
                    "these charges.", new Font(bsFont, 9)));
                cellS3.Colspan = 2;
                cellS3.PaddingBottom = 40;
                cellS3.PaddingTop = 3;
                cellS3.Border = 0;
                cellS3.BorderWidthBottom = 1;
                tSignChild.AddCell(cellS3);
                // 第三行
                PdfPCell cellS4 = new PdfPCell(new Phrase("Guest Signature", fontValue));
                cellS4.Colspan = 2;
                cellS4.PaddingBottom = 5;
                cellS4.PaddingTop = 3;
                cellS4.Border = 0;
                cellS4.HorizontalAlignment = Element.ALIGN_CENTER;
                cellS4.VerticalAlignment = Element.ALIGN_MIDDLE;
                tSignChild.AddCell(cellS4);
                #endregion
                cellS2.AddElement(tSignChild);
                tableSign.AddCell(cellS2);
                #endregion

                tableSign.WriteSelectedRows(0, -1, 0, 220, write.DirectContent);
                //docPDF.Add(tableSign);    // 绝对位置不用添加
                #endregion
                docPDF.Close();//关闭
                write.Close();
                wfs.Close();
                #endregion
            }
            return new FileContentResult(File.ReadAllBytes(filePath), "application/pdf");
        }
    }
}
