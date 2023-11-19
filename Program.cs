using System;
using System.IO;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Collections;
using PdfiumViewer;
using System.Net;

namespace WindowsFormsApp2
{
    public class Program
    {
        static Font printFont;
        static StreamReader streamToPrint;
        static int printNum = 0;//多页打印
        static string imageFile = "";//单个图片文件
        static ArrayList fileList = new ArrayList();//多个图片文件
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            
            //string[] argasd = { "https://storage-1252908908.cos.ap-shanghai.myqcloud.com/%25E5%25A2%25A8%25E5%25B8%259D-1645777304.pdf", "A4 210 x 297 毫米", "1", "1", "1", "pdf", "2", "%25E5%25A2%25A8%25E5%25B8%259D-1645777304.pdf" };
            //string[] argasd = { "D:\\scan\\0.txt,D:\\scan\\1.txt","8K 270 x 390 毫米","1", "1", "3", "image" };
            // 在整个程序最开始的地方进行引用DLL的内存载入
            if (UDFResource("WindowsFormsApp2.x86.pdfium.dll"))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                // Application.Run();
                if (args[5] == "image")
                {
                    printImage(args);
                }
                else if (args[5] == "pdf")
                {
                    printPdf(args);
                }
            }
            else
            {
                return;
            }
            
        }

        // <summary>
        /// 函数功能：将嵌入项目的文件加载到内存中。
        /// <para>respath=嵌入项目的文件路径=[解决方案名称].[文件夹路径].[文件名]；</para>
        /// </summary>
        /// <param name="respath">嵌入项目的文件路径=[解决方案名称].[文件夹路径].[文件名]</param>
        /// <returns>文件加载成功返回True</returns>
        public static bool UDFResource(string respath)
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += (s, a) =>
                {
                    using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(respath))
                    {
                        byte[] data = new byte[stream.Length];
                        stream.Read(data, 0, data.Length);
                        return System.Reflection.Assembly.Load(data);
                    }
                };
                return true;
            }

            catch
            {
                return false;
            }
        }

        static void printImage(string[] args)
        {
            //fileList.Add("D:\\scan\\0.txt");
            //fileList.Add("D:\\scan\\blank.txt");
            //打印内容获取
            fileList = new ArrayList(args[0].Split(','));
            PrintDocument docToPrint = new PrintDocument();
            docToPrint.PrinterSettings.PrinterName = "EPSON AM-C4000 Series";
            docToPrint.DocumentName = args[7];
            //打印纸设置
            foreach (PaperSize ps in docToPrint.PrinterSettings.PaperSizes)
            {
                if (ps.PaperName == args[1])
                {
                    docToPrint.PrinterSettings.DefaultPageSettings.PaperSize = ps;
                    docToPrint.DefaultPageSettings.PaperSize = ps;
                }
            }
            //docToPrint.DefaultPageSettings.Landscape = true;
            //docToPrint.PrinterSettings.Duplex = Duplex.Horizontal;
            //打印方向设置，1：横向，2：竖向
            if (args[2] == "1")
            {
                docToPrint.DefaultPageSettings.Landscape = true;
            }
            else
            {
                docToPrint.DefaultPageSettings.Landscape = false;
            }
            //打印颜色设置，1：彩色，2：黑白
            if (args[3] == "1")
            {
                docToPrint.DefaultPageSettings.Color = true;
            }
            else
            {
                docToPrint.DefaultPageSettings.Color = false;
            }
            //单双面设置，1：单面，2：长边，3：短边
            if (args[4] == "1")
            {
                docToPrint.PrinterSettings.Duplex = Duplex.Simplex;
            }
            else if (args[4] == "2")
            {
                docToPrint.PrinterSettings.Duplex = Duplex.Vertical;
            }
            else if (args[4] == "3")
            {
                docToPrint.PrinterSettings.Duplex = Duplex.Horizontal;
            }
            Margins margins = new Margins(0, 0, 0, 0);
            docToPrint.DefaultPageSettings.Margins = margins;
            docToPrint.PrinterSettings.DefaultPageSettings.Margins = margins;

            docToPrint.BeginPrint += new PrintEventHandler(docToPrint_BeginPrint);
            docToPrint.EndPrint += new PrintEventHandler(docToPrint_EndPrint);
            docToPrint.PrintPage += new PrintPageEventHandler(docToPrint_PrintPage);
            docToPrint.PrintController = new StandardPrintController();
            docToPrint.Print();
            ///弹出打印设置界面-调试用
           /* Console.WriteLine(docToPrint);
            var setupA = new PageSetupDialog();
            setupA.Document = docToPrint;
            if (DialogResult.OK == setupA.ShowDialog())
            {
                docToPrint.Print();
            }*/
        }


        // The PrintPage event is raised for each page to be printed.
        static void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;
            string line = null;

            // Calculate the number of lines per page.
            linesPerPage = ev.MarginBounds.Height /
               printFont.GetHeight(ev.Graphics);

            // Print each line of the file.
            while (count < linesPerPage &&
               ((line = streamToPrint.ReadLine()) != null))
            {
                yPos = topMargin + (count *
                   printFont.GetHeight(ev.Graphics));
                ev.Graphics.DrawString(line, printFont, Brushes.Black,
                   leftMargin, yPos, new StringFormat());
                count++;
            }

            // If more lines exist, print another page.
            if (line != null)
                ev.HasMorePages = true;
            else
                ev.HasMorePages = false;
        }

        static void docToPrint_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            /*if (fileList.Count == 0)
                fileList.Add(imageFile);*/
        }
        static void docToPrint_EndPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {

        }
        static void docToPrint_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            //图片抗锯齿
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            StreamReader readstring = new StreamReader(fileList[printNum].ToString().Trim(), System.Text.Encoding.Default);
            string myfile;
            myfile = readstring.ReadToEnd();//此句读取到尾时，已把光标指针移动到文件结尾
            readstring.Close();
            System.Drawing.Image image = Base64ToImg(myfile);
            System.Drawing.Rectangle destRect2 = new System.Drawing.Rectangle(0, 0, e.MarginBounds.Width-11, e.MarginBounds.Height-11);
            e.Graphics.DrawImage(image, destRect2, 11, 11, image.Width, image.Height, System.Drawing.GraphicsUnit.Pixel);
            
            if (printNum < fileList.Count - 1)
            {
                printNum++;
                e.HasMorePages = true;//HasMorePages为true则再次运行PrintPage事件
                return;
            }
            e.HasMorePages = false;
        }

        //解析base64编码获取图片
        static Image Base64ToImg(string base64)
        {
            base64 = base64.Replace("data:image/png;base64,", "").Replace("data:image/jgp;base64,", "").Replace("data:image/jpg;base64,", "").Replace("data:image/jpeg;base64,", "");//将base64头部信息替换
            byte[] bytes = Convert.FromBase64String(base64);
            MemoryStream memStream = new MemoryStream(bytes);
            return Image.FromStream(memStream);
        }

        //下载网络pdf到本地
        public static void DownloadPdf(string pdfUrlPath)
        {
            WebClient wc = new WebClient();
            byte[] buffer = wc.DownloadData(pdfUrlPath);
            FileStream fileStream = new FileStream("D:\\scan\\panpan.pdf", FileMode.Create);
            fileStream.Write(buffer, 0, buffer.Length);
            fileStream.Close();
        }

        //打印本地pdf
        static void printPdf(string[] args)
        {
            DownloadPdf(args[0]);
            int num =int.Parse(args[6]);
            for (int i = 0; i < num; i++)
            {
                using (var document = PdfDocument.Load("D:\\scan\\panpan.pdf"))
            {
               
                    using (var printDocument = document.CreatePrintDocument())
                    {
                        printDocument.PrinterSettings.PrintFileName = "panpan.pdf";
                        printDocument.PrinterSettings.PrinterName = "EPSON AM-C4000 Series";
                        printDocument.DocumentName = args[7];
                        //打印纸设置
                        foreach (PaperSize ps in printDocument.PrinterSettings.PaperSizes)
                        {
                            if (ps.PaperName == args[1])
                            {
                                printDocument.PrinterSettings.DefaultPageSettings.PaperSize = ps;
                                printDocument.DefaultPageSettings.PaperSize = ps;
                            }
                        }
                        if (args[2] == "1")
                        {
                            printDocument.DefaultPageSettings.Landscape = true;
                        }
                        else
                        {
                            printDocument.DefaultPageSettings.Landscape = false;
                        }
                        //打印颜色设置，1：彩色，2：黑白
                        if (args[3] == "1")
                        {
                            printDocument.DefaultPageSettings.Color = true;
                        }
                        else
                        {
                            printDocument.DefaultPageSettings.Color = false;
                        }
                        //单双面设置，1：单面，2：长边，3：短边
                        if (args[4] == "1")
                        {
                            printDocument.PrinterSettings.Duplex = Duplex.Simplex;
                        }
                        else if (args[4] == "2")
                        {
                            printDocument.PrinterSettings.Duplex = Duplex.Vertical;
                        }
                        else if (args[4] == "3")
                        {
                            printDocument.PrinterSettings.Duplex = Duplex.Horizontal;
                        }
                        Margins margins = new Margins(0, 0, 0, 0);
                        printDocument.DefaultPageSettings.Margins = margins;
                        printDocument.PrinterSettings.DefaultPageSettings.Margins = margins;
                        printDocument.PrinterSettings.PrintFileName = "file.pdf";
                        printDocument.PrintController = new StandardPrintController();
                        printDocument.Print();
                    }
                }
            }
          
        }
    }
}
