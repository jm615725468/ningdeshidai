using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Printing;

namespace Zprinter
{
    public class Printer
    {
        private Font printFont;
        private Font titleFont;
        private StringReader streamToPrint;
        private int leftMargin = 0;

        /// <summary>
        /// 设置PrintDocument 的相关属性
        /// </summary>
        /// <param name="str">要打印的字符串</param>
        public void print(string str)
        {
            try
            {
                streamToPrint = new StringReader(str);
                printFont = new Font("宋体", 10);
                titleFont = new Font("宋体", 15);
                System.Drawing.Printing.PrintDocument pd = new System.Drawing.Printing.PrintDocument();
                pd.PrinterSettings.PrinterName = "Microsoft Print to PDF";//打印机的名字
                pd.DocumentName = pd.PrinterSettings.MaximumCopies.ToString();
                pd.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.pd_PrintPage);

                pd.PrintController = new System.Drawing.Printing.StandardPrintController();
                pd.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 打印格式（字符串）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ev"></param>

        private void pd_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = this.leftMargin;
            float topMargin = 0;
            String line = null;
            linesPerPage = ev.MarginBounds.Height / printFont.GetHeight(ev.Graphics);
            while (count < linesPerPage &&
            ((line = streamToPrint.ReadLine()) != null))
            {
                if (count == 0)
                {
                    yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
                    ev.Graphics.DrawString(line, titleFont, Brushes.Black, leftMargin + 10, yPos, new StringFormat());
                }
                else
                {
                    yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
                    ev.Graphics.DrawString(line, printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
                }
                count++;
            }
            if (line != null)
                ev.HasMorePages = true;
            else
                ev.HasMorePages = false;

        }
        private void GetPrintName()
        {
            //获取本地打印机的名字
            PrintDocument print = new PrintDocument();
            string sDefault = print.PrinterSettings.PrinterName;//默认打印机名

            //label1.Text = sDefault;

            foreach (string sPrint in PrinterSettings.InstalledPrinters)//获取所有打印机名称
            {
                //listBox1.Items.Add(sPrint);
                //textBox1.Text += sPrint + "\n";
                //if (sPrint == sDefault)
                    //listBox1.SelectedIndex = listBox1.Items.IndexOf(sPrint);

            }
        }
        /// <summary>
                /// 设置PrintDocument 的相关属性
                /// </summary>
                /// <param name="str">要打印的字符串</param>
        public void print2()
        {
            try
            {
                // streamToPrint = new StringReader(str);
                printFont = new Font("宋体", 10);
                titleFont = new Font("宋体", 15);
                System.Drawing.Printing.PrintDocument pd = new System.Drawing.Printing.PrintDocument();
                pd.PrinterSettings.PrinterName = "Hewlett-Packard HP LaserJet Pro MFP M126a";
                pd.DocumentName = pd.PrinterSettings.MaximumCopies.ToString();
                //pd.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.pd_PrintPage);
                pd.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.MyPrintDocument_PrintPage);
                pd.PrintController = new System.Drawing.Printing.StandardPrintController();
                pd.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        /// <summary>
        /// 打印的格式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyPrintDocument_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            /*如果需要改变自己 可以在new Font(new FontFamily("黑体"),11）中的“黑体”改成自己要的字体就行了，黑体 后面的数字代表字体的大小
             System.Drawing.Brushes.Blue , 170, 10 中的 System.Drawing.Brushes.Blue 为颜色，后面的为输出的位置 */
            e.Graphics.DrawString("新乡市三月软件公司入库单", new Font(new FontFamily("黑体"), 11), System.Drawing.Brushes.Black, 170, 10);
            e.Graphics.DrawString("供货商:河南科技学院", new Font(new FontFamily("黑体"), 8), System.Drawing.Brushes.Blue, 10, 12);
            //信息的名称
            e.Graphics.DrawLine(Pens.Black, 8, 30, 480, 30);
            e.Graphics.DrawString("入库单编号", new Font(new FontFamily("黑体"), 8), System.Drawing.Brushes.Black, 9, 35);
            e.Graphics.DrawString("商品名称", new Font(new FontFamily("黑体"), 8), System.Drawing.Brushes.Black, 160, 35);
            e.Graphics.DrawString("数量", new Font(new FontFamily("黑体"), 8), System.Drawing.Brushes.Black, 260, 35);
            e.Graphics.DrawString("单价", new Font(new FontFamily("黑体"), 8), System.Drawing.Brushes.Black, 330, 35);
            e.Graphics.DrawString("总金额", new Font(new FontFamily("黑体"), 8), System.Drawing.Brushes.Black, 400, 35);
            e.Graphics.DrawLine(Pens.Black, 8, 50, 480, 50);
            //产品信息
            e.Graphics.DrawString("R2011-01-2016:06:35", new Font(new FontFamily("黑体"), 8), System.Drawing.Brushes.Black, 9, 55);
            e.Graphics.DrawString("联想A460", new Font(new FontFamily("黑体"), 8), System.Drawing.Brushes.Black, 160, 55);
            e.Graphics.DrawString("100", new Font(new FontFamily("黑体"), 8), System.Drawing.Brushes.Black, 260, 55);
            e.Graphics.DrawString("200.00", new Font(new FontFamily("黑体"), 8), System.Drawing.Brushes.Black, 330, 55);
            e.Graphics.DrawString("20000.00", new Font(new FontFamily("黑体"), 8), System.Drawing.Brushes.Black, 400, 55);


            e.Graphics.DrawLine(Pens.Black, 8, 200, 480, 200);
            e.Graphics.DrawString("地址：新乡市河南科技学院信息工程学院", new Font(new FontFamily("黑体"), 8), System.Drawing.Brushes.Black, 9, 210);
            e.Graphics.DrawString("经办人:任忌", new Font(new FontFamily("黑体"), 8), System.Drawing.Brushes.Black, 220, 210);
            e.Graphics.DrawString("服务热线:15083128577", new Font(new FontFamily("黑体"), 8), System.Drawing.Brushes.Black, 320, 210);
            e.Graphics.DrawString("入库时间:" + DateTime.Now.ToString(), new Font(new FontFamily("黑体"), 8), System.Drawing.Brushes.Black, 9, 230);
        }
        /// <summary>
                /// 设置PrintDocument 的相关属性
                /// </summary>
                /// <param name="str"></param>
        public void print3()
        {
            try
            {
                /*
                // streamToPrint = new StringReader(str);
                printFont = new Font("宋体", 10);
                titleFont = new Font("宋体", 15);
                System.Drawing.Printing.PrintDocument pd = new System.Drawing.Printing.PrintDocument();
                pd.PrinterSettings.PrinterName = "Hewlett-Packard HP LaserJet Pro MFP M126a";
                pd.DocumentName = pd.PrinterSettings.MaximumCopies.ToString();
                //pd.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.pd_PrintPage);
                pd.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.Menu);
                pd.PrintController = new System.Drawing.Printing.StandardPrintController();
                pd.Print();
                 
                 */



                //新建打印对象
                System.Drawing.Printing.PrintDocument pd = new System.Drawing.Printing.PrintDocument();
                //打印机名字
                pd.PrinterSettings.PrinterName = "Hewlett-Packard HP LaserJet Pro MFP M126a";
                //打印文档显示的名字
                pd.DocumentName = "订单";
                //
                //pd.PrinterSettings.MaximumCopies.ToString();

                //打印的格式
                pd.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.Menu);

                //
                pd.PrintController = new System.Drawing.Printing.StandardPrintController();

                //开始打印
                pd.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //绘图需要使用的数组 后期可以套用变量list
        public string[] menu = { "菜一", "菜二", "中级菜单" };

        /// <summary>
        /// 打印的格式:菜单
        /// </summary>
        /// <param name="sender">自定义报表（原理是绘图）</param>
        /// <param name="e"></param>
        private void Menu(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {

            //绘制（输出，文字格式（字体，大小），颜色，位置起始位置x，y轴坐标）                 
            e.Graphics.DrawString("店名", new Font(new FontFamily("黑体"), 11), System.Drawing.Brushes.Black, 138, 10);

            //打印两个点的坐标（颜色，坐标1，坐标2）
            e.Graphics.DrawLine(Pens.Black, 8, 30, 292, 30);


            e.Graphics.DrawString("订单号：（）", new Font(new FontFamily("黑体"), 11), System.Drawing.Brushes.Black, 10, 35);

            e.Graphics.DrawString("编号", new Font(new FontFamily("黑体"), 11), System.Drawing.Brushes.Black, 10, 55);
            e.Graphics.DrawString("菜名", new Font(new FontFamily("黑体"), 11), System.Drawing.Brushes.Black, 75, 55);
            e.Graphics.DrawString("单价", new Font(new FontFamily("黑体"), 11), System.Drawing.Brushes.Black, 150, 55);
            e.Graphics.DrawString("数量", new Font(new FontFamily("黑体"), 11), System.Drawing.Brushes.Black, 200, 55);

            int i = 0;
            //循环输出变量
            foreach (string element in menu)
            {

                i = i + 20;
                e.Graphics.DrawString(element, new Font(new FontFamily("黑体"), 11), System.Drawing.Brushes.Black, 75, 80 + i);

            }


            e.Graphics.DrawLine(Pens.Black, 8, 200, 292, 200);

        }
        public void print4()
        {
            try
            {

                //新建打印对象
                System.Drawing.Printing.PrintDocument pd = new System.Drawing.Printing.PrintDocument();
                //打印机名字
                pd.PrinterSettings.PrinterName = "Hewlett-Packard HP LaserJet Pro MFP M126a";
                //打印文档显示的名字
                pd.DocumentName = "订单";
                //
                //pd.PrinterSettings.MaximumCopies.ToString();

                //打印的格式
                pd.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.Image);

                //
                pd.PrintController = new System.Drawing.Printing.StandardPrintController();

                //开始打印
                pd.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        /// <summary>
        /// 打印的格式:图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            //直接调用图片对象绘制
            //e.Graphics.DrawImage(pictureBox1.Image, 20, 20);

        }

        private void button6_Click(object sender, EventArgs e)
        {
            //printDocument1.PrintController.OnEndPrint(printDocument1, new System.Drawing.Printing.PrintEventArgs());
        }

    }

}

