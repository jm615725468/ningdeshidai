using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Data.SqlClient;

using System.Drawing;
using System.Configuration;
using System.Xml;

using System.Net;

using System.Drawing.Printing;
using System.Management;



namespace HNSys
{
    class CommonMean
    {
        #region txt文本加到TextBox
        public void txtLoad(string patch, TextBox textBox)
        {
            string StringTemp = null;
            if (System.IO.File.Exists(patch))
            {
                // 显示日志信息
                using (System.IO.StreamReader sr = new System.IO.StreamReader(Application.StartupPath + "\\Logs_20210522.txt", Encoding.UTF8))
                {
                    StringTemp = sr.ReadToEnd();//获取txt文件的字符串
                }

                string[] sArray = StringTemp.Split(new char[2] { '\r', '\n' });//根据回车换行将字符串分为字符串数组
                sArray = sArray.Where(s => !string.IsNullOrEmpty(s)).ToArray();
                textBox.Text = sArray[sArray.Length - 1];
                for (int i = sArray.Length - 2; i >= 0; i--)
                {
                    textBox.Text = textBox.Text + "\r\n" + sArray[i];
                }

            }

        }
        #endregion

        #region ListboxOperate

        public void ListboxOperate(int Operate, string StrigInsert, ListBox listBox)
        {
            switch (Operate)
            {
                case 1:
                    listBox.Invoke(new MethodInvoker(delegate ()
                    {
                        listBox.Items.Insert(0, StrigInsert);
                    }));
                    break;

                case 2:
                    listBox.Invoke(new MethodInvoker(delegate ()
                    {
                        listBox.Items.Clear();
                    }));
                    break;
                default: break;

            }
        }
        #endregion

        #region DataGridViewToCSV

        //第一列ID不导出
        public void DataGridViewToCSV(string FilePath, DataGridView dataGridView1)
        {
            // string FilePath = Application.StartupPath + "\\Data\\" + DateTime.Now.ToString("yyyyMM") + "\\" + Tags.DangQianMoJuanHao + "_" + Tags.DangQanXingHao + ".csv";
            StreamWriter sw = new StreamWriter(FilePath, true, System.Text.Encoding.GetEncoding(-0));

            string strLine = "";
            try
            {
                //表头
                for (int i = 1; i < dataGridView1.ColumnCount; i++)
                {
                    if (i > 1)
                        strLine += ",";
                    strLine += dataGridView1.Columns[i].HeaderText;
                }
                strLine.Remove(strLine.Length - 1);
                sw.WriteLine(strLine);
                strLine = "";
                //表的内容
                for (int j = 0; j < dataGridView1.Rows.Count; j++)
                {
                    strLine = "";
                    int colCount = dataGridView1.Columns.Count;
                    for (int k = 1; k < colCount; k++)
                    {
                        if (k > 1 && k < colCount)
                            strLine += ",";
                        if (dataGridView1.Rows[j].Cells[k].Value == null)
                            strLine += "";
                        else
                        {
                            string cell = dataGridView1.Rows[j].Cells[k].Value.ToString().Trim();
                            //防止里面含有特殊符号
                            cell = cell.Replace("\"", "\"\"");
                            cell = "\"" + cell + "\"";
                            strLine += cell;
                        }
                    }
                    sw.WriteLine(strLine);
                }
                sw.Close();
            }
            catch (Exception ex)
            {

            }

        }


        public void DataGridViewToCSV1(string FilePath)
        {
            StreamWriter sw = new StreamWriter(FilePath, true, System.Text.Encoding.GetEncoding(-0));

            string strLine = "";
            try
            {
                //表头
              // strLine += ",";
                strLine += "采集时间";
                strLine += ",";
                strLine += "员工账号";
                strLine += ",";
                strLine += "设备编号";
                for (int i = 0; i < 310; i++)
                {
                    strLine += ",";
                    strLine += CommonTags.DeviceParameterName[i];          
                }

              
                strLine.Remove(strLine.Length - 1);
                sw.WriteLine(strLine);
                strLine = "";
                //表的内容
                StringBuilder sb = new StringBuilder();
                sb.Append("select CellectTime,Operator,DeviceCode");


                for (int i = 0; i < 312; i++)
                {
                    sb.Append(",Parameter");
                    sb.Append((i + 1).ToString());
                }

                sb.Append(" from table1 where CellectTime > " + "'" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd HH:mm:ss") + "'" + "and CellectTime <" + "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" + "order by CellectTime desc");

                DataTable dt = SQLite.ParameterGetDataSet(sb.ToString()).Tables[0];

                if (dt.Rows.Count > 0)
                {
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        strLine = "";
                        int colCount = dt.Columns.Count;
                        for (int k = 0; k < colCount; k++)
                        {
                            if (k > 0 && k < colCount)
                                strLine += ",";

                            if (dt.Rows[j][k] == null)
                                strLine += "";
                            else
                            {
                                string cell = dt.Rows[j][k].ToString();
                                //防止里面含有特殊符号
                                cell = cell.Replace("\"", "\"\"");
                                cell = "\"" + cell + "\"";
                                strLine += cell;
                            }
                         

                        }
                        sw.WriteLine(strLine);
                    }
                    sw.Close();
                }
            }
            catch (Exception ex)
            {

            }

        }

        public void DataGridViewToCSV2(string FilePath)
        {
            StreamWriter sw = new StreamWriter(FilePath, true, System.Text.Encoding.GetEncoding(-0));

            string strLine = "";
            try
            {
                //表头
                // strLine += ",";
                strLine += "采集时间";       
              
                for (int i = 0; i < 168; i++)
                {
                    strLine += ",";
                    strLine += CommonTags.OverRollName[i];
                }




                strLine.Remove(strLine.Length - 1);
                sw.WriteLine(strLine);
                strLine = "";
                //表的内容

                StringBuilder sb = new StringBuilder();
                sb.Append("select CellectTime");


                for (int i = 0; i < 168; i++)
                {
                    sb.Append(",Parameter");
                    sb.Append((i + 1).ToString());
                }

                sb.Append(" from table1 where CellectTime > " + "'" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd HH:mm:ss") + "'" + "and CellectTime <" + "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" + "order by CellectTime desc");

                DataTable dt = SQLite.OverRollGetDataSet(sb.ToString()).Tables[0];

                if (dt.Rows.Count > 0)
                {
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        strLine = "";
                        int colCount = dt.Columns.Count;
                        for (int k = 0; k < colCount; k++)
                        {
                            if (k > 0 && k < colCount)
                                strLine += ",";

                            if (dt.Rows[j][k] == null)
                                strLine += "";
                            else
                            {
                                string cell = dt.Rows[j][k].ToString();
                                //防止里面含有特殊符号
                                cell = cell.Replace("\"", "\"\"");
                                cell = "\"" + cell + "\"";
                                strLine += cell;
                            }


                        }
                        sw.WriteLine(strLine);
                    }
                    sw.Close();
                }
            }
            catch (Exception ex)
            {

            }

        }


        #endregion

        #region Byte转化为Float、Bool

        public float[] GetFloat(HslCommunication.OperateResult<byte[]> Bytes, int Start, int Amout)
        {
            int temp_int = Amout / 4;
            float[] array = new float[temp_int];
            byte[] ByteTemp = new byte[4] { 0, 0, 0, 0 };
            int IntTemp;
            try
            {

                for (int i = 0; i < temp_int; i++)
                {
                    IntTemp = i * 4 + Start;
                    ByteTemp[0] = Bytes.Content[IntTemp + 3];
                    ByteTemp[1] = Bytes.Content[IntTemp + 2];
                    ByteTemp[2] = Bytes.Content[IntTemp + 1];
                    ByteTemp[3] = Bytes.Content[IntTemp];
                    array[i] = BitConverter.ToSingle(ByteTemp, 0);

                }
            }
            catch
            {

            }

            return array;

        }

        public bool[] GetBool(HslCommunication.OperateResult<byte[]> Bytes, int Start, int Amount)
        {
            bool[] array = new bool[Amount * 8];
            int Temp_Int;
            int Temp_Int1;
            try
            {
                for (int j = 0; j < Amount; j++)
                {
                    Temp_Int = 8 * j;
                    Temp_Int1 = j + Start;
                    for (int i = 0; i < 8; i++)
                    { //对于byte的每bit进行判定
                        array[Temp_Int + i] = (Bytes.Content[Temp_Int1] & 1) == 1;   //判定byte的最后一位是否为1，若为1，则是true；否则是false
                        Bytes.Content[Temp_Int1] = (byte)(Bytes.Content[Temp_Int1] >> 1);       //将byte右移一位
                    }
                }
            }
            catch
            { }
            return array;
        }

        #endregion

        #region Appconfig读写

        //读取Value值 
        public static string GetConfigString(string key)
        {
            // 
            // TODO: 在此处添加构造函数逻辑 
            // 
            return ConfigurationSettings.AppSettings[key];
        }
        //写操作 
        public static void SetValue(string AppKey, string AppValue)
        {
            XmlDocument xDoc = new XmlDocument();
            //获取可执行文件的路径和名称 
            xDoc.Load(System.Windows.Forms.Application.ExecutablePath + ".config");

            XmlNode xNode;
            XmlElement xElem1;
            XmlElement xElem2;
            xNode = xDoc.SelectSingleNode("//appSettings");

            xElem1 = (XmlElement)xNode.SelectSingleNode("//add[@key='" + AppKey + "']");
            if (xElem1 != null) xElem1.SetAttribute("value", AppValue);
            else
            {
                xElem2 = xDoc.CreateElement("add");
                xElem2.SetAttribute("key", AppKey);
                xElem2.SetAttribute("value", AppValue);
                xNode.AppendChild(xElem2);
            }
            xDoc.Save(System.Windows.Forms.Application.ExecutablePath + ".config");
        }
        #endregion

        #region ini文件操作


        #endregion

        #region 时间划分
        public string DateDiff(DateTime DateTime1, DateTime DateTime2)
        {
            string dateDiff = null;
            try
            {
                TimeSpan ts1 = new TimeSpan(DateTime1.Ticks);
                TimeSpan ts2 = new TimeSpan(DateTime2.Ticks);
                TimeSpan ts = ts1.Subtract(ts2).Duration();
                dateDiff = ts.Days.ToString() + "天"
                        + ts.Hours.ToString() + "小时"
                        + ts.Minutes.ToString() + "分钟"
                        + ts.Seconds.ToString() + "秒"
                        + ts.Milliseconds.ToString() + "毫秒";
            }
            catch
            {

            }
            return dateDiff;
        }
        #endregion

        #region 判断text.Box中是否只含有数字
        public bool NumberOnlyCheck(string text)
        {
            try
            {
                Convert.ToInt32(text);
                return false;

            }
            catch
            {
                try
                {
                    Convert.ToSingle(text);
                    return false;
                }
                catch
                {
                    return true;
                }
            }

        }
        public bool NumberOnlyCheck1(string text)
        {
            try
            {
                Convert.ToInt32(text);
                return true;

            }
            catch
            {
               return false;
                
            }

        }
        public bool FloatCheck(string text)
        {
            try
            {
                Convert.ToSingle(text);
                return true;
            }
            catch
            {
                return false;
            }

        }

        #endregion

        #region groupBox改变颜色

        public void groupBox_Paint(object sender, PaintEventArgs e)
        {
            GroupBox gBox = (GroupBox)sender;

            e.Graphics.Clear(gBox.BackColor);
            e.Graphics.DrawString(gBox.Text, gBox.Font, Brushes.Black, 10, 1);
            var vSize = e.Graphics.MeasureString(gBox.Text, gBox.Font);
            e.Graphics.DrawLine(Pens.Black, 1, vSize.Height / 2, 8, vSize.Height / 2);
            e.Graphics.DrawLine(Pens.Black, vSize.Width + 8, vSize.Height / 2, gBox.Width - 2, vSize.Height / 2);
            e.Graphics.DrawLine(Pens.Black, 1, vSize.Height / 2, 1, gBox.Height - 2);
            e.Graphics.DrawLine(Pens.Black, 1, gBox.Height - 2, gBox.Width - 2, gBox.Height - 2);
            e.Graphics.DrawLine(Pens.Black, gBox.Width - 2, vSize.Height / 2, gBox.Width - 2, gBox.Height - 2);
        }

        public void groupBox_PaintRed(object sender, PaintEventArgs e)
        {
            GroupBox gBox = (GroupBox)sender;

            e.Graphics.Clear(gBox.BackColor);
            e.Graphics.DrawString(gBox.Text, gBox.Font, Brushes.Red, 10, 1);
            var vSize = e.Graphics.MeasureString(gBox.Text, gBox.Font);
            e.Graphics.DrawLine(Pens.Red, 1, vSize.Height / 2, 8, vSize.Height / 2);
            e.Graphics.DrawLine(Pens.Red, vSize.Width + 8, vSize.Height / 2, gBox.Width - 2, vSize.Height / 2);
            e.Graphics.DrawLine(Pens.Red, 1, vSize.Height / 2, 1, gBox.Height - 2);
            e.Graphics.DrawLine(Pens.Red, 1, gBox.Height - 2, gBox.Width - 2, gBox.Height - 2);
            e.Graphics.DrawLine(Pens.Red, gBox.Width - 2, vSize.Height / 2, gBox.Width - 2, gBox.Height - 2);
        }
        #endregion

        #region groupBox改变颜色

        public void Panel_Paint(object sender, PaintEventArgs e)
        {
            Panel gBox = (Panel)sender;
            Pen pen = new Pen(Color.Black, 2);
            e.Graphics.Clear(gBox.BackColor);
            // e.Graphics.DrawString(gBox.Text, gBox.Font, Brushes.Black, 10, 4);
            var vSize = e.Graphics.MeasureString(gBox.Text, gBox.Font);
            e.Graphics.DrawLine(pen, 0, vSize.Height / 2, 8, vSize.Height / 2);
            e.Graphics.DrawLine(pen, vSize.Width + 8, vSize.Height / 2, gBox.Width - 2, vSize.Height / 2);
            e.Graphics.DrawLine(pen, 1, vSize.Height / 2, 1, gBox.Height - 2);
            e.Graphics.DrawLine(pen, 0, gBox.Height - 2, gBox.Width - 2, gBox.Height - 2);
            e.Graphics.DrawLine(pen, gBox.Width - 2, vSize.Height / 2, gBox.Width - 2, gBox.Height - 2);
        }
        #endregion

        #region FTP文件上传
        private NetworkCredential networkCredential;
        public bool FTPFileUpload()
        {
            networkCredential = new NetworkCredential("CM03YCXR01", "SVOLT@2021");
            ////选取要上传的文件
            //OpenFileDialog openFileDlg = new OpenFileDialog();
            //openFileDlg.FileName = openFileDlg.FileNames.ToString();
            //openFileDlg.Filter = "所有文件(*.*)|*.*";
            //if (openFileDlg.ShowDialog() != DialogResult.OK)
            //{
            //    return;
            //}
            FileInfo fileInfo = new FileInfo("C:\\Users\\jkh\\Desktop\\123.PNG");
            try
            {
                string uri = "ftp://czccd.svolt.cn/123.PNG";
                FtpWebRequest request = CreateFtpWebRequest(uri, WebRequestMethods.Ftp.UploadFile);
                request.ContentLength = fileInfo.Length;
                int bufLen = 8196;
                byte[] buf = new byte[bufLen];
                FileStream fileStream = fileInfo.OpenRead();

                Stream responseStream = request.GetRequestStream();

                int contentLen = fileStream.Read(buf, 0, bufLen);
                while (contentLen != 0)
                {
                    responseStream.Write(buf, 0, contentLen);
                    contentLen = fileStream.Read(buf, 0, bufLen);
                }
                responseStream.Close();
                fileStream.Close();
                FtpWebResponse response = GetFtpResponse(request);
                if (response == null)
                {
                    return false;
                }

                MessageBox.Show("上传成功!");
                return true;
            }
            catch (WebException err)
            {

                MessageBox.Show(err.Message, "上传失败!");
                return false;
            }
        }

        //创建FtpWebRequest对象
        private FtpWebRequest CreateFtpWebRequest(string uri, string requestMethod)
        {
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(uri);
            request.Credentials = networkCredential;
            request.KeepAlive = true;
            request.UseBinary = true;
            request.Method = requestMethod;
            // 如果客户端应用程序的数据传输过程侦听数据端口上的连接，则为 false；如果客户端应在数据端口上启动连接，则为 true
            request.UsePassive = false;
            return request;
        }

        private FtpWebResponse GetFtpResponse(FtpWebRequest request)
        {
            FtpWebResponse response = null;
            try
            {
                response = (FtpWebResponse)request.GetResponse();

                return response;
            }
            catch 
            {

                return null;
            }
        }
        #endregion

        #region 其他

        public string DelQuota(string str)//删除特殊字符
        {
            string result = str;
            string[] strQuota = { "~", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "`", ";", "'", ",", "<", ">", "?" };
            for (int i = 0; i < strQuota.Length; i++)
            {
                if (result.IndexOf(strQuota[i]) > -1)
                    result = result.Replace(strQuota[i], "");
            }
            return result;
        }




        public void SetDoubleBuffering(System.Windows.Forms.Control control, bool value)
        {
            System.Reflection.PropertyInfo controlProperty = typeof(System.Windows.Forms.Control)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            controlProperty.SetValue(control, value, null);

        }


        #endregion

        #region dataGridViewToCSV

        public bool dataGridViewToCSV(DataGridView dataGridView)
        {
            if (dataGridView.Rows.Count == 0)
            {
                MessageBox.Show("没有数据可导出!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.CreatePrompt = true;
            saveFileDialog.FileName = null;
            saveFileDialog.Title = "保存";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Stream stream = saveFileDialog.OpenFile();
                StreamWriter sw = new StreamWriter(stream, System.Text.Encoding.GetEncoding(-0));
                string strLine = "";
                try
                {
                    //表头
                    for (int i = 0; i < dataGridView.ColumnCount; i++)
                    {
                        if (i > 0)
                            strLine += ",";
                        strLine += dataGridView.Columns[i].HeaderText;
                    }
                    strLine.Remove(strLine.Length - 1);
                    sw.WriteLine(strLine);
                    strLine = "";
                    //表的内容
                    for (int j = 0; j < dataGridView.Rows.Count; j++)
                    {
                        strLine = "";
                        int colCount = dataGridView.Columns.Count;
                        for (int k = 0; k < colCount; k++)
                        {
                            if (k > 0 && k < colCount)
                                strLine += ",";
                            if (dataGridView.Rows[j].Cells[k].Value == null)
                                strLine += "";
                            else
                            {
                                string cell = dataGridView.Rows[j].Cells[k].Value.ToString().Trim();
                                //防止里面含有特殊符号
                                cell = cell.Replace("\"", "\"\"");
                                cell = "\"" + cell + "\"";
                                strLine += cell;
                            }
                        }
                        sw.WriteLine(strLine);
                    }
                    sw.Close();
                    stream.Close();
                    MessageBox.Show("数据被导出到：" + saveFileDialog.FileName.ToString(), "导出完毕", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "导出错误", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }
            return true;
        }
        #endregion 

        #region CSV文件读取

        public static DataTable OpenCSV(string filePath)//从csv读取数据返回table
        {
            System.Text.Encoding encoding = GetType(filePath); //Encoding.ASCII;//
            DataTable dt = new DataTable();
            System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open,
            System.IO.FileAccess.Read);
            System.IO.StreamReader sr = new System.IO.StreamReader(fs, encoding);
            //记录每次读取的一行记录
            string strLine = "";
            //记录每行记录中的各字段内容
            string[] aryLine = null;
            string[] tableHead = null;
            //标示列数
            int columnCount = 0;
            //标示是否是读取的第一行
            bool IsFirst = true;
            //逐行读取CSV中的数据
            while ((strLine = sr.ReadLine()) != null)
            {
                if (IsFirst == true)
                {
                    tableHead = strLine.Split(',');
                    IsFirst = false;
                    columnCount = tableHead.Length;
                    //创建列
                    for (int i = 0; i < columnCount; i++)
                    {
                        DataColumn dc = new DataColumn(tableHead[i]);
                        dt.Columns.Add(dc);
                    }
                }
                else
                {
                    aryLine = strLine.Split(',');
                    DataRow dr = dt.NewRow();
                    for (int j = 0; j < columnCount; j++)
                    {
                        dr[j] = aryLine[j];
                    }
                    dt.Rows.Add(dr);
                }
            }
            if (aryLine != null && aryLine.Length > 0)
            {
                dt.DefaultView.Sort = tableHead[0] + " " + "asc";
            }
            sr.Close();
            fs.Close();
            return dt;
        }
        /// 给定文件的路径，读取文件的二进制数据，判断文件的编码类型
        /// <param name="FILE_NAME">文件路径</param>
        /// <returns>文件的编码类型</returns>
        public static System.Text.Encoding GetType(string FILE_NAME)
        {
            System.IO.FileStream fs = new System.IO.FileStream(FILE_NAME, System.IO.FileMode.Open,
            System.IO.FileAccess.Read);
            System.Text.Encoding r = GetType(fs);
            fs.Close();
            return r;
        }
        /// 通过给定的文件流，判断文件的编码类型
        /// <param name="fs">文件流</param>
        /// <returns>文件的编码类型</returns>
        public static System.Text.Encoding GetType(System.IO.FileStream fs)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM
            System.Text.Encoding reVal = System.Text.Encoding.Default;
            System.IO.BinaryReader r = new System.IO.BinaryReader(fs, System.Text.Encoding.Default);
            int i;
            int.TryParse(fs.Length.ToString(), out i);
            byte[] ss = r.ReadBytes(i);
            if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
            {
                reVal = System.Text.Encoding.UTF8;
            }
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = System.Text.Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = System.Text.Encoding.Unicode;
            }
            r.Close();
            return reVal;
        }
        /// 判断是否是不带 BOM 的 UTF8 格式
        /// <param name="data"></param>
        /// <returns></returns>
        private static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1; //计算当前正分析的字符应还有的字节数
            byte curByte; //当前分析的字节.
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式");
            }
            return true;
        }
        #endregion


        







    }



}
