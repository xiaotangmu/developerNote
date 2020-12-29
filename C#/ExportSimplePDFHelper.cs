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
            string text = "Page No. " + writer.PageNumber + " of   " ;
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
            //Add paging to footer 只是生成 1 of 2
            //{
            //    cb.BeginText();
            //    cb.SetFontAndSize(bf, 12);
            //    cb.SetTextMatrix(document.PageSize.GetRight(180), document.PageSize.GetBottom(30));
            //    cb.ShowText(text);
            //    cb.EndText();
            //    float len = bf.GetWidthPoint(text, 12);
            //    cb.AddTemplate(footerTemplate, document.PageSize.GetRight(180) + len, document.PageSize.GetBottom(30));
            //}

            ////Move the pointer and draw line to separate header section from rest of page
            //// 可以理解为 聚焦、画线
            //cb.MoveTo(40, document.PageSize.Height - 100);    // 聚焦
            //cb.LineTo(document.PageSize.Width - 40, document.PageSize.Height - 100);  // 画线
            //cb.Stroke();
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
            //public float writeSelectedRows(int rowStart, int rowEnd, float xPos, float yPos, PdfContentByte canvas);
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

        //private const string _basePath = "Services\\Printer\\Templates";
        public const string basePath = "Services\\Printer\\Fonts";
        public const string ImgPath = "Services\\Printer\\Img";
        public static string currentContentPath = Startup.CurrentContentPath;

        /// <summary>
        /// 生成页眉
        /// </summary>
        public static void GeneratorHeader(Document docPDF)
        {
            Image png = Image.GetInstance(new Uri(Path.Combine(currentContentPath, ImgPath, "tourism2.png")));
            // 2. 比例缩放
            png.ScalePercent(19);
            png.SetAbsolutePosition(0, (PageSize.A4.Height - 75));
            docPDF.Add(png);//将图片添加到pdf文档中
            
        }
        /// <summary>
        /// 生成页脚
        /// </summary>
        public static void GeneratorFooter(Document docPDF, PdfWriter write, Font font)
        {
            Image png = Image.GetInstance(new Uri(Path.Combine(currentContentPath, ImgPath, "tourism3.png")));
            png.SetAbsolutePosition(0, 5);
            png.ScalePercent(15);
            png.SpacingAfter = 5;   // 下外邊距
            docPDF.Add(png);
            // 自设计页脚
            //public float writeSelectedRows(int rowStart, int rowEnd, float xPos, float yPos, PdfContentByte canvas);
            //参数rowStart是你想开始的行的数目，参数rowEnd是你想显示的最后的行（如果你想显示所有的行，用 - 1），xPos和yPos是表格的坐标，canvas是一个PdfContentByte对象。在示例代码1009中，我们添加了一个表在(100, 600)处：
            PdfPTable tabFot = new PdfPTable(new float[] { 1F });
            tabFot.TotalWidth = 500F;
            PdfPCell cellf = new PdfPCell(new Phrase("Colina de Mong-Ha, Macau, China Tel: (853) 2851 5222 Fax: (853) 2855 6925 www.ift.edu.mo/pousada", font));
            cellf.Border = 0;
            tabFot.AddCell(cellf);
            tabFot.WriteSelectedRows(0, -1, 66, 25, write.DirectContent);
        }
        /// <summary>
        /// 生成新页
        /// </summary>
        public static void NewPage()
        {

        }
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
                //PageSize.A4.Rotate();当需要把PDF纸张设置为横向时
                Document docPDF = new Document(PageSize.A4, 10, 10, 80, 250);  // 左右上下
                PdfWriter write = PdfWriter.GetInstance(docPDF, wfs);   // 默认字体显示不了中文
                write.PageEvent = new ITextEvents();
                docPDF.Open();
                
                #region 字体
                // 字体
                // 1. 用到window 本地字体，可以显示中文，第一个参数为本地字体路径，可以复制window 的字体
                BaseFont bsFont = BaseFont.CreateFont(Path.Combine(currentContentPath, basePath, "msyh.ttc,0"), BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                // 2. 使用自带字体，在这里需要注意的是，itextsharp不支持中文字符，想要显示中文字符的话需要自己设置字体 
                //BaseFont bsFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                //标题字体
                Font fontTitle = new Font(bsFont, 18);
                fontTitle.SetStyle("bold");
                //fontTitle.SetFamily("微软雅黑");
                //文本字体
                Font fontText = new Font(bsFont, 10);
                fontText.SetStyle("bold");
                //fontText.SetFamily("黑体");
                //填充值字体
                Font fontValue = new Font(bsFont, 10);
                //fontValue.SetFamily("黑体");
                #endregion
                #region 用户信息和出单基本信息表格
                PdfPTable table2 = new PdfPTable(2);
                table2.WidthPercentage = 100;
                // 外边距
                table2.SpacingAfter = 10;
                //table2.SpacingBefore = 5;
                // 左边显示人名和地区
                PdfPCell cell = new PdfPCell();
                cell.MinimumHeight = 5f;
                cell.Border = 0;
                PdfPTable tChildNameAndZone = new PdfPTable(1);
                for(int i = 0; i < 2; i++)
                {
                    if(i == 0)
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
                //cell2.MinimumHeight = 5f;
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
                string[] tPageModelHeader = { "NoteCode", "NoteType", "PayMethod", "Debit(+)", "Credit(-)" };
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
                    strList.Add("");  // NoteType
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
                //tableSign.WidthPercentage = 100; // 失败
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
    /// <summary>
    /// 构建表格，生成PDF
    /// </summary>
    /// <param name="fileName">保存文件名称，不包括路径</param>
    /// <param name="model">数据模型</param>
    /// <param name="currentUser">当前用户信息</param>
    /// <returns>FileContentResult格式的文件流</returns>
    //public static async Task<IActionResult> GeneratePDF(string fileName, AssetInfoModel model, CurrentUserInfo currentUser)
    public static async Task<IActionResult> GeneratePDF()
    {
        #region
        //    if (model.AssetInfo == null)
        //    {
        //        model.AssetInfo = new AssetInfo();
        //    }
        //    if (model.AssetCurrent == null)
        //    {
        //        model.AssetCurrent = new AssetCurrent();
        //    }
        //    if (model.AssetMain == null)
        //    {
        //        model.AssetMain = new AssetMaintenance();
        //    }
        //    if (!Directory.Exists(Startup.TemporaryFileDirectory))
        //    {
        //        Directory.CreateDirectory(Startup.TemporaryFileDirectory);
        //    }
        //    string filePath = Path.Combine(Startup.TemporaryFileDirectory, fileName);
        //    using (FileStream wfs = new FileStream(filePath, FileMode.OpenOrCreate))
        //    {
        //        //PageSize.A4.Rotate();当需要把PDF纸张设置为横向时
        //        Document docPDF = new Document(PageSize.A4, 10, 10, 20, 20);
        //        PdfWriter write = PdfWriter.GetInstance(docPDF, wfs);
        //        docPDF.Open();
        //        //在这里需要注意的是，itextsharp不支持中文字符，想要显示中文字符的话需要自己设置字体 
        //        BaseFont bsFont = BaseFont.CreateFont(Path.Combine(Startup.CurrentContentPath, _basePath, "simsun.ttc,0"), BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
        //        //标题字体
        //        Font fontTitle = new Font(bsFont, 18);
        //        fontTitle.SetStyle("bold");
        //        fontTitle.SetFamily("黑体");
        //        //文本字体
        //        Font fontText = new Font(bsFont, 9);
        //        fontText.SetStyle("bold");
        //        fontText.SetFamily("黑体");
        //        //填充值字体
        //        Font fontValue = new Font(bsFont, 9);
        //        fontValue.SetFamily("黑体");
        //        // 宽度
        //        float[] clos = new float[] { 25, 25, 25, 25, 25, 25 };
        //        PdfPTable table = new PdfPTable(clos);
        //        #region 标题
        //        PdfPCell cellTitle = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("Card"), fontTitle));
        //        cellTitle.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cellTitle.DisableBorderSide(Rectangle.TOP_BORDER);
        //        cellTitle.DisableBorderSide(Rectangle.LEFT_BORDER);
        //        cellTitle.DisableBorderSide(Rectangle.RIGHT_BORDER);
        //        //cellTitle.Border = Rectangle.RIGHT_BORDER | Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER;
        //        cellTitle.Colspan = 6;
        //        table.AddCell(cellTitle);
        //        #endregion
        //        #region 第1行
        //        PdfPCell cell1_1 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("AssetCode"), fontText));
        //        cell1_1.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell1_2 = new PdfPCell(new Paragraph(model.AssetInfo.AssetCode, fontValue));
        //        cell1_2.MinimumHeight = 5f;
        //        cell1_2.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell1_3 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("AssetName"), fontText));
        //        cell1_3.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell1_4 = new PdfPCell(new Paragraph(model.AssetInfo.AssetName, fontValue));
        //        cell1_4.MinimumHeight = 5f;
        //        cell1_4.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell1_5 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("IRange_Type_Asset_Category"), fontText));
        //        cell1_5.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        //资产分类
        //        PdfPCell cell1_6 = new PdfPCell();
        //        var category = await new AssetManagement().GetAssetCateGory(currentUser.SuperId, model.AssetInfo.ACCode);
        //        if (category != null)
        //        {
        //            cell1_6.Phrase = new Paragraph(category.AC_NAME, fontValue);
        //            cell1_6.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        }
        //        table.AddCell(cell1_1);
        //        table.AddCell(cell1_2);
        //        table.AddCell(cell1_3);
        //        table.AddCell(cell1_4);
        //        table.AddCell(cell1_5);
        //        table.AddCell(cell1_6);
        //        #endregion
        //        #region 第2行
        //        PdfPCell cell2_1 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("AssetSpecification"), fontText));
        //        cell2_1.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell2_2 = new PdfPCell(new Paragraph(model.AssetInfo.AssetType, fontValue));
        //        cell2_2.MinimumHeight = 5f;
        //        cell2_2.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell2_3 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("SN"), fontText));
        //        cell2_3.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell2_4 = new PdfPCell(new Paragraph(model.AssetInfo.AssetSN, fontValue));
        //        cell2_4.MinimumHeight = 5f;
        //        cell2_4.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell2_5 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("Unit"), fontText));
        //        cell2_5.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        //资产分类
        //        PdfPCell cell2_6 = new PdfPCell(new Paragraph(model.AssetInfo.ACUnit, fontValue));
        //        cell2_6.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        table.AddCell(cell2_1);
        //        table.AddCell(cell2_2);
        //        table.AddCell(cell2_3);
        //        table.AddCell(cell2_4);
        //        table.AddCell(cell2_5);
        //        table.AddCell(cell2_6);
        //        #endregion
        //        #region 第3行
        //        PdfPCell cell3_1 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("Amount"), fontText));
        //        cell3_1.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell3_2 = new PdfPCell(new Paragraph(model.AssetInfo.AssetPrice, fontValue));
        //        cell3_2.MinimumHeight = 5f;
        //        cell3_2.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell3_3 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("UsageOrganization"), fontText));
        //        cell3_3.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell3_4 = new PdfPCell();
        //        var currentUsingOrganization = await new OrganizationManagement().GetBySmidAndCode(currentUser.SuperId, model.AssetCurrent.CurrentOrgCode);
        //        if (currentUsingOrganization != null)
        //        {
        //            cell3_4.Phrase = new Paragraph(currentUsingOrganization.ORGANIZATION_NAME, fontValue);
        //            cell3_4.MinimumHeight = 5f;
        //            cell3_4.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        }
        //        PdfPCell cell3_5 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("UsageDepartment"), fontText));
        //        cell3_5.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        //资产分类
        //        PdfPCell cell3_6 = new PdfPCell();
        //        //资产使用部门
        //        var currentDepartment = await new OrganizationManagement().GetDepartment(currentUser.SuperId, model.AssetCurrent.CurrentDepCode, currentUsingOrganization?.ID);
        //        if (currentDepartment != null)
        //        {
        //            cell3_6.Phrase = new Paragraph(currentDepartment.DEPART_NAME, fontValue);
        //            cell3_6.MinimumHeight = 5f;
        //            cell3_6.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        }
        //        table.AddCell(cell3_1);
        //        table.AddCell(cell3_2);
        //        table.AddCell(cell3_3);
        //        table.AddCell(cell3_4);
        //        table.AddCell(cell3_5);
        //        table.AddCell(cell3_6);
        //        #endregion
        //        #region 第4行
        //        PdfPCell cell4_1 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("UsageUser"), fontText));
        //        cell4_1.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell4_2 = new PdfPCell();
        //        //资产使用人（员工）
        //        if (currentUsingOrganization != null && currentDepartment != null && model != null && model.AssetCurrent != null && !string.IsNullOrEmpty(model.AssetCurrent.CurrentUserCode))
        //        {
        //            var currentStaff = await new OrganizationManagement().GetStaff(currentUsingOrganization.ID, currentDepartment.ID, model.AssetCurrent.CurrentUserCode);
        //            if (currentStaff != null)
        //            {
        //                cell4_2.Phrase = new Paragraph(currentStaff.STAFF_NAME, fontValue);
        //                cell4_2.VerticalAlignment = Element.ALIGN_MIDDLE;
        //            }
        //        }
        //        PdfPCell cell4_3 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("PurchaseDate"), fontText));
        //        cell4_3.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell4_4 = new PdfPCell(new Paragraph(model.AssetInfo.PurDate, fontValue));
        //        cell4_4.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell4_5 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("AssetSource"), fontText));
        //        cell4_5.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell4_6 = new PdfPCell();
        //        cell4_6.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        if (!string.IsNullOrEmpty(model.AssetInfo.AssetFrom))
        //        {
        //            string fromText = await new DictBLL().GetDictDataText("AssetSource", model.AssetInfo.AssetFrom, true);
        //            cell4_6.Phrase = new Paragraph(fromText, fontValue);
        //        }
        //        table.AddCell(cell4_1);
        //        table.AddCell(cell4_2);
        //        table.AddCell(cell4_3);
        //        table.AddCell(cell4_4);
        //        table.AddCell(cell4_5);
        //        table.AddCell(cell4_6);
        //        #endregion
        //        #region 第5行
        //        PdfPCell cell5_1 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("IRange_Type_OwnCompany"), fontText));
        //        cell5_1.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell5_2 = new PdfPCell();
        //        //资产所属公司
        //        var belongOrganization = await new OrganizationManagement().GetBySmidAndCode(currentUser.SuperId, model.AssetInfo.OrgCode);
        //        if (belongOrganization != null)
        //        {
        //            cell5_2.Phrase = new Paragraph(belongOrganization.ORGANIZATION_NAME, fontValue);
        //            cell5_2.MinimumHeight = 5f;
        //            cell5_2.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        }
        //        PdfPCell cell5_3 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("UsageArea"), fontText));
        //        cell5_3.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell5_4 = new PdfPCell();
        //        //区位
        //        var area = await new AssetAreaManagement().GetArea(currentUser.SuperId, model.AssetCurrent.AreaCode);
        //        if (area != null)
        //        {
        //            cell5_4.Phrase = new Paragraph(area.AA_NAME, fontValue);
        //            cell5_4.MinimumHeight = 5f;
        //            cell5_4.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        }
        //        PdfPCell cell5_5 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("Age"), fontText));
        //        cell5_1.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell5_6 = new PdfPCell();
        //        //使用期限（月），需要将资产的年期限进行转换
        //        if (!string.IsNullOrEmpty(model.AssetInfo.ACLife))
        //        {
        //            if (int.TryParse(model.AssetInfo.ACLife, out int year))
        //            {
        //                cell5_6.Phrase = new Paragraph((year * 12).ToString(), fontValue);
        //                cell5_6.VerticalAlignment = Element.ALIGN_MIDDLE;
        //            }
        //        }
        //        //PdfPCell cell5_5 = new PdfPCell(new Paragraph("存放地点", fontText));
        //        //cell5_5.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        ////存放地点
        //        //PdfPCell cell5_6 = new PdfPCell(new Paragraph(model.AssetCurrent.Location, fontValue));
        //        //cell5_6.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        table.AddCell(cell5_1);
        //        table.AddCell(cell5_2);
        //        table.AddCell(cell5_3);
        //        table.AddCell(cell5_4);
        //        table.AddCell(cell5_5);
        //        table.AddCell(cell5_6);
        //        #endregion
        //        #region 第6行
        //        PdfPCell cell6_1 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("Supplier"), fontText));
        //        cell6_1.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell6_2 = new PdfPCell(new Paragraph(model.AssetMain.Supplier, fontValue));
        //        cell6_2.MinimumHeight = 5f;
        //        cell6_2.VerticalAlignment = Element.ALIGN_MIDDLE;

        //        PdfPCell cell6_3 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("ContractPerson"), fontText));
        //        cell6_1.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell6_4 = new PdfPCell(new Paragraph(model.AssetMain.Contacts, fontValue));
        //        cell6_2.MinimumHeight = 5f;
        //        cell6_2.VerticalAlignment = Element.ALIGN_MIDDLE;

        //        PdfPCell cell6_5 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("ContractTel"), fontText));
        //        cell6_1.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell6_6 = new PdfPCell(new Paragraph(model.AssetMain.TelNum, fontValue));
        //        cell6_2.MinimumHeight = 5f;
        //        cell6_2.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        table.AddCell(cell6_1);
        //        table.AddCell(cell6_2);
        //        table.AddCell(cell6_3);
        //        table.AddCell(cell6_4);
        //        table.AddCell(cell6_5);
        //        table.AddCell(cell6_6);
        //        #endregion
        //        #region 第7行
        //        PdfPCell cell7_1 = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("Remark"), fontText));
        //        cell7_1.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        PdfPCell cell7_2 = new PdfPCell(new Paragraph(model.AssetInfo.AssetDes, fontValue));
        //        cell7_2.MinimumHeight = 5f;
        //        cell7_2.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        cell7_2.Colspan = 5;
        //        table.AddCell(cell7_1);
        //        table.AddCell(cell7_2);
        //        #endregion
        //        #region 变更记录
        //        //标题
        //        PdfPCell cellRecord = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("ChangeLog"), fontValue));
        //        cellRecord.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        cellRecord.Colspan = 6;
        //        table.AddCell(cellRecord);
        //        //表头
        //        PdfPCell cellTitleDate = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("ProcessingDate"), fontText));
        //        cellTitleDate.HorizontalAlignment = Element.ALIGN_CENTER;
        //        PdfPCell cellTitleCharge = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("DealingPeople"), fontText));
        //        cellTitleCharge.HorizontalAlignment = Element.ALIGN_CENTER;
        //        PdfPCell cellTitleType = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("DealingMethod"), fontText));
        //        cellTitleType.HorizontalAlignment = Element.ALIGN_CENTER;
        //        PdfPCell cellTitleRemark = new PdfPCell(new Paragraph(await Localizer.GetValueAsync("DealingContent"), fontText));
        //        cellTitleRemark.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cellTitleRemark.Colspan = 3;
        //        table.AddCell(cellTitleDate);
        //        table.AddCell(cellTitleCharge);
        //        table.AddCell(cellTitleType);
        //        table.AddCell(cellTitleRemark);
        //        //资产变更信息
        //        var assetInfo = await new AssetsOperationBLL().GetAssetInfo(currentUser, model.AssetInfo.AssetCode);
        //        if (assetInfo == null)
        //        {
        //            throw new Exception(await Localizer.GetValueAsync("NoExistAssetInformation"));
        //        }
        //        var modification = await new AssetManagement().GetModifyRecord(currentUser.SuperId, assetInfo.ID);
        //        if (modification != null)
        //        {
        //            foreach (var record in modification)
        //            {
        //                PdfPCell cellDate = new PdfPCell(new Paragraph(record.OrderDate, fontValue));
        //                cellDate.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                PdfPCell cellCharge = new PdfPCell(new Paragraph(record.Audit, fontValue));
        //                cellCharge.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                PdfPCell cellType = new PdfPCell(new Paragraph(record.Type, fontValue));
        //                cellType.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                PdfPCell cellRemark = new PdfPCell(new Paragraph(record.Remark, fontValue));
        //                cellRemark.Colspan = 3;
        //                cellRemark.MinimumHeight = 5f;
        //                cellRemark.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                table.AddCell(cellDate);
        //                table.AddCell(cellCharge);
        //                table.AddCell(cellType);
        //                table.AddCell(cellRemark);
        //            }
        //        }
        //        #endregion
        //        docPDF.Add(table);//将表格添加到pdf文档中

        //        docPDF.Close();//关闭
        //        write.Close();
        //        wfs.Close();
        //    }

        //    return new FileContentResult(File.ReadAllBytes(filePath), "application/pdf");
        #endregion
        #region 简单例子
        ExportSimplePDFHelper eph = new ExportSimplePDFHelper();
        if (!Directory.Exists(Startup.TemporaryFileDirectory))
        {
            Directory.CreateDirectory(Startup.TemporaryFileDirectory);
        }
        string fileName = "test.pdf";
        string filePath = Path.Combine(Startup.TemporaryFileDirectory, fileName);
        using (FileStream wfs = new FileStream(filePath, FileMode.OpenOrCreate))
        {
            //PageSize.A4.Rotate();当需要把PDF纸张设置为横向时
            Document docPDF = new Document(PageSize.A4, 10, 10, 7, 7);  // 左右上下
            PdfWriter write = PdfWriter.GetInstance(docPDF, wfs);   // 默认字体显示不了中文
            docPDF.Open();
            string basePath = "Services\\Printer\\Fonts";
            string ImgPath = "Services\\Printer\\Img";
            string currentContentPath = Startup.CurrentContentPath;
            #region 字体
            // 字体
            // 1. 用到window 本地字体，可以显示中文，第一个参数为本地字体路径，可以复制window 的字体
            BaseFont bsFont = BaseFont.CreateFont(Path.Combine(currentContentPath, basePath, "simsun.ttc,0"), BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
            // 2. 使用自带字体，在这里需要注意的是，itextsharp不支持中文字符，想要显示中文字符的话需要自己设置字体 
            //BaseFont bsFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            //标题字体
            Font fontTitle = new Font(bsFont, 18);
            fontTitle.SetStyle("bold");
            fontTitle.SetFamily("微软雅黑");
            //文本字体
            Font fontText = new Font(bsFont, 10);
            fontText.SetStyle("bold");
            //fontText.SetFamily("黑体");
            //填充值字体
            Font fontValue = new Font(bsFont, 10);
            //fontValue.SetFamily("黑体");
            #endregion
            #region 图片
            // 插入图片，默认为块，左对齐
            Image png = Image.GetInstance(new Uri(Path.Combine(currentContentPath, ImgPath, "tourism2.png")));
            // 设置绝对位置，注意位置左下角才为（0，0）此时图片为背景图片，可以被覆盖
            //png.SetAbsolutePosition(0,750);
            // 缩放
            // 1. 直接设置图片像素
            //png.ScaleAbsolute(250, 70);
            // 2. 比例缩放
            png.ScalePercent(19);
            docPDF.Add(png);//将图片添加到pdf文档中
            png = Image.GetInstance(new Uri(Path.Combine(currentContentPath, ImgPath, "tourism3.png")));
            png.SetAbsolutePosition(0, 5);
            png.ScalePercent(15);
            docPDF.Add(png);
            #endregion
            #region 块
            // 块
            //Chunk chunk1 = new Chunk("This text is underlined", FontFactory.GetFont(FontFactory.HELVETICA, 12, Font.UNDERLINE));
            //Chunk chunk2 = new Chunk("This font is of type ITALIC | STRIKETHRU", FontFactory.GetFont(FontFactory.HELVETICA, 12, Font.ITALIC | Font.STRIKETHRU));
            //docPDF.Add(chunk1);
            //docPDF.Add(chunk2);
            #endregion
            #region 短句
            //// 短句
            //Phrase phrases1 = new Phrase("This text is underlined", FontFactory.GetFont(FontFactory.HELVETICA, 12, Font.UNDERLINE));
            //Phrase phrases2 = new Phrase("This font is of type ITALIC | STRIKETHRU", FontFactory.GetFont(FontFactory.HELVETICA, 12, Font.ITALIC | Font.STRIKETHRU));
            //docPDF.Add(phrases1);
            //docPDF.Add(phrases2);
            #endregion
            #region 段落
            //// 段落
            //Paragraph p1 = new Paragraph(new Chunk("This is my first paragraph.", FontFactory.GetFont(FontFactory.HELVETICA, 12)));
            //Paragraph p2 = new Paragraph(new Phrase("This is my second paragraph.", FontFactory.GetFont(FontFactory.HELVETICA, 12)));
            //Paragraph p3 = new Paragraph("This is my third paragraph.", FontFactory.GetFont(FontFactory.HELVETICA, 12));
            //// 段落中可以添加对象
            //p1.Add("you can add strings, "); 
            //p1.Add(new Chunk("you can add chunks ")); 
            //p1.Add(new Phrase("or you can add phrases."));
            //docPDF.Add(p1);
            //docPDF.Add(p2);
            //docPDF.Add(p3);
            #endregion
            #region 锚点
            //// 锚点（链接）
            #endregion
            #region 列表
            //// 列表
            //List list = new List(true, 20);
            //list.Add(new ListItem("First line"));
            //list.Add(new ListItem("The second line is longer to see what happens once the end of the line is reached. Will it start on a new line?"));
            //list.Add(new ListItem("Third line"));
            //docPDF.Add(list);
            #endregion
            #region 表格
            // 1. 表格，默认从第一行开始添加，满了再下一行
            //Table aTable = new Table(2, 2);
            //aTable.AddCell("0.0");
            //aTable.AddCell("0.1");
            //aTable.AddCell("1.0");
            //aTable.AddCell("1.1");
            // 设置自动填充空格
            //Table aTable = new Table(4, 4);
            //aTable.AutoFillEmptyCells = true;
            //aTable.AddCell("2.2", new System.Drawing.Point(2, 2));
            //aTable.AddCell("3.3", new System.Drawing.Point(3, 3));
            //aTable.AddCell("2.1", new System.Drawing.Point(2, 1));
            //aTable.AddCell("1.3", new System.Drawing.Point(1, 3));

            // 2. 动态生成表格
            //Table table1 = new Table(3);
            //table1.Width = 100; // 页面比例
            //table1.BorderWidth = 0;
            ////table1.Cellpadding = 3;
            //table1.Cellspacing = 2;

            //Cell cell = new Cell("header");
            ////cell.BorderWidth = 1;
            //cell.BorderWidthBottom = 1;
            //cell.BorderWidthTop = 1;
            //cell.Header = true;
            //cell.Colspan = 3;
            //cell.BackgroundColor = new BaseColor(235, 235, 235);
            //cell.HorizontalAlignment = Element.ALIGN_CENTER;
            //cell.VerticalAlignment = Element.ALIGN_MIDDLE;  // 没用
            //table1.AddCell(cell);
            //cell = new Cell("example cell with colspan 1 and rowspan 2zhong中文");
            //cell.Rowspan = 2;
            //cell.BorderWidth = 0;
            //table1.AddCell(cell);
            //table1.AddCell("1.1");
            //table1.AddCell("2.1");
            //table1.AddCell("1.2");
            //table1.AddCell("2.2");
            //table1.AddCell("cell test1");
            //cell = new Cell("big cell");
            //cell.BorderWidth = 0;
            //cell.BorderWidthBottom = 1;
            //cell.Rowspan = 2;
            //cell.Colspan = 2;
            //cell.BackgroundColor = new BaseColor(0xC0, 0xC0, 0xC0);
            //table1.AddCell(cell);
            //table1.AddCell("cell test2");
            //// 设置单元格对齐方式
            ////cell.HorizontalAlignment = Element.ALIGN_CENTER;
            ////cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            //docPDF.Add(table1);
            // 3. 复杂表格, 可以分段（行）显示表
            // 参数有三种：float[] relativeWidths；int numColumns; PdfPTable table
            PdfPTable table2 = new PdfPTable(2);
            table2.WidthPercentage = 100;
            // 外边距
            table2.SpacingAfter = 5;
            table2.SpacingBefore = 5;
            // 左边格子显示
            PdfPCell cell = new PdfPCell(new Paragraph("header", new Font(bsFont, 10)));
            cell.MinimumHeight = 5f;
            //cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            //cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Border = 0;
            PdfPTable tableChild = new PdfPTable(3);
            #region 子表格
            for (int i = 0; i < 5; i++)
            {
                tableChild.AddCell(new PdfPCell());
                PdfPCell pc = new PdfPCell(new Paragraph("名m", fontValue));
                pc.Padding = 5;
                tableChild.AddCell(pc);
                pc = new PdfPCell(new Paragraph("字z", fontValue));
                pc.Padding = 5;
                tableChild.AddCell(pc);
            }

            #endregion
            PdfPCell cell2 = new PdfPCell(tableChild);
            cell2.MinimumHeight = 5f;
            cell2.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell2.HorizontalAlignment = Element.ALIGN_CENTER;
            cell2.Border = 0;
            table2.AddCell(cell);
            table2.AddCell(cell2);
            docPDF.Add(table2);
            #endregion
            Paragraph p = new Paragraph("INFORMATION INVOICE");
            p.Alignment = Element.ALIGN_MIDDLE;
            p.Alignment = Element.ALIGN_CENTER;
            docPDF.Add(p);
            #region 表格2
            float[] left = { 25, 150, 25, 25 };
            PdfPTable table3 = new PdfPTable(left);
            table3.WidthPercentage = 100;
            // 外边距
            table3.SpacingAfter = 5;
            table3.SpacingBefore = 5;
            string[] titles = { "Date", "Description", "Debit", "Credit" };
            // 表头
            for (int k = 0; k < titles.Length; k++)
            {
                PdfPCell cell3_1 = new PdfPCell(new Paragraph(titles[k], new Font(bsFont, 10)));
                cell3_1.Border = 0;
                cell3_1.PaddingBottom = 5;
                cell3_1.BorderWidthBottom = 1;
                cell3_1.BorderWidthTop = 1;
                cell3_1.BackgroundColor = new BaseColor(235, 235, 235);
                cell3_1.MinimumHeight = 5f;
                table3.AddCell(cell3_1);
            }
            List<string[]> contents = new List<string[]>();
            contents.Add(new string[] { "22-07-20", "Hostel Fee", "300.00", "", "Chan Mei Lai, 22 - 23 Jul; 1 night; Double room; MOP300 per room per night (refer proposal: XXX/XXX/XXXX)" });
            contents.Add(new string[] { "22-07-20", "Visa Card", "", "300.00", "XXXXXXXXXXXX1111 XX/XX" });
            for (int j = 0; j < contents.Count(); j++)
            {
                for (int i = 0; i < contents[j].Length; i++)
                {
                    PdfPCell cell3_1 = new PdfPCell();
                    cell3_1.Padding = 2;
                    cell3_1.PaddingBottom = 5;
                    cell3_1.Border = 0;
                    cell3_1.MinimumHeight = 5f;
                    if (i == contents[j].Length - 1)
                    {
                        if (j == contents.Count() - 1)
                        {
                            cell3_1.BorderWidthBottom = 1;
                        }
                        table3.AddCell(cell3_1);
                        cell3_1.Colspan = 3;
                        cell3_1.PaddingLeft = 20;
                    }
                    cell3_1.AddElement(new Paragraph(contents[j][i], new Font(bsFont, 10)));
                    table3.AddCell(cell3_1);
                }
            }
            docPDF.Add(table3);
            #endregion
            #region 表格3，绝对位置
            PdfPTable tableSign = new PdfPTable(2);
            // 绝对位置,不能设置为宽度比例要直接确定的值
            tableSign.TotalWidth = 600F;
            //tableSign.WidthPercentage = 100; // 失败
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
            string[] tsTitle2 = { "300.00", "0.00" };
            for (int i = 0; i < tsTitle.Length; i++)
            {
                PdfPCell cellS21 = new PdfPCell(new Phrase(tsTitle[i], fontText));
                cellS21.PaddingBottom = 5;
                cellS21.PaddingTop = 10;
                cellS21.Border = 0;
                tSignChild.AddCell(cellS21);
                PdfPCell cellS22 = new PdfPCell(new Phrase(tsTitle2[i], fontText));
                cellS22.HorizontalAlignment = Element.ALIGN_CENTER;
                cellS22.VerticalAlignment = Element.ALIGN_MIDDLE;
                cellS22.Border = 0;
                cellS22.PaddingBottom = 5;
                cellS22.PaddingTop = 10;
                cellS22.BorderWidthBottom = 1;
                tSignChild.AddCell(cellS22);
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
            PdfPCell cellS4 = new PdfPCell(new Phrase("Guest Signature", fontText));
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
            #region 翻页，需要用到template
            //PdfContentByte content = write.DirectContent;
            //PdfTemplate template = content.CreateTemplate(1, 1);
            //template.SaveState();
            //template.SetColorFill(BaseColor.Red);
            //template.Rectangle(0, 0, 1, 1);
            //template.FillStroke();
            //template.RestoreState();
            //content.AddTemplate(template, 1, 0, 0, -1, 1, 1);
            //docPDF.NewPage();
            //docPDF.Add(new Paragraph("hello"));
            #endregion
            #region 页脚
            //直接使用显示不出来
            //HeaderFooter footer = new HeaderFooter(new Phrase("This is page: "), true);
            //footer.Border = Rectangle.NO_BORDER;
            //docPDF.Footer = footer;
            // 自设计页脚
            //public float writeSelectedRows(int rowStart, int rowEnd, float xPos, float yPos, PdfContentByte canvas);
            //参数rowStart是你想开始的行的数目，参数rowEnd是你想显示的最后的行（如果你想显示所有的行，用 - 1），xPos和yPos是表格的坐标，canvas是一个PdfContentByte对象。在示例代码1009中，我们添加了一个表在(100, 600)处：
            PdfPTable tabFot = new PdfPTable(new float[] { 1F });
            tabFot.TotalWidth = 500F;
            PdfPCell cellf = new PdfPCell(new Phrase("Colina de Mong-Ha, Macau, China Tel: (853) 2851 5222 Fax: (853) 2855 6925 www.ift.edu.mo/pousada", new Font(bsFont, 10)));
            cellf.Border = 0;
            tabFot.AddCell(cellf);
            tabFot.WriteSelectedRows(0, -1, 66, 25, write.DirectContent);

            #endregion
            #region 页眉
            // 显示不了
            //HeaderFooter header = new HeaderFooter(new Phrase("This is a header without a page number"), false);
            //docPDF.Header = header;
            // 自设计页眉，参考页脚
            #endregion

            docPDF.Close();//关闭
            write.Close();
            wfs.Close();
            #endregion
        }
        return new FileContentResult(File.ReadAllBytes(filePath), "application/pdf");
        //byte[] readBytes = new BinaryReader(new FileStream("456.dat", FileMode.Open)).ReadBytes(-1);
        //return new FileContentResult(readBytes, "application/pdf");
    }

    //private static async Task<Dictionary<string, string>> FillKeys(AssetInfoModel model, CurrentUserInfo currentUser)
    //{
    //    Dictionary<string, string> para = new Dictionary<string, string>();
    //    para.Add("asset_code", model.AssetInfo.AssetCode);
    //    para.Add("asset_name", model.AssetInfo.AssetName);
    //    //资产分类
    //    var category = await new AssetManagement().GetAssetCateGory(currentUser.SuperId, model.AssetInfo.ACCode);
    //    if (category != null)
    //    {
    //        para.Add("asset_category", category.AC_NAME);
    //    }
    //    else
    //    {
    //        para.Add("asset_category", "");
    //    }
    //    para.Add("asset_type", model.AssetInfo.AssetType);
    //    para.Add("asset_sn", model.AssetInfo.AssetSN);
    //    para.Add("asset_unit", model.AssetInfo.ACUnit);
    //    para.Add("asset_price", model.AssetInfo.AssetPrice);
    //    //资产使用公司
    //    var currentUsingOrganization = await new OrganizationManagement().GetBySmidAndCode(currentUser.SuperId, model.AssetCurrent.CurrentOrgCode);
    //    if (currentUsingOrganization != null)
    //    {
    //        para.Add("asset_using_org", currentUsingOrganization.ORGANIZATION_NAME);
    //    }
    //    //资产使用部门
    //    var currentDepartment = await new OrganizationManagement().GetDepartment(currentUser.SuperId, model.AssetCurrent.CurrentDepCode, currentUsingOrganization?.ID);
    //    if (currentDepartment != null)
    //    {
    //        para.Add("asset_using_depart", currentDepartment.DEPART_NAME);
    //    }
    //    //资产使用人（员工）
    //    if (currentUsingOrganization != null && currentDepartment != null && model != null && model.AssetCurrent != null && !string.IsNullOrEmpty(model.AssetCurrent.CurrentUserCode))
    //    {
    //        var currentStaff = await new OrganizationManagement().GetStaff(currentUsingOrganization?.ID, currentDepartment?.ID, model.AssetCurrent.CurrentUserCode);
    //        if (currentStaff != null)
    //        {
    //            para.Add("asset_user", currentStaff.STAFF_NAME);
    //        }
    //    }
    //    para.Add("asset_purchase_date", model.AssetInfo.PurDate);
    //    //资产管理员（当前使用公司的管理员）
    //    if (currentUsingOrganization != null && model != null && model.AssetCurrent != null && !string.IsNullOrEmpty(model.AssetCurrent.CurrentManager))
    //    {
    //        var currentManager = await new OrganizationManagement().GetStaff(currentUsingOrganization.ID, model.AssetCurrent.CurrentManager);
    //        if (currentManager != null)
    //        {
    //            para.Add("asset_manager", currentManager.STAFF_NAME);
    //        }
    //    }
    //    //资产所属公司
    //    var belongOrganization = await new OrganizationManagement().GetBySmidAndCode(currentUser.SuperId, model.AssetInfo.OrgCode);
    //    if (belongOrganization != null)
    //    {
    //        para.Add("asset_belong_org", belongOrganization.ORGANIZATION_NAME);
    //    }
    //    //区位
    //    var area = await new AssetAreaManagement().GetArea(currentUser.SuperId, model.AssetCurrent.AreaCode);
    //    if (area != null)
    //    {
    //        para.Add("asset_area", model.AssetCurrent.AreaCode);
    //    }
    //    para.Add("asset_address", model.AssetCurrent.Location);
    //    //使用期限（月），需要将资产的年期限进行转换
    //    if (!string.IsNullOrEmpty(model.AssetInfo.ACLife))
    //    {
    //        if (int.TryParse(model.AssetInfo.ACLife, out int year))
    //        {
    //            para.Add("asset_life", (year * 12).ToString());
    //        }
    //    }
    //    para.Add("asset_support", model.AssetMain.Supplier);
    //    para.Add("asset_from", model.AssetInfo.AssetFrom);
    //    para.Add("asset_remark", model.AssetInfo.AssetDes);
    //    //资产变更信息
    //    var assetInfo = await new AssetsOperationBLL().GetAssetInfo(currentUser, model.AssetInfo.AssetCode);
    //    if (assetInfo == null)
    //    {
    //        throw new Exception(await Localizer.GetValueAsync("NoExistAssetInformation"));
    //    }
    //    var modification = await new AssetManagement().GetModifyRecord(currentUser.SuperId, assetInfo.ID);
    //    if (modification != null)
    //    {
    //        for (int i = 0; i < modification.Count(); i++)
    //        {
    //            para.Add(string.Format("asset_modify_date{0}", i), modification.ToList()[i].OrderDate);
    //            para.Add(string.Format("asset_modify_charge{0}", i), modification.ToList()[i].Audit);
    //            para.Add(string.Format("asset_modify_type{0}", i), modification.ToList()[i].Type);
    //            para.Add(string.Format("asset_modify_remark{0}", i), modification.ToList()[i].Remark);
    //        }
    //    }

    //    return para;
    //}
}
}
