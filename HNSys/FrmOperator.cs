using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using HslCommunication.LogNet;

namespace HNSys
{
    public partial class FrmOperator : Form
    {
        #region 声明
        private Dictionary<string, string> dicDeviceAlarm = new Dictionary<string, string>();

        List<HNSys.Model.Formular> Formulars = new List<HNSys.Model.Formular>();


        private Thread FormularInsertMonitor = null;


        CommonMean commonMean = new CommonMean();

        public ILogNet OperationLog { get; set; }
        public ILogNet PLCLog { get; set; }
        public ILogNet ERRLog { get; set; }

        string OperationLogPath = Application.StartupPath + "\\HNLogs\\OperationLog";

        string PLCLogPath = Application.StartupPath + "\\HNLogs\\PLCLog";

        string ERRLogPath = Application.StartupPath + "\\HNLogs\\ERRLog";

        public bool LogloadFlag;

        public string _log = String.Empty;

        public bool FormularUploadFlag;

        public bool FormularDownloadFlag;

        private System.Timers.Timer DelayStart;


        #endregion 

        public FrmOperator()
        {
            InitializeComponent();
        }

        private void FrmOperator_Load(object sender, EventArgs e)
        {
            DGVInit();
            LogInit();

            ThreadInit();
            InitialDeviceParmDataGridView();

            this.DelayStart = new System.Timers.Timer();
            this.DelayStart.Interval = 5000;
            this.DelayStart.AutoReset = true;
            this.DelayStart.Elapsed += DelayStartTime_Elapsed;
            this.DelayStart.Start();

            tabControl1.Enabled = false;
        }

        #region 日志文件初始化

        public void LogInit()
        {
            LogloadFlag = true;
            OperationLog = new LogNetDateTime(Application.StartupPath + "\\HNLogs\\OperationLog", GenerateMode.ByEveryDay);

            PLCLog = new LogNetDateTime(Application.StartupPath + "\\HNLogs\\PLCLog", GenerateMode.ByEveryDay);

            ERRLog = new LogNetDateTime(Application.StartupPath + "\\HNLogs\\ERRLog", GenerateMode.ByEveryDay);
        }



        #endregion

        #region DGV初始化
        public void DGVInit()
        {

            DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
            headerStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle = headerStyle;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.ClearSelection();
            dataGridView1.ColumnCount = 216;

            dataGridView2.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.ColumnHeadersDefaultCellStyle = headerStyle;
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.ClearSelection();
            dataGridView2.ReadOnly = false;




        }

        private void InitialDeviceParmDataGridView()
        {
            #region dataGridView1内容初始化
            Dictionary<string, string> FormulaParamName = new Dictionary<string, string>();
            Dictionary<string, string> FormulaParamName1 = new Dictionary<string, string>();

            dataGridView1.AutoGenerateColumns = false;
            //dataGridView2.AutoGenerateColumns = false;

            FormulaParamName.Clear();
            dataGridView1.Columns.Clear();
            //  dataGridView2.Columns.Clear();

            FormulaParamName.Add("CellectTime", "保存时间");
            FormulaParamName1.Add("CellectTime", "下载时间");

            for (int i = 0; i < 215; i++)
            {
                FormulaParamName.Add("Parameter" + (i + 1).ToString(), CommonTags.FormulaParam[i]);
                FormulaParamName1.Add("Parameter" + (i + 1).ToString(), CommonTags.FormulaParam[i]);
            }

            foreach (var item in FormulaParamName.Keys)
            {
                DataGridViewTextBoxColumn dgvc = new DataGridViewTextBoxColumn();
                dgvc.HeaderText = FormulaParamName[item];
                dgvc.ReadOnly = true;
                dgvc.Width = 150;
                dgvc.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                this.dataGridView1.Columns.Add(dgvc);
            }
            this.dataGridView1.Columns[0].Width = 250;

            foreach (var item in FormulaParamName1.Keys)
            {
                DataGridViewTextBoxColumn dgvc1 = new DataGridViewTextBoxColumn();
                dgvc1.HeaderText = FormulaParamName1[item];
                dgvc1.ReadOnly = true;
                dgvc1.Width = 150;
                dgvc1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                //  this.dataGridView2.Columns.Add(dgvc1);
            }

            //  this.dataGridView2.Columns[0].Width = 250;


            dataGridView1.Columns[0].DataPropertyName = "CellectTime";
            //   dataGridView2.Columns[0].DataPropertyName = "CellectTime";
            for (int i = 0; i < 215; i++)
            {
                dataGridView1.Columns[i + 1].DataPropertyName = "Parameter" + (i + 1).ToString();
                // dataGridView2.Columns[i + 1].DataPropertyName = "Parameter" + (i + 1).ToString();
            }

            //FormulaParamName.Remove("CellectTime");

            #endregion

            #region dataGridView2内容初始化
            for (int i = 1; i < 14; i++)
            {
                DataGridViewTextBoxColumn dgvc = new DataGridViewTextBoxColumn();
                dgvc.HeaderText = i.ToString();
                dgvc.ReadOnly = false;
                dgvc.Width = 142;
                dgvc.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                this.dataGridView2.Columns.Add(dgvc);
            }

            this.dataGridView2.Columns[0].Width = 30;
            

            for (int i = 1; i < 46; i++)
            {
                dataGridView2.Rows.Add((i+1).ToString());
            }

            dataGridView2.Rows[0].Cells[1].Value = CommonTags.FormulaParam[0];//配方名
            dataGridView2.Rows[0].Cells[2].Value = CommonTags.FormulaParam[1];//涂布速度

            for (int i = 1; i < 8; i++)
            {
                dataGridView2.Rows[2].Cells[i].Value = CommonTags.FormulaParam[i + 1];
                dataGridView2.Rows[4].Cells[i].Value = CommonTags.FormulaParam[i + 8];
            }


            for (int i = 1; i < 13; i++)
            {
                dataGridView2.Rows[6].Cells[i].Value = CommonTags.FormulaParam[i + 15];
                dataGridView2.Rows[8].Cells[i].Value = CommonTags.FormulaParam[i + 27];

                if (i < 4)
                {
                    dataGridView2.Rows[10].Cells[i].Value = CommonTags.FormulaParam[i + 39];
                }

                dataGridView2.Rows[12].Cells[i].Value = CommonTags.FormulaParam[i + 42];
                dataGridView2.Rows[14].Cells[i].Value = CommonTags.FormulaParam[i + 54];

                if (i < 4)
                {
                    dataGridView2.Rows[16].Cells[i].Value = CommonTags.FormulaParam[i + 66];
                }

                dataGridView2.Rows[18].Cells[i].Value = CommonTags.FormulaParam[i + 69];
                dataGridView2.Rows[20].Cells[i].Value = CommonTags.FormulaParam[i + 81];
                dataGridView2.Rows[22].Cells[i].Value = CommonTags.FormulaParam[i + 93];
                dataGridView2.Rows[24].Cells[i].Value = CommonTags.FormulaParam[i + 105];
                dataGridView2.Rows[26].Cells[i].Value = CommonTags.FormulaParam[i + 117];
                dataGridView2.Rows[28].Cells[i].Value = CommonTags.FormulaParam[i + 129];

                if (i < 9)
                {
                    dataGridView2.Rows[30].Cells[i].Value = CommonTags.FormulaParam[i + 141];
                    dataGridView2.Rows[32].Cells[i].Value = CommonTags.FormulaParam[i + 149];
                }

                if (i < 5)
                {
                    dataGridView2.Rows[34].Cells[i].Value = CommonTags.FormulaParam[i + 157];
                }

                dataGridView2.Rows[36].Cells[i].Value = CommonTags.FormulaParam[i + 161];

                dataGridView2.Rows[38].Cells[i].Value = CommonTags.FormulaParam[i + 173];

                dataGridView2.Rows[40].Cells[i].Value = CommonTags.FormulaParam[i + 185];

                dataGridView2.Rows[42].Cells[i].Value = CommonTags.FormulaParam[i + 197];

                if (i < 6)
                {
                    dataGridView2.Rows[44].Cells[i].Value = CommonTags.FormulaParam[i + 209];
                }

            }






            #endregion 

        }

        #endregion 

        #region 线程参数初始化
        private void ThreadInit()
        {

            FormularInsertMonitor = new Thread(new System.Threading.ThreadStart(FormularInsertMonitorThreadFunction));
            FormularInsertMonitor.IsBackground = true;
            FormularInsertMonitor.Priority = ThreadPriority.AboveNormal;
            FormularInsertMonitor.Start();
        }
        #endregion

        private void DelayStartTime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ComboxInit();
            tabControl1.Enabled = true;
            DelayStart.Stop();
        }




        public void ComboxInit()
        {
            StringBuilder sb1 = new StringBuilder();
            sb1.Append("select Parameter1");
            sb1.Append(" from table1 order by ID desc limit 0,19");

            DataTable dt1 = SQLite.FormulaGetDataSet(sb1.ToString()).Tables[0];

            comboBox1.Items.Clear();



            if (dt1.Rows.Count > 0)
            {
                try
                {
                    for (int i = 0; i < dt1.Rows.Count; i++)
                    {
                        comboBox1.Items.Add(dt1.Rows[i][0].ToString());
                    }
                }
                catch
                {

                }
            }

        }



        #region 线程函数



        #region 配方下载上传监控
        private void FormularInsertMonitorThreadFunction()
        {
            while (true)
            {
                #region 配方从PLC中上传到上位机
                if (CommonTags.PLC1_REAL[1] == 2)
                {

                    if (!FormularUploadFlag)
                    {
                        LogShowSave("PLCLog", "DB309.DBD4 = 2_PLC请求上传配方_" + CommonTags.PLC1_String);

                        FormularUploadFlag = true;

                        if (CommonTags.PLC1_String.Length == 0)
                        {
                            LogShowSave("ERRLog", "配方名不能为空");
                        }
                        else if (SQLite.FormulaExistJudge(CommonTags.PLC1_String))
                        {
                            LogShowSave("ERRLog", "配方名_" + CommonTags.PLC1_String + "_已存在，不能保存");
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("Insert into table1(CellectTime");

                            for (int i = 0; i < 162; i++)
                            {
                                sb.Append(",Parameter");
                                sb.Append((i + 1).ToString());
                            }
                            sb.Append(") values(");

                            sb.Append("'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" + "," + "'" + CommonTags.PLC1_String + "'");

                            for (int j = 0; j < 161; j++)
                            {
                                sb.Append("," + "'" + CommonTags.PLC1_REAL1[j] + "'");
                            }

                            sb.Append(")");

                            SQLite.FormulaInsert(sb.ToString());
                            ComboxInit();
                            LogShowSave("OperationLog", "配方名_" + CommonTags.PLC1_String + "_上传成功");
                            PLCReadWrite.PLC1Connect.Write("DB309.8", (float)2);
                        }
                    }


                    PLCReadWrite.PLC1Connect.Write("DB309.4", (float)0);
                }
                #endregion

                #region 配方从上位机下载到PLC

                if (CommonTags.PLC1_REAL[1] == 1)
                {
                    if (!FormularDownloadFlag)
                    {
                        LogShowSave("PLCLog", "DB309.DBD4 = 1_PLC请求下载配方_" + CommonTags.PLC1_String);
                        FormularDownloadFlag = true;

                        if (CommonTags.PLC1_String.Length == 0)
                        {
                            LogShowSave("ERRLog", "配方名不能为空");
                        }
                        else if (SQLite.FormulaExistJudge(CommonTags.PLC1_String))
                        {

                            StringBuilder sb1 = new StringBuilder();
                            sb1.Append("select CellectTime");

                            for (int i = 0; i < 213; i++)
                            {
                                sb1.Append(",Parameter");
                                sb1.Append((i + 1).ToString());
                            }

                            sb1.Append(" from table1 where Parameter1 like " + "'" + CommonTags.PLC1_String + "'");

                            DataTable dt1 = SQLite.FormulaGetDataSet(sb1.ToString()).Tables[0];

                            if (dt1.Rows.Count > 0)
                            {
                                HslCommunication.OperateResult WriteResult;

                                #region 配方下载
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1892", float.Parse(dt1.Rows[0][2].ToString()));//涂布速度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1892_涂布速度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1896", float.Parse(dt1.Rows[0][3].ToString()));///放卷张力
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1896_放卷张力_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1900", float.Parse(dt1.Rows[0][4].ToString()));//牵引A张力
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1900_牵引A张力_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1904", float.Parse(dt1.Rows[0][5].ToString()));//机头B张力
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1904_机头B张力_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1908", float.Parse(dt1.Rows[0][6].ToString()));//牵引B张力
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1908_牵引B张力_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1912", float.Parse(dt1.Rows[0][7].ToString()));//牵引C张力
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1912_牵引C张力_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1916", float.Parse(dt1.Rows[0][8].ToString()));//牵引D张力
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1916_牵引D张力_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1920", float.Parse(dt1.Rows[0][9].ToString()));//收卷张力
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1920_收卷张力_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1924", float.Parse(dt1.Rows[0][10].ToString()));//放卷摆辊
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1924_放卷摆辊_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1928", float.Parse(dt1.Rows[0][11].ToString()));//牵引A摆辊
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1928_牵引A摆辊_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1932", float.Parse(dt1.Rows[0][12].ToString()));//机头B摆辊
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1932_机头B摆辊_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1936", float.Parse(dt1.Rows[0][13].ToString()));//牵引B摆辊
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1936_牵引B摆辊_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1940", float.Parse(dt1.Rows[0][14].ToString()));//牵引C摆辊
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1940_牵引C摆辊_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1944", float.Parse(dt1.Rows[0][15].ToString()));//牵引D摆辊
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1944_牵引D摆辊_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1948", float.Parse(dt1.Rows[0][16].ToString()));//收卷摆辊
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1948_收卷摆辊_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1952", float.Parse(dt1.Rows[0][17].ToString()));//烘箱A1鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1952_烘箱A1鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1956", float.Parse(dt1.Rows[0][18].ToString()));//烘箱A2鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1956_烘箱A2鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1960", float.Parse(dt1.Rows[0][19].ToString()));//烘箱A3鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1960_烘箱A3鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1964", float.Parse(dt1.Rows[0][20].ToString()));//烘箱A4鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1964_烘箱A4鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1968", float.Parse(dt1.Rows[0][21].ToString()));//烘箱A5鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1968_烘箱A5鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1972", float.Parse(dt1.Rows[0][22].ToString()));//烘箱A6鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1972_烘箱A6鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1976", float.Parse(dt1.Rows[0][23].ToString()));//烘箱A7鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1976_烘箱A7鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1980", float.Parse(dt1.Rows[0][24].ToString()));//烘箱A8鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1980_烘箱A8鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1984", float.Parse(dt1.Rows[0][25].ToString()));//烘箱A9鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1984_烘箱A9鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1988", float.Parse(dt1.Rows[0][26].ToString()));//烘箱A10鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1988_烘箱A10鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1992", float.Parse(dt1.Rows[0][27].ToString()));//烘箱A11鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1992_烘箱A11鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.1996", float.Parse(dt1.Rows[0][28].ToString()));//烘箱A12鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.1996_烘箱A12鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2000", float.Parse(dt1.Rows[0][29].ToString()));//烘箱A1温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2000_烘箱A1温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2004", float.Parse(dt1.Rows[0][30].ToString()));//烘箱A2温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2004_烘箱A2温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2008", float.Parse(dt1.Rows[0][31].ToString()));//烘箱A3温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2008_烘箱A3温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2012", float.Parse(dt1.Rows[0][32].ToString()));//烘箱A4温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2012_烘箱A4温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2016", float.Parse(dt1.Rows[0][33].ToString()));//烘箱A5温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2016_烘箱A5温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2020", float.Parse(dt1.Rows[0][34].ToString()));//烘箱A6温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2020_烘箱A6温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2024", float.Parse(dt1.Rows[0][35].ToString()));//烘箱A7温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2024_烘箱A7温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2028", float.Parse(dt1.Rows[0][36].ToString()));//烘箱A8温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2028_烘箱A8温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2032", float.Parse(dt1.Rows[0][37].ToString()));//烘箱A9温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2032_烘箱A9温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2036", float.Parse(dt1.Rows[0][38].ToString()));//烘箱A10温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2036_烘箱A10温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2040", float.Parse(dt1.Rows[0][39].ToString()));//烘箱A11温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2040_烘箱A11温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2044", float.Parse(dt1.Rows[0][40].ToString()));//烘箱A12温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2044_烘箱A12温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2048", float.Parse(dt1.Rows[0][41].ToString()));//烘箱A抽风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2048_烘箱A抽风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2052", float.Parse(dt1.Rows[0][42].ToString()));//烘箱A外部抽风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2052_烘箱A外部抽风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2056", float.Parse(dt1.Rows[0][43].ToString()));//烘箱A串联风机频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2056_烘箱A串联风机频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2060", float.Parse(dt1.Rows[0][44].ToString()));//烘箱B1鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2060_烘箱B1鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2064", float.Parse(dt1.Rows[0][45].ToString()));//烘箱B2鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2064_烘箱B2鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2068", float.Parse(dt1.Rows[0][46].ToString()));//烘箱B3鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2068_烘箱B3鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2072", float.Parse(dt1.Rows[0][47].ToString()));//烘箱B4鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2072_烘箱B4鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2076", float.Parse(dt1.Rows[0][48].ToString()));//烘箱B5鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2076_烘箱B5鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2080", float.Parse(dt1.Rows[0][49].ToString()));//烘箱B6鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2080_烘箱B6鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2084", float.Parse(dt1.Rows[0][50].ToString()));//烘箱B7鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2084_烘箱B7鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2088", float.Parse(dt1.Rows[0][51].ToString()));//烘箱B8鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2088_烘箱B8鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2092", float.Parse(dt1.Rows[0][52].ToString()));//烘箱B9鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2092_烘箱B9鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2096", float.Parse(dt1.Rows[0][53].ToString()));//烘箱B10鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2096_烘箱B10鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2100", float.Parse(dt1.Rows[0][54].ToString()));//烘箱B11鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2100_烘箱B11鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2104", float.Parse(dt1.Rows[0][55].ToString()));//烘箱B12鼓风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2104_烘箱B12鼓风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2108", float.Parse(dt1.Rows[0][56].ToString()));//烘箱B1温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2108_烘箱烘箱B1温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2112", float.Parse(dt1.Rows[0][57].ToString()));//烘箱B2温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2112_烘箱烘箱B2温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2116", float.Parse(dt1.Rows[0][58].ToString()));//烘箱B3温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2116_烘箱烘箱B3温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2120", float.Parse(dt1.Rows[0][59].ToString()));//烘箱B4温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2120_烘箱烘箱B4温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2124", float.Parse(dt1.Rows[0][60].ToString()));//烘箱B5温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2124_烘箱烘箱B5温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2128", float.Parse(dt1.Rows[0][61].ToString()));//烘箱B6温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2128_烘箱烘箱B6温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2132", float.Parse(dt1.Rows[0][62].ToString()));//烘箱B7温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2132_烘箱烘箱B7温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2136", float.Parse(dt1.Rows[0][63].ToString()));//烘箱B8温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2136_烘箱烘箱B8温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2140", float.Parse(dt1.Rows[0][64].ToString()));//烘箱B9温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2140_烘箱烘箱B9温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2144", float.Parse(dt1.Rows[0][65].ToString()));// 烘箱B10温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2144_烘箱烘箱B10温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2148", float.Parse(dt1.Rows[0][66].ToString()));//烘箱B11温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2148_烘箱烘箱B11温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2152", float.Parse(dt1.Rows[0][67].ToString()));//烘箱B12温度
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2152_烘箱烘箱B12温度_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2156", float.Parse(dt1.Rows[0][68].ToString()));//烘箱B抽风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2156_烘箱B抽风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2160", float.Parse(dt1.Rows[0][69].ToString()));//烘箱B外部抽风频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2160_烘箱B外部抽风频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2164", float.Parse(dt1.Rows[0][70].ToString()));//烘箱B串联风机频率
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2164_烘箱B串联风机频率_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2168", float.Parse(dt1.Rows[0][71].ToString()));//降频A1
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2168_降频A1_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2172", float.Parse(dt1.Rows[0][72].ToString()));//降频A2
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2172_降频A2_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2176", float.Parse(dt1.Rows[0][73].ToString()));//降频A3
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2176_降频A3_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2180", float.Parse(dt1.Rows[0][74].ToString()));//降频A4
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2180_降频A4_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2184", float.Parse(dt1.Rows[0][75].ToString()));//降频A5
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2184_降频A5_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2188", float.Parse(dt1.Rows[0][76].ToString()));//降频A6
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2188_降频A6_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2192", float.Parse(dt1.Rows[0][77].ToString()));//降频A7
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2192_降频A7_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2196", float.Parse(dt1.Rows[0][78].ToString()));//降频A8
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2196_降频A8_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2200", float.Parse(dt1.Rows[0][79].ToString()));//降频A9
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2200_降频A9_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2204", float.Parse(dt1.Rows[0][80].ToString()));//降频A10
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2204_降频A10_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2208", float.Parse(dt1.Rows[0][81].ToString()));//降频A11
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2208_降频A11_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2212", float.Parse(dt1.Rows[0][82].ToString()));//降频A12
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2212_降频A12_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2216", float.Parse(dt1.Rows[0][83].ToString()));//降频B1
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2216_降频B1_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2220", float.Parse(dt1.Rows[0][84].ToString()));//降频B2
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2220_降频B2_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2224", float.Parse(dt1.Rows[0][85].ToString()));//降频B3
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2224_降频B3_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2228", float.Parse(dt1.Rows[0][86].ToString()));//降频B4
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2228_降频B4_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2232", float.Parse(dt1.Rows[0][87].ToString()));//降频B5
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2232_降频B5_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2236", float.Parse(dt1.Rows[0][88].ToString()));//降频B6
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2236_降频B6_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2240", float.Parse(dt1.Rows[0][89].ToString()));//降频B7
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2240_降频B7_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2244", float.Parse(dt1.Rows[0][90].ToString()));//降频B8
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2244_降频B8_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2248", float.Parse(dt1.Rows[0][91].ToString()));//降频B9
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2248_降频B9_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2252", float.Parse(dt1.Rows[0][92].ToString()));//降频B10
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2252_降频B10_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2256", float.Parse(dt1.Rows[0][93].ToString()));//降频B11
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2256_降频B11_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2260", float.Parse(dt1.Rows[0][94].ToString()));//降频B12
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2260_降频B12_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2264", float.Parse(dt1.Rows[0][95].ToString()));
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2264_降频C1_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2268", float.Parse(dt1.Rows[0][96].ToString()));
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2268_降频C2_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2272", float.Parse(dt1.Rows[0][97].ToString()));
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2272_降频C3_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2276", float.Parse(dt1.Rows[0][98].ToString()));
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2276_降频C4_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2280", float.Parse(dt1.Rows[0][99].ToString()));
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2280_降频C5_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2284", float.Parse(dt1.Rows[0][100].ToString()));//降频C6
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2284_降频C6_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2288", float.Parse(dt1.Rows[0][101].ToString()));//降频C7
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2288_降频C7_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2292", float.Parse(dt1.Rows[0][102].ToString()));//降频C8
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2292_降频C8_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2296", float.Parse(dt1.Rows[0][103].ToString()));//降频C9
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2296_降频C9_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2300", float.Parse(dt1.Rows[0][104].ToString()));//降频C10
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2300_降频C10_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2304", float.Parse(dt1.Rows[0][105].ToString()));//降频C11
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2304_降频C11_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2308", float.Parse(dt1.Rows[0][106].ToString()));//降频C12
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2308_降频C12_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2312", float.Parse(dt1.Rows[0][107].ToString()));//恢复时间A1
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2312_恢复时间A1_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2316", float.Parse(dt1.Rows[0][108].ToString()));//恢复时间A2
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2316_恢复时间A2_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2320", float.Parse(dt1.Rows[0][109].ToString()));//恢复时间A3
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2320_恢复时间A3_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2324", float.Parse(dt1.Rows[0][110].ToString()));//恢复时间A4
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2324_恢复时间A4_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2328", float.Parse(dt1.Rows[0][111].ToString()));//恢复时间A5
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2328_恢复时间A5_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2332", float.Parse(dt1.Rows[0][112].ToString()));//恢复时间A6
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2332_恢复时间A6_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2336", float.Parse(dt1.Rows[0][113].ToString()));//恢复时间A7
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2336_恢复时间A7_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2340", float.Parse(dt1.Rows[0][114].ToString()));//恢复时间A8
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2340_恢复时间A8_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2344", float.Parse(dt1.Rows[0][115].ToString()));//恢复时间A9
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2344_恢复时间A9_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2348", float.Parse(dt1.Rows[0][116].ToString()));//恢复时间A10
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2348_恢复时间A10_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2352", float.Parse(dt1.Rows[0][117].ToString()));//恢复时间A11
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2352_恢复时间A11_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2356", float.Parse(dt1.Rows[0][118].ToString()));//恢复时间A12
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2356_恢复时间A12_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2360", float.Parse(dt1.Rows[0][119].ToString()));//恢复时间B1
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2360_恢复时间B1_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2364", float.Parse(dt1.Rows[0][120].ToString()));//恢复时间B2
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2364_恢复时间B2_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2368", float.Parse(dt1.Rows[0][121].ToString()));//恢复时间B3
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2368_恢复时间B3_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2372", float.Parse(dt1.Rows[0][122].ToString()));//恢复时间B4
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2372_恢复时间B4_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2376", float.Parse(dt1.Rows[0][123].ToString()));//恢复时间B5
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2376_恢复时间B5_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2380", float.Parse(dt1.Rows[0][124].ToString()));//恢复时间B6
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2380_恢复时间B6_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2384", float.Parse(dt1.Rows[0][125].ToString()));//恢复时间B7
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2384_恢复时间B7_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2388", float.Parse(dt1.Rows[0][126].ToString()));//恢复时间B8
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2388_恢复时间B8_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2392", float.Parse(dt1.Rows[0][127].ToString()));//恢复时间B9
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2392_恢复时间B9_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2396", float.Parse(dt1.Rows[0][128].ToString()));//恢复时间B10
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2396_恢复时间B10_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2400", float.Parse(dt1.Rows[0][129].ToString()));//恢复时间B11
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2400_恢复时间B11_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2404", float.Parse(dt1.Rows[0][130].ToString()));//恢复时间B12
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2404_恢复时间B12_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2408", float.Parse(dt1.Rows[0][131].ToString()));//恢复时间C1
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2408_恢复时间C1_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2412", float.Parse(dt1.Rows[0][132].ToString()));//恢复时间C2
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2412_恢复时间C2_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2416", float.Parse(dt1.Rows[0][133].ToString()));//恢复时间C3
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2416_恢复时间C3_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2420", float.Parse(dt1.Rows[0][134].ToString()));//恢复时间C4
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2420_恢复时间C4_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2424", float.Parse(dt1.Rows[0][135].ToString()));//恢复时间C5
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2424_恢复时间C5_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2428", float.Parse(dt1.Rows[0][136].ToString()));//恢复时间C6
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2428_恢复时间C6_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2432", float.Parse(dt1.Rows[0][137].ToString()));//恢复时间C7
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2432_恢复时间C7_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2436", float.Parse(dt1.Rows[0][138].ToString()));//恢复时间C8
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2436_恢复时间C8_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2440", float.Parse(dt1.Rows[0][139].ToString()));//恢复时间C9
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2440_恢复时间C9_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2444", float.Parse(dt1.Rows[0][140].ToString()));//恢复时间C10
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2444_恢复时间C10_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2448", float.Parse(dt1.Rows[0][141].ToString()));//恢复时间C11
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2448_恢复时间C11_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2452", float.Parse(dt1.Rows[0][142].ToString()));//恢复时间C12
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2452_恢复时间C12_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2456", float.Parse(dt1.Rows[0][143].ToString()));//机头A上供料泵1泵速
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2456_机头A上供料泵1泵速_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2460", float.Parse(dt1.Rows[0][144].ToString()));//机头A上供料泵2泵速
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2460_机头A上供料泵2泵速_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2464", float.Parse(dt1.Rows[0][145].ToString()));//机头A下供料泵1泵速
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2464_机头A下供料泵1泵速_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2468", float.Parse(dt1.Rows[0][146].ToString()));//机头A下供料泵2泵速
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2468_机头A下供料泵2泵速_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2472", float.Parse(dt1.Rows[0][147].ToString()));//机头A_AT供料泵1泵速
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2472_机头A_AT供料泵1泵速_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2476", float.Parse(dt1.Rows[0][148].ToString()));//机头A_AT供料泵2泵速
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2476_机头A_AT供料泵2泵速_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2480", float.Parse(dt1.Rows[0][149].ToString()));//机头A_AT供料泵3泵速
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2480_机头A_AT供料泵3泵速_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2484", float.Parse(dt1.Rows[0][150].ToString()));//机头A_AT供料泵4泵速
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2484_机头A_AT供料泵4泵速_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2488", float.Parse(dt1.Rows[0][151].ToString()));//机尾B上供料泵1泵速
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2488_机尾B上供料泵1泵速_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2492", float.Parse(dt1.Rows[0][152].ToString()));//机尾B上供料泵2泵速
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2492_机尾B上供料泵2泵速_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2496", float.Parse(dt1.Rows[0][153].ToString()));//机尾B下供料泵1泵速
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2496_机尾B下供料泵1泵速_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2500", float.Parse(dt1.Rows[0][154].ToString()));//机尾B下供料泵2泵速
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2500_机尾B下供料泵2泵速_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2504", float.Parse(dt1.Rows[0][155].ToString()));//机尾B_AT供料泵1泵速
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2504_机尾B_AT供料泵1泵速_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2508", float.Parse(dt1.Rows[0][156].ToString()));//机尾B_AT供料泵2泵速
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2508_机尾B_AT供料泵2泵速_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2512", float.Parse(dt1.Rows[0][157].ToString()));//机尾B_AT供料泵3泵速
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2512_机尾B_AT供料泵3泵速_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2516", float.Parse(dt1.Rows[0][158].ToString()));//机尾B_AT供料泵4泵速
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2516_机尾B_AT供料泵4泵速_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2520", float.Parse(dt1.Rows[0][159].ToString()));//机头A左刀GAP
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2520_机头A左刀GAP_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2524", float.Parse(dt1.Rows[0][160].ToString()));//机头A右刀GAP
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2524_机头A右刀GAP_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2528", float.Parse(dt1.Rows[0][161].ToString()));//机尾B左刀GAP
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2528_机尾B左刀GAP_写入失败"); goto WriteFalt; }
                                WriteResult = PLCReadWrite.PLC1Connect.Write("DB309.2532", float.Parse(dt1.Rows[0][162].ToString()));//机尾B右刀GAP
                                if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB309.2532_机尾B右刀GAP_写入失败"); goto WriteFalt; }

                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用1
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用1_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用2
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用2_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用3
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用3_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用4
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用4_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用5
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用5_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用6
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用6_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用7
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用7_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用8
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用8_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用9
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用9_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用10
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用10_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用11
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用11_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用12
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用12_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用13
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用13_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用14
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用14_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用15
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用15_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用16
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用16_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用17
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用17_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用18
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用18_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用19
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用19_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用20
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用20_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用21
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用22_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用22
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用22_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用23
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用23_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用24
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用24_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用25
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用25_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用26
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用26_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用27
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用27_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用28
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用28_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用29
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用29_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用30
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用30_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用31
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用31_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用32
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用32_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用33
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用33_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用34
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用34_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用35
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用35_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用36
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用36_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用37
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用37_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用38
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用38_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用39
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用39_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用40
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用40_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用41
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用41_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用42
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用42_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用43
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用43_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用44
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB304.1308_备用44_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用45
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用45_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用46
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用46_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用47
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用47_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用48
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用48_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用49
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用49_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用50
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用50_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用51
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用51_写入失败"); goto WriteFalt; }
                                //WriteResult = PLCReadWrite.PLC1Connect.Write("", (float)0);//备用52
                                //if (!WriteResult.IsSuccess) { LogShowSave("PLCLog", "DB308.1308_备用52_写入失败"); goto WriteFalt; }

                                #endregion

                                LogShowSave("PLCLog", "配方_" + CommonTags.PLC1_String + "_下载成功");
                                PLCReadWrite.PLC1Connect.Write("DB309.8", (float)1);
                            }



                        WriteFalt:;
                        }
                        else
                        {
                            LogShowSave("ERRLog", "配方名_" + CommonTags.PLC1_String + "_不存在，不能下载");
                        }
                    }

                    PLCReadWrite.PLC1Connect.Write("DB309.4", (float)0);
                }

                #endregion 

                if (CommonTags.PLC1_REAL[1] == 0)
                {
                    FormularUploadFlag = false;
                    FormularDownloadFlag = false;

                }

                Thread.Sleep(2000);

            }
        }
        #endregion 

        #endregion       

        #region 查询最近XXX条配方
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (CommonTags.LocalLoginName == "未登录")
            {
                MessageBox.Show("当前未登录，请先登录");
                return;
            }

            StringBuilder sb1 = new StringBuilder();
            sb1.Append("select CellectTime");

            for (int i = 0; i < 213; i++)
            {
                sb1.Append(",Parameter");
                sb1.Append((i + 1).ToString());
            }
            sb1.Append(" from table1 order by ID desc limit 0," + numericUpDown4.Value.ToString());

            DataTable dt1 = SQLite.FormulaGetDataSet(sb1.ToString()).Tables[0];

            if (dt1.Rows.Count > 0)
            {
                Formulars.Clear();

                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    Model.Formular formular = new Model.Formular();

                    #region 参数赋值
                    formular.CellectTime = (dt1.Rows[i][0]).ToString();
                    formular.Parameter1 = dt1.Rows[i][1].ToString();
                    formular.Parameter2 = float.Parse(dt1.Rows[i][2].ToString());
                    formular.Parameter2 = float.Parse(dt1.Rows[i][2].ToString());
                    formular.Parameter3 = float.Parse(dt1.Rows[i][3].ToString());
                    formular.Parameter4 = float.Parse(dt1.Rows[i][4].ToString());
                    formular.Parameter5 = float.Parse(dt1.Rows[i][5].ToString());
                    formular.Parameter6 = float.Parse(dt1.Rows[i][6].ToString());
                    formular.Parameter7 = float.Parse(dt1.Rows[i][7].ToString());
                    formular.Parameter8 = float.Parse(dt1.Rows[i][8].ToString());
                    formular.Parameter9 = float.Parse(dt1.Rows[i][9].ToString());
                    formular.Parameter10 = float.Parse(dt1.Rows[i][10].ToString());
                    formular.Parameter11 = float.Parse(dt1.Rows[i][11].ToString());
                    formular.Parameter12 = float.Parse(dt1.Rows[i][12].ToString());
                    formular.Parameter13 = float.Parse(dt1.Rows[i][13].ToString());
                    formular.Parameter14 = float.Parse(dt1.Rows[i][14].ToString());
                    formular.Parameter15 = float.Parse(dt1.Rows[i][15].ToString());
                    formular.Parameter16 = float.Parse(dt1.Rows[i][16].ToString());
                    formular.Parameter17 = float.Parse(dt1.Rows[i][17].ToString());
                    formular.Parameter18 = float.Parse(dt1.Rows[i][18].ToString());
                    formular.Parameter19 = float.Parse(dt1.Rows[i][19].ToString());
                    formular.Parameter20 = float.Parse(dt1.Rows[i][20].ToString());
                    formular.Parameter21 = float.Parse(dt1.Rows[i][21].ToString());
                    formular.Parameter22 = float.Parse(dt1.Rows[i][22].ToString());
                    formular.Parameter23 = float.Parse(dt1.Rows[i][23].ToString());
                    formular.Parameter24 = float.Parse(dt1.Rows[i][24].ToString());
                    formular.Parameter25 = float.Parse(dt1.Rows[i][25].ToString());
                    formular.Parameter26 = float.Parse(dt1.Rows[i][26].ToString());
                    formular.Parameter27 = float.Parse(dt1.Rows[i][27].ToString());
                    formular.Parameter28 = float.Parse(dt1.Rows[i][28].ToString());
                    formular.Parameter29 = float.Parse(dt1.Rows[i][29].ToString());
                    formular.Parameter30 = float.Parse(dt1.Rows[i][30].ToString());
                    formular.Parameter31 = float.Parse(dt1.Rows[i][31].ToString());
                    formular.Parameter32 = float.Parse(dt1.Rows[i][32].ToString());
                    formular.Parameter33 = float.Parse(dt1.Rows[i][33].ToString());
                    formular.Parameter34 = float.Parse(dt1.Rows[i][34].ToString());
                    formular.Parameter35 = float.Parse(dt1.Rows[i][35].ToString());
                    formular.Parameter36 = float.Parse(dt1.Rows[i][36].ToString());
                    formular.Parameter37 = float.Parse(dt1.Rows[i][37].ToString());
                    formular.Parameter38 = float.Parse(dt1.Rows[i][38].ToString());
                    formular.Parameter39 = float.Parse(dt1.Rows[i][39].ToString());
                    formular.Parameter40 = float.Parse(dt1.Rows[i][40].ToString());
                    formular.Parameter41 = float.Parse(dt1.Rows[i][41].ToString());
                    formular.Parameter42 = float.Parse(dt1.Rows[i][42].ToString());
                    formular.Parameter43 = float.Parse(dt1.Rows[i][43].ToString());
                    formular.Parameter44 = float.Parse(dt1.Rows[i][44].ToString());
                    formular.Parameter45 = float.Parse(dt1.Rows[i][45].ToString());
                    formular.Parameter46 = float.Parse(dt1.Rows[i][46].ToString());
                    formular.Parameter47 = float.Parse(dt1.Rows[i][47].ToString());
                    formular.Parameter48 = float.Parse(dt1.Rows[i][48].ToString());
                    formular.Parameter49 = float.Parse(dt1.Rows[i][49].ToString());
                    formular.Parameter50 = float.Parse(dt1.Rows[i][50].ToString());
                    formular.Parameter51 = float.Parse(dt1.Rows[i][51].ToString());
                    formular.Parameter52 = float.Parse(dt1.Rows[i][52].ToString());
                    formular.Parameter53 = float.Parse(dt1.Rows[i][53].ToString());
                    formular.Parameter54 = float.Parse(dt1.Rows[i][54].ToString());
                    formular.Parameter55 = float.Parse(dt1.Rows[i][55].ToString());
                    formular.Parameter56 = float.Parse(dt1.Rows[i][56].ToString());
                    formular.Parameter57 = float.Parse(dt1.Rows[i][57].ToString());
                    formular.Parameter58 = float.Parse(dt1.Rows[i][58].ToString());
                    formular.Parameter59 = float.Parse(dt1.Rows[i][59].ToString());
                    formular.Parameter60 = float.Parse(dt1.Rows[i][60].ToString());
                    formular.Parameter61 = float.Parse(dt1.Rows[i][61].ToString());
                    formular.Parameter62 = float.Parse(dt1.Rows[i][62].ToString());
                    formular.Parameter63 = float.Parse(dt1.Rows[i][63].ToString());
                    formular.Parameter64 = float.Parse(dt1.Rows[i][64].ToString());
                    formular.Parameter65 = float.Parse(dt1.Rows[i][65].ToString());
                    formular.Parameter66 = float.Parse(dt1.Rows[i][66].ToString());
                    formular.Parameter67 = float.Parse(dt1.Rows[i][67].ToString());
                    formular.Parameter68 = float.Parse(dt1.Rows[i][68].ToString());
                    formular.Parameter69 = float.Parse(dt1.Rows[i][69].ToString());
                    formular.Parameter70 = float.Parse(dt1.Rows[i][70].ToString());
                    formular.Parameter71 = float.Parse(dt1.Rows[i][71].ToString());
                    formular.Parameter72 = float.Parse(dt1.Rows[i][72].ToString());
                    formular.Parameter73 = float.Parse(dt1.Rows[i][73].ToString());
                    formular.Parameter74 = float.Parse(dt1.Rows[i][74].ToString());
                    formular.Parameter75 = float.Parse(dt1.Rows[i][75].ToString());
                    formular.Parameter76 = float.Parse(dt1.Rows[i][76].ToString());
                    formular.Parameter77 = float.Parse(dt1.Rows[i][77].ToString());
                    formular.Parameter78 = float.Parse(dt1.Rows[i][78].ToString());
                    formular.Parameter79 = float.Parse(dt1.Rows[i][79].ToString());
                    formular.Parameter80 = float.Parse(dt1.Rows[i][80].ToString());
                    formular.Parameter81 = float.Parse(dt1.Rows[i][81].ToString());
                    formular.Parameter82 = float.Parse(dt1.Rows[i][82].ToString());
                    formular.Parameter83 = float.Parse(dt1.Rows[i][83].ToString());
                    formular.Parameter84 = float.Parse(dt1.Rows[i][84].ToString());
                    formular.Parameter85 = float.Parse(dt1.Rows[i][85].ToString());
                    formular.Parameter86 = float.Parse(dt1.Rows[i][86].ToString());
                    formular.Parameter87 = float.Parse(dt1.Rows[i][87].ToString());
                    formular.Parameter88 = float.Parse(dt1.Rows[i][88].ToString());
                    formular.Parameter89 = float.Parse(dt1.Rows[i][89].ToString());
                    formular.Parameter90 = float.Parse(dt1.Rows[i][90].ToString());
                    formular.Parameter91 = float.Parse(dt1.Rows[i][91].ToString());
                    formular.Parameter92 = float.Parse(dt1.Rows[i][92].ToString());
                    formular.Parameter93 = float.Parse(dt1.Rows[i][93].ToString());
                    formular.Parameter94 = float.Parse(dt1.Rows[i][94].ToString());
                    formular.Parameter95 = float.Parse(dt1.Rows[i][95].ToString());
                    formular.Parameter96 = float.Parse(dt1.Rows[i][96].ToString());
                    formular.Parameter97 = float.Parse(dt1.Rows[i][97].ToString());
                    formular.Parameter98 = float.Parse(dt1.Rows[i][98].ToString());
                    formular.Parameter99 = float.Parse(dt1.Rows[i][99].ToString());
                    formular.Parameter100 = float.Parse(dt1.Rows[i][100].ToString());
                    formular.Parameter101 = float.Parse(dt1.Rows[i][101].ToString());
                    formular.Parameter102 = float.Parse(dt1.Rows[i][102].ToString());
                    formular.Parameter103 = float.Parse(dt1.Rows[i][103].ToString());
                    formular.Parameter104 = float.Parse(dt1.Rows[i][104].ToString());
                    formular.Parameter105 = float.Parse(dt1.Rows[i][105].ToString());
                    formular.Parameter106 = float.Parse(dt1.Rows[i][106].ToString());
                    formular.Parameter107 = float.Parse(dt1.Rows[i][107].ToString());
                    formular.Parameter108 = float.Parse(dt1.Rows[i][108].ToString());
                    formular.Parameter109 = float.Parse(dt1.Rows[i][109].ToString());
                    formular.Parameter110 = float.Parse(dt1.Rows[i][110].ToString());
                    formular.Parameter111 = float.Parse(dt1.Rows[i][111].ToString());
                    formular.Parameter112 = float.Parse(dt1.Rows[i][112].ToString());
                    formular.Parameter113 = float.Parse(dt1.Rows[i][113].ToString());
                    formular.Parameter114 = float.Parse(dt1.Rows[i][114].ToString());
                    formular.Parameter115 = float.Parse(dt1.Rows[i][115].ToString());
                    formular.Parameter116 = float.Parse(dt1.Rows[i][116].ToString());
                    formular.Parameter117 = float.Parse(dt1.Rows[i][117].ToString());
                    formular.Parameter118 = float.Parse(dt1.Rows[i][118].ToString());
                    formular.Parameter119 = float.Parse(dt1.Rows[i][119].ToString());
                    formular.Parameter120 = float.Parse(dt1.Rows[i][120].ToString());
                    formular.Parameter121 = float.Parse(dt1.Rows[i][121].ToString());
                    formular.Parameter122 = float.Parse(dt1.Rows[i][122].ToString());
                    formular.Parameter123 = float.Parse(dt1.Rows[i][123].ToString());
                    formular.Parameter124 = float.Parse(dt1.Rows[i][124].ToString());
                    formular.Parameter125 = float.Parse(dt1.Rows[i][125].ToString());
                    formular.Parameter126 = float.Parse(dt1.Rows[i][126].ToString());
                    formular.Parameter127 = float.Parse(dt1.Rows[i][127].ToString());
                    formular.Parameter128 = float.Parse(dt1.Rows[i][128].ToString());
                    formular.Parameter129 = float.Parse(dt1.Rows[i][129].ToString());
                    formular.Parameter130 = float.Parse(dt1.Rows[i][130].ToString());
                    formular.Parameter131 = float.Parse(dt1.Rows[i][131].ToString());
                    formular.Parameter132 = float.Parse(dt1.Rows[i][132].ToString());
                    formular.Parameter133 = float.Parse(dt1.Rows[i][133].ToString());
                    formular.Parameter134 = float.Parse(dt1.Rows[i][134].ToString());
                    formular.Parameter135 = float.Parse(dt1.Rows[i][135].ToString());
                    formular.Parameter136 = float.Parse(dt1.Rows[i][136].ToString());
                    formular.Parameter137 = float.Parse(dt1.Rows[i][137].ToString());
                    formular.Parameter138 = float.Parse(dt1.Rows[i][138].ToString());
                    formular.Parameter139 = float.Parse(dt1.Rows[i][139].ToString());
                    formular.Parameter140 = float.Parse(dt1.Rows[i][140].ToString());
                    formular.Parameter141 = float.Parse(dt1.Rows[i][141].ToString());
                    formular.Parameter142 = float.Parse(dt1.Rows[i][142].ToString());
                    formular.Parameter143 = float.Parse(dt1.Rows[i][143].ToString());
                    formular.Parameter144 = float.Parse(dt1.Rows[i][144].ToString());
                    formular.Parameter145 = float.Parse(dt1.Rows[i][145].ToString());
                    formular.Parameter146 = float.Parse(dt1.Rows[i][146].ToString());
                    formular.Parameter147 = float.Parse(dt1.Rows[i][147].ToString());
                    formular.Parameter148 = float.Parse(dt1.Rows[i][148].ToString());
                    formular.Parameter149 = float.Parse(dt1.Rows[i][149].ToString());
                    formular.Parameter150 = float.Parse(dt1.Rows[i][150].ToString());
                    formular.Parameter151 = float.Parse(dt1.Rows[i][151].ToString());
                    formular.Parameter152 = float.Parse(dt1.Rows[i][152].ToString());
                    formular.Parameter153 = float.Parse(dt1.Rows[i][153].ToString());
                    formular.Parameter154 = float.Parse(dt1.Rows[i][154].ToString());
                    formular.Parameter155 = float.Parse(dt1.Rows[i][155].ToString());
                    formular.Parameter156 = float.Parse(dt1.Rows[i][156].ToString());
                    formular.Parameter157 = float.Parse(dt1.Rows[i][157].ToString());
                    formular.Parameter158 = float.Parse(dt1.Rows[i][158].ToString());
                    formular.Parameter159 = float.Parse(dt1.Rows[i][159].ToString());
                    formular.Parameter160 = float.Parse(dt1.Rows[i][160].ToString());
                    formular.Parameter161 = float.Parse(dt1.Rows[i][161].ToString());
                    formular.Parameter162 = float.Parse(dt1.Rows[i][162].ToString());
                    //   formular.Parameter163 = float.Parse(dt1.Rows[i][163].ToString());
                    //formular.Parameter164 = float.Parse(dt1.Rows[i][164].ToString());
                    //formular.Parameter165 = float.Parse(dt1.Rows[i][165].ToString());
                    //formular.Parameter166 = float.Parse(dt1.Rows[i][166].ToString());
                    //formular.Parameter167 = float.Parse(dt1.Rows[i][167].ToString());
                    //formular.Parameter168 = float.Parse(dt1.Rows[i][168].ToString());
                    //formular.Parameter169 = float.Parse(dt1.Rows[i][169].ToString());
                    //formular.Parameter170 = float.Parse(dt1.Rows[i][170].ToString());
                    //formular.Parameter171 = float.Parse(dt1.Rows[i][171].ToString());
                    //formular.Parameter172 = float.Parse(dt1.Rows[i][172].ToString());
                    //formular.Parameter173 = float.Parse(dt1.Rows[i][173].ToString());
                    //formular.Parameter174 = float.Parse(dt1.Rows[i][174].ToString());
                    //formular.Parameter175 = float.Parse(dt1.Rows[i][175].ToString());
                    //formular.Parameter176 = float.Parse(dt1.Rows[i][176].ToString());
                    //formular.Parameter177 = float.Parse(dt1.Rows[i][177].ToString());
                    //formular.Parameter178 = float.Parse(dt1.Rows[i][178].ToString());
                    //formular.Parameter179 = float.Parse(dt1.Rows[i][179].ToString());
                    //formular.Parameter180 = float.Parse(dt1.Rows[i][180].ToString());
                    //formular.Parameter181 = float.Parse(dt1.Rows[i][181].ToString());
                    //formular.Parameter182 = float.Parse(dt1.Rows[i][182].ToString());
                    //formular.Parameter183 = float.Parse(dt1.Rows[i][183].ToString());
                    //formular.Parameter184 = float.Parse(dt1.Rows[i][184].ToString());
                    //formular.Parameter185 = float.Parse(dt1.Rows[i][185].ToString());
                    //formular.Parameter186 = float.Parse(dt1.Rows[i][186].ToString());
                    //formular.Parameter187 = float.Parse(dt1.Rows[i][187].ToString());
                    //formular.Parameter188 = float.Parse(dt1.Rows[i][188].ToString());
                    //formular.Parameter189 = float.Parse(dt1.Rows[i][189].ToString());
                    //formular.Parameter190 = float.Parse(dt1.Rows[i][190].ToString());
                    //formular.Parameter191 = float.Parse(dt1.Rows[i][191].ToString());
                    //formular.Parameter192 = float.Parse(dt1.Rows[i][192].ToString());
                    //formular.Parameter193 = float.Parse(dt1.Rows[i][193].ToString());
                    //formular.Parameter194 = float.Parse(dt1.Rows[i][194].ToString());
                    //formular.Parameter195 = float.Parse(dt1.Rows[i][195].ToString());
                    //formular.Parameter196 = float.Parse(dt1.Rows[i][196].ToString());
                    //formular.Parameter197 = float.Parse(dt1.Rows[i][197].ToString());
                    //formular.Parameter198 = float.Parse(dt1.Rows[i][198].ToString());
                    //formular.Parameter199 = float.Parse(dt1.Rows[i][199].ToString());
                    //formular.Parameter200 = float.Parse(dt1.Rows[i][200].ToString());
                    //formular.Parameter201 = float.Parse(dt1.Rows[i][201].ToString());
                    //formular.Parameter202 = float.Parse(dt1.Rows[i][202].ToString());
                    //formular.Parameter203 = float.Parse(dt1.Rows[i][203].ToString());
                    //formular.Parameter204 = float.Parse(dt1.Rows[i][204].ToString());
                    //formular.Parameter205 = float.Parse(dt1.Rows[i][205].ToString());
                    //formular.Parameter206 = float.Parse(dt1.Rows[i][206].ToString());
                    //formular.Parameter207 = float.Parse(dt1.Rows[i][207].ToString());
                    //formular.Parameter208 = float.Parse(dt1.Rows[i][208].ToString());
                    //formular.Parameter209 = float.Parse(dt1.Rows[i][209].ToString());
                    //formular.Parameter210 = float.Parse(dt1.Rows[i][210].ToString());
                    //formular.Parameter211 = float.Parse(dt1.Rows[i][211].ToString());
                    //formular.Parameter212 = float.Parse(dt1.Rows[i][212].ToString());
                    //formular.Parameter213 = float.Parse(dt1.Rows[i][213].ToString());
                    //formular.Parameter214 = float.Parse(dt1.Rows[i][214].ToString());
                    //formular.Parameter215 = float.Parse(dt1.Rows[i][215].ToString());

                    #endregion

                    Formulars.Add(formular);
                }
                try
                {
                    if (Formulars != null && Formulars.Count > 0)
                    {
                        dataGridView1.DataSource = null;
                        dataGridView1.VirtualMode = false;
                        dataGridView1.AutoGenerateColumns = false;
                        dataGridView1.DataSource = Formulars;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }


        }

        #endregion

        #region 查询配方名为XXX的配方
        private void button5_Click(object sender, EventArgs e)
        {
            if (CommonTags.LocalLoginName == "未登录")
            {
                MessageBox.Show("当前未登录，请先登录");
                return;
            }
            StringBuilder sb1 = new StringBuilder();
            sb1.Append("select CellectTime");

            for (int i = 0; i < 215; i++)
            {
                sb1.Append(",Parameter");
                sb1.Append((i + 1).ToString());
            }
            sb1.Append(" from table1 where Parameter1 like" + "'" + textBox1.Text.Trim() + "'");

            DataTable dt1 = SQLite.FormulaGetDataSet(sb1.ToString()).Tables[0];

            if (dt1.Rows.Count > 0)
            {
                Formulars.Clear();

                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    Model.Formular formular = new Model.Formular();

                    #region 参数赋值
                    formular.CellectTime = (dt1.Rows[i][0]).ToString();
                    formular.Parameter1 = dt1.Rows[i][1].ToString();
                    formular.Parameter2 = float.Parse(dt1.Rows[i][2].ToString());
                    formular.Parameter2 = float.Parse(dt1.Rows[i][2].ToString());
                    formular.Parameter3 = float.Parse(dt1.Rows[i][3].ToString());
                    formular.Parameter4 = float.Parse(dt1.Rows[i][4].ToString());
                    formular.Parameter5 = float.Parse(dt1.Rows[i][5].ToString());
                    formular.Parameter6 = float.Parse(dt1.Rows[i][6].ToString());
                    formular.Parameter7 = float.Parse(dt1.Rows[i][7].ToString());
                    formular.Parameter8 = float.Parse(dt1.Rows[i][8].ToString());
                    formular.Parameter9 = float.Parse(dt1.Rows[i][9].ToString());
                    formular.Parameter10 = float.Parse(dt1.Rows[i][10].ToString());
                    formular.Parameter11 = float.Parse(dt1.Rows[i][11].ToString());
                    formular.Parameter12 = float.Parse(dt1.Rows[i][12].ToString());
                    formular.Parameter13 = float.Parse(dt1.Rows[i][13].ToString());
                    formular.Parameter14 = float.Parse(dt1.Rows[i][14].ToString());
                    formular.Parameter15 = float.Parse(dt1.Rows[i][15].ToString());
                    formular.Parameter16 = float.Parse(dt1.Rows[i][16].ToString());
                    formular.Parameter17 = float.Parse(dt1.Rows[i][17].ToString());
                    formular.Parameter18 = float.Parse(dt1.Rows[i][18].ToString());
                    formular.Parameter19 = float.Parse(dt1.Rows[i][19].ToString());
                    formular.Parameter20 = float.Parse(dt1.Rows[i][20].ToString());
                    formular.Parameter21 = float.Parse(dt1.Rows[i][21].ToString());
                    formular.Parameter22 = float.Parse(dt1.Rows[i][22].ToString());
                    formular.Parameter23 = float.Parse(dt1.Rows[i][23].ToString());
                    formular.Parameter24 = float.Parse(dt1.Rows[i][24].ToString());
                    formular.Parameter25 = float.Parse(dt1.Rows[i][25].ToString());
                    formular.Parameter26 = float.Parse(dt1.Rows[i][26].ToString());
                    formular.Parameter27 = float.Parse(dt1.Rows[i][27].ToString());
                    formular.Parameter28 = float.Parse(dt1.Rows[i][28].ToString());
                    formular.Parameter29 = float.Parse(dt1.Rows[i][29].ToString());
                    formular.Parameter30 = float.Parse(dt1.Rows[i][30].ToString());
                    formular.Parameter31 = float.Parse(dt1.Rows[i][31].ToString());
                    formular.Parameter32 = float.Parse(dt1.Rows[i][32].ToString());
                    formular.Parameter33 = float.Parse(dt1.Rows[i][33].ToString());
                    formular.Parameter34 = float.Parse(dt1.Rows[i][34].ToString());
                    formular.Parameter35 = float.Parse(dt1.Rows[i][35].ToString());
                    formular.Parameter36 = float.Parse(dt1.Rows[i][36].ToString());
                    formular.Parameter37 = float.Parse(dt1.Rows[i][37].ToString());
                    formular.Parameter38 = float.Parse(dt1.Rows[i][38].ToString());
                    formular.Parameter39 = float.Parse(dt1.Rows[i][39].ToString());
                    formular.Parameter40 = float.Parse(dt1.Rows[i][40].ToString());
                    formular.Parameter41 = float.Parse(dt1.Rows[i][41].ToString());
                    formular.Parameter42 = float.Parse(dt1.Rows[i][42].ToString());
                    formular.Parameter43 = float.Parse(dt1.Rows[i][43].ToString());
                    formular.Parameter44 = float.Parse(dt1.Rows[i][44].ToString());
                    formular.Parameter45 = float.Parse(dt1.Rows[i][45].ToString());
                    formular.Parameter46 = float.Parse(dt1.Rows[i][46].ToString());
                    formular.Parameter47 = float.Parse(dt1.Rows[i][47].ToString());
                    formular.Parameter48 = float.Parse(dt1.Rows[i][48].ToString());
                    formular.Parameter49 = float.Parse(dt1.Rows[i][49].ToString());
                    formular.Parameter50 = float.Parse(dt1.Rows[i][50].ToString());
                    formular.Parameter51 = float.Parse(dt1.Rows[i][51].ToString());
                    formular.Parameter52 = float.Parse(dt1.Rows[i][52].ToString());
                    formular.Parameter53 = float.Parse(dt1.Rows[i][53].ToString());
                    formular.Parameter54 = float.Parse(dt1.Rows[i][54].ToString());
                    formular.Parameter55 = float.Parse(dt1.Rows[i][55].ToString());
                    formular.Parameter56 = float.Parse(dt1.Rows[i][56].ToString());
                    formular.Parameter57 = float.Parse(dt1.Rows[i][57].ToString());
                    formular.Parameter58 = float.Parse(dt1.Rows[i][58].ToString());
                    formular.Parameter59 = float.Parse(dt1.Rows[i][59].ToString());
                    formular.Parameter60 = float.Parse(dt1.Rows[i][60].ToString());
                    formular.Parameter61 = float.Parse(dt1.Rows[i][61].ToString());
                    formular.Parameter62 = float.Parse(dt1.Rows[i][62].ToString());
                    formular.Parameter63 = float.Parse(dt1.Rows[i][63].ToString());
                    formular.Parameter64 = float.Parse(dt1.Rows[i][64].ToString());
                    formular.Parameter65 = float.Parse(dt1.Rows[i][65].ToString());
                    formular.Parameter66 = float.Parse(dt1.Rows[i][66].ToString());
                    formular.Parameter67 = float.Parse(dt1.Rows[i][67].ToString());
                    formular.Parameter68 = float.Parse(dt1.Rows[i][68].ToString());
                    formular.Parameter69 = float.Parse(dt1.Rows[i][69].ToString());
                    formular.Parameter70 = float.Parse(dt1.Rows[i][70].ToString());
                    formular.Parameter71 = float.Parse(dt1.Rows[i][71].ToString());
                    formular.Parameter72 = float.Parse(dt1.Rows[i][72].ToString());
                    formular.Parameter73 = float.Parse(dt1.Rows[i][73].ToString());
                    formular.Parameter74 = float.Parse(dt1.Rows[i][74].ToString());
                    formular.Parameter75 = float.Parse(dt1.Rows[i][75].ToString());
                    formular.Parameter76 = float.Parse(dt1.Rows[i][76].ToString());
                    formular.Parameter77 = float.Parse(dt1.Rows[i][77].ToString());
                    formular.Parameter78 = float.Parse(dt1.Rows[i][78].ToString());
                    formular.Parameter79 = float.Parse(dt1.Rows[i][79].ToString());
                    formular.Parameter80 = float.Parse(dt1.Rows[i][80].ToString());
                    formular.Parameter81 = float.Parse(dt1.Rows[i][81].ToString());
                    formular.Parameter82 = float.Parse(dt1.Rows[i][82].ToString());
                    formular.Parameter83 = float.Parse(dt1.Rows[i][83].ToString());
                    formular.Parameter84 = float.Parse(dt1.Rows[i][84].ToString());
                    formular.Parameter85 = float.Parse(dt1.Rows[i][85].ToString());
                    formular.Parameter86 = float.Parse(dt1.Rows[i][86].ToString());
                    formular.Parameter87 = float.Parse(dt1.Rows[i][87].ToString());
                    formular.Parameter88 = float.Parse(dt1.Rows[i][88].ToString());
                    formular.Parameter89 = float.Parse(dt1.Rows[i][89].ToString());
                    formular.Parameter90 = float.Parse(dt1.Rows[i][90].ToString());
                    formular.Parameter91 = float.Parse(dt1.Rows[i][91].ToString());
                    formular.Parameter92 = float.Parse(dt1.Rows[i][92].ToString());
                    formular.Parameter93 = float.Parse(dt1.Rows[i][93].ToString());
                    formular.Parameter94 = float.Parse(dt1.Rows[i][94].ToString());
                    formular.Parameter95 = float.Parse(dt1.Rows[i][95].ToString());
                    formular.Parameter96 = float.Parse(dt1.Rows[i][96].ToString());
                    formular.Parameter97 = float.Parse(dt1.Rows[i][97].ToString());
                    formular.Parameter98 = float.Parse(dt1.Rows[i][98].ToString());
                    formular.Parameter99 = float.Parse(dt1.Rows[i][99].ToString());
                    formular.Parameter100 = float.Parse(dt1.Rows[i][100].ToString());
                    formular.Parameter101 = float.Parse(dt1.Rows[i][101].ToString());
                    formular.Parameter102 = float.Parse(dt1.Rows[i][102].ToString());
                    formular.Parameter103 = float.Parse(dt1.Rows[i][103].ToString());
                    formular.Parameter104 = float.Parse(dt1.Rows[i][104].ToString());
                    formular.Parameter105 = float.Parse(dt1.Rows[i][105].ToString());
                    formular.Parameter106 = float.Parse(dt1.Rows[i][106].ToString());
                    formular.Parameter107 = float.Parse(dt1.Rows[i][107].ToString());
                    formular.Parameter108 = float.Parse(dt1.Rows[i][108].ToString());
                    formular.Parameter109 = float.Parse(dt1.Rows[i][109].ToString());
                    formular.Parameter110 = float.Parse(dt1.Rows[i][110].ToString());
                    formular.Parameter111 = float.Parse(dt1.Rows[i][111].ToString());
                    formular.Parameter112 = float.Parse(dt1.Rows[i][112].ToString());
                    formular.Parameter113 = float.Parse(dt1.Rows[i][113].ToString());
                    formular.Parameter114 = float.Parse(dt1.Rows[i][114].ToString());
                    formular.Parameter115 = float.Parse(dt1.Rows[i][115].ToString());
                    formular.Parameter116 = float.Parse(dt1.Rows[i][116].ToString());
                    formular.Parameter117 = float.Parse(dt1.Rows[i][117].ToString());
                    formular.Parameter118 = float.Parse(dt1.Rows[i][118].ToString());
                    formular.Parameter119 = float.Parse(dt1.Rows[i][119].ToString());
                    formular.Parameter120 = float.Parse(dt1.Rows[i][120].ToString());
                    formular.Parameter121 = float.Parse(dt1.Rows[i][121].ToString());
                    formular.Parameter122 = float.Parse(dt1.Rows[i][122].ToString());
                    formular.Parameter123 = float.Parse(dt1.Rows[i][123].ToString());
                    formular.Parameter124 = float.Parse(dt1.Rows[i][124].ToString());
                    formular.Parameter125 = float.Parse(dt1.Rows[i][125].ToString());
                    formular.Parameter126 = float.Parse(dt1.Rows[i][126].ToString());
                    formular.Parameter127 = float.Parse(dt1.Rows[i][127].ToString());
                    formular.Parameter128 = float.Parse(dt1.Rows[i][128].ToString());
                    formular.Parameter129 = float.Parse(dt1.Rows[i][129].ToString());
                    formular.Parameter130 = float.Parse(dt1.Rows[i][130].ToString());
                    formular.Parameter131 = float.Parse(dt1.Rows[i][131].ToString());
                    formular.Parameter132 = float.Parse(dt1.Rows[i][132].ToString());
                    formular.Parameter133 = float.Parse(dt1.Rows[i][133].ToString());
                    formular.Parameter134 = float.Parse(dt1.Rows[i][134].ToString());
                    formular.Parameter135 = float.Parse(dt1.Rows[i][135].ToString());
                    formular.Parameter136 = float.Parse(dt1.Rows[i][136].ToString());
                    formular.Parameter137 = float.Parse(dt1.Rows[i][137].ToString());
                    formular.Parameter138 = float.Parse(dt1.Rows[i][138].ToString());
                    formular.Parameter139 = float.Parse(dt1.Rows[i][139].ToString());
                    formular.Parameter140 = float.Parse(dt1.Rows[i][140].ToString());
                    formular.Parameter141 = float.Parse(dt1.Rows[i][141].ToString());
                    formular.Parameter142 = float.Parse(dt1.Rows[i][142].ToString());
                    formular.Parameter143 = float.Parse(dt1.Rows[i][143].ToString());
                    formular.Parameter144 = float.Parse(dt1.Rows[i][144].ToString());
                    formular.Parameter145 = float.Parse(dt1.Rows[i][145].ToString());
                    formular.Parameter146 = float.Parse(dt1.Rows[i][146].ToString());
                    formular.Parameter147 = float.Parse(dt1.Rows[i][147].ToString());
                    formular.Parameter148 = float.Parse(dt1.Rows[i][148].ToString());
                    formular.Parameter149 = float.Parse(dt1.Rows[i][149].ToString());
                    formular.Parameter150 = float.Parse(dt1.Rows[i][150].ToString());
                    formular.Parameter151 = float.Parse(dt1.Rows[i][151].ToString());
                    formular.Parameter152 = float.Parse(dt1.Rows[i][152].ToString());
                    formular.Parameter153 = float.Parse(dt1.Rows[i][153].ToString());
                    formular.Parameter154 = float.Parse(dt1.Rows[i][154].ToString());
                    formular.Parameter155 = float.Parse(dt1.Rows[i][155].ToString());
                    formular.Parameter156 = float.Parse(dt1.Rows[i][156].ToString());
                    formular.Parameter157 = float.Parse(dt1.Rows[i][157].ToString());
                    formular.Parameter158 = float.Parse(dt1.Rows[i][158].ToString());
                    formular.Parameter159 = float.Parse(dt1.Rows[i][159].ToString());
                    formular.Parameter160 = float.Parse(dt1.Rows[i][160].ToString());
                    formular.Parameter161 = float.Parse(dt1.Rows[i][161].ToString());
                    formular.Parameter162 = float.Parse(dt1.Rows[i][162].ToString());
                    //formular.Parameter163 = float.Parse(dt1.Rows[i][163].ToString());
                    //formular.Parameter164 = float.Parse(dt1.Rows[i][164].ToString());
                    //formular.Parameter165 = float.Parse(dt1.Rows[i][165].ToString());
                    //formular.Parameter166 = float.Parse(dt1.Rows[i][166].ToString());
                    //formular.Parameter167 = float.Parse(dt1.Rows[i][167].ToString());
                    //formular.Parameter168 = float.Parse(dt1.Rows[i][168].ToString());
                    //formular.Parameter169 = float.Parse(dt1.Rows[i][169].ToString());
                    //formular.Parameter170 = float.Parse(dt1.Rows[i][170].ToString());
                    //formular.Parameter171 = float.Parse(dt1.Rows[i][171].ToString());
                    //formular.Parameter172 = float.Parse(dt1.Rows[i][172].ToString());
                    //formular.Parameter173 = float.Parse(dt1.Rows[i][173].ToString());
                    //formular.Parameter174 = float.Parse(dt1.Rows[i][174].ToString());
                    //formular.Parameter175 = float.Parse(dt1.Rows[i][175].ToString());
                    //formular.Parameter176 = float.Parse(dt1.Rows[i][176].ToString());
                    //formular.Parameter177 = float.Parse(dt1.Rows[i][177].ToString());
                    //formular.Parameter178 = float.Parse(dt1.Rows[i][178].ToString());
                    //formular.Parameter179 = float.Parse(dt1.Rows[i][179].ToString());
                    //formular.Parameter180 = float.Parse(dt1.Rows[i][180].ToString());
                    //formular.Parameter181 = float.Parse(dt1.Rows[i][181].ToString());
                    //formular.Parameter182 = float.Parse(dt1.Rows[i][182].ToString());
                    //formular.Parameter183 = float.Parse(dt1.Rows[i][183].ToString());
                    //formular.Parameter184 = float.Parse(dt1.Rows[i][184].ToString());
                    //formular.Parameter185 = float.Parse(dt1.Rows[i][185].ToString());
                    //formular.Parameter186 = float.Parse(dt1.Rows[i][186].ToString());
                    //formular.Parameter187 = float.Parse(dt1.Rows[i][187].ToString());
                    //formular.Parameter188 = float.Parse(dt1.Rows[i][188].ToString());
                    //formular.Parameter189 = float.Parse(dt1.Rows[i][189].ToString());
                    //formular.Parameter190 = float.Parse(dt1.Rows[i][190].ToString());
                    //formular.Parameter191 = float.Parse(dt1.Rows[i][191].ToString());
                    //formular.Parameter192 = float.Parse(dt1.Rows[i][192].ToString());
                    //formular.Parameter193 = float.Parse(dt1.Rows[i][193].ToString());
                    //formular.Parameter194 = float.Parse(dt1.Rows[i][194].ToString());
                    //formular.Parameter195 = float.Parse(dt1.Rows[i][195].ToString());
                    //formular.Parameter196 = float.Parse(dt1.Rows[i][196].ToString());
                    //formular.Parameter197 = float.Parse(dt1.Rows[i][197].ToString());
                    //formular.Parameter198 = float.Parse(dt1.Rows[i][198].ToString());
                    //formular.Parameter199 = float.Parse(dt1.Rows[i][199].ToString());
                    //formular.Parameter200 = float.Parse(dt1.Rows[i][200].ToString());
                    //formular.Parameter201 = float.Parse(dt1.Rows[i][201].ToString());
                    //formular.Parameter202 = float.Parse(dt1.Rows[i][202].ToString());
                    //formular.Parameter203 = float.Parse(dt1.Rows[i][203].ToString());
                    //formular.Parameter204 = float.Parse(dt1.Rows[i][204].ToString());
                    //formular.Parameter205 = float.Parse(dt1.Rows[i][205].ToString());
                    //formular.Parameter206 = float.Parse(dt1.Rows[i][206].ToString());
                    //formular.Parameter207 = float.Parse(dt1.Rows[i][207].ToString());
                    //formular.Parameter208 = float.Parse(dt1.Rows[i][208].ToString());
                    //formular.Parameter209 = float.Parse(dt1.Rows[i][209].ToString());
                    //formular.Parameter210 = float.Parse(dt1.Rows[i][210].ToString());
                    //formular.Parameter211 = float.Parse(dt1.Rows[i][211].ToString());
                    //formular.Parameter212 = float.Parse(dt1.Rows[i][212].ToString());
                    //formular.Parameter213 = float.Parse(dt1.Rows[i][213].ToString());


                    #endregion

                    Formulars.Add(formular);
                }
                try
                {
                    if (Formulars != null && Formulars.Count > 0)
                    {
                        dataGridView1.DataSource = null;
                        dataGridView1.VirtualMode = false;
                        dataGridView1.AutoGenerateColumns = false;
                        dataGridView1.DataSource = Formulars;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        #endregion

        #region 删除配方名为XXX的配方
        private void button6_Click(object sender, EventArgs e)
        {
            if (CommonTags.LocalLoginName == "未登录")
            {
                MessageBox.Show("当前未登录，请先登录");
                return;
            }

            SQLite.FormularDelate(textBox3.Text.Trim());
            ComboxInit();

            MessageBox.Show("配方删除成功");

        }
        #endregion

        #region 日志

        public void LogShowSave(string category, string log)
        {
            switch (category)
            {
                case "OperationLog":
                    OperationLog.WriteAnyString("【" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + "】" + log);
                    break;

                case "PLCLog":
                    PLCLog.WriteAnyString("【" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + "】" + log);
                    break;

                case "ERRLog":
                    ERRLog.WriteAnyString("【" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + "】" + log);
                    break;

                default:
                    break;
            }

            Log = log;
        }








        public string Log
        {
            get
            {
                return _log;
            }
            set
            {
                if (LogloadFlag)
                {
                    _log = $"{"【" + DateTime.Now.ToString("yyyyMMdd") + "_" + DateTime.Now.ToString("HH:mm:ss") + "】"}{value}{Environment.NewLine}{_log}";
                }
                else
                {
                    _log = $"{value}{Environment.NewLine}{_log}";
                }
                this.Invoke(new Action(() =>
                {
                    if (_log.Length > 50000000)
                    {
                        _log = string.Empty;
                    }
                    if (_log.Length > 10000)
                    {
                        textBox1.Text = _log.Substring(0, 5000).Trim();
                    }
                    else
                    {
                        textBox2.Text = _log.Trim();
                    }


                }));
            }




        }




        #endregion

        #region 手动导出查询数据
        private void button8_Click(object sender, EventArgs e)
        {
            if (CommonTags.LocalLoginName == "未登录")
            {
                MessageBox.Show("当前未登录，请先登录");
                return;
            }
            commonMean.dataGridViewToCSV(dataGridView1);
        }
        #endregion

        #region 从CSV文件中导入配方

        private void button3_Click(object sender, EventArgs e)
        {
            if (CommonTags.LocalLoginName == "未登录")
            {
                MessageBox.Show("当前未登录，请先登录");
                return;
            }
            string path;
            DataTable aaa = new DataTable();
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog1.FileName.ToString();

                aaa = CommonMean.OpenCSV(path);

                #region 赋值
                dataGridView2.Rows[1].Cells[1].Value = aaa.Rows[0][1].ToString().Replace("\"", "");
                dataGridView2.Rows[1].Cells[2].Value = aaa.Rows[0][2].ToString().Replace("\"", "");

                for (int i = 1; i < 8; i++)
                {
                    //dataGridView2.Rows[2].Cells[i].Value = aaa.Columns[i+3].Caption.ToString();//
                    dataGridView2.Rows[3].Cells[i].Value = aaa.Rows[0][i + 2].ToString().Replace("\"", "");
                    //dataGridView2.Rows[4].Cells[i].Value = aaa.Columns[i + 10].Caption.ToString();
                    dataGridView2.Rows[5].Cells[i].Value = aaa.Rows[0][i + 9].ToString().Replace("\"", "");

                }


                for (int i = 1; i < 13; i++)
                {
                    // dataGridView2.Rows[6].Cells[i].Value = aaa.Columns[i + 17].Caption.ToString();
                    dataGridView2.Rows[7].Cells[i].Value = aaa.Rows[0][i + 16].ToString().Replace("\"", "");

                    //dataGridView2.Rows[8].Cells[i].Value = aaa.Columns[i + 29].Caption.ToString();
                    dataGridView2.Rows[9].Cells[i].Value = aaa.Rows[0][i + 28].ToString().Replace("\"", "");

                    if (i < 4)
                    {
                        //  dataGridView2.Rows[10].Cells[i].Value = aaa.Columns[i + 41].Caption.ToString();
                        dataGridView2.Rows[11].Cells[i].Value = aaa.Rows[0][i + 40].ToString().Replace("\"", "");
                    }

                    //dataGridView2.Rows[12].Cells[i].Value = aaa.Columns[i + 44].Caption.ToString();
                    dataGridView2.Rows[13].Cells[i].Value = aaa.Rows[0][i + 43].ToString().Replace("\"", "");

                    //dataGridView2.Rows[14].Cells[i].Value = aaa.Columns[i + 56].Caption.ToString();
                    dataGridView2.Rows[15].Cells[i].Value = aaa.Rows[0][i + 55].ToString().Replace("\"", "");

                    if (i < 4)
                    {
                        //dataGridView2.Rows[16].Cells[i].Value = aaa.Columns[i + 68].Caption.ToString();
                        dataGridView2.Rows[17].Cells[i].Value = aaa.Rows[0][i + 67].ToString().Replace("\"", "");
                    }

                    //dataGridView2.Rows[18].Cells[i].Value = aaa.Columns[i + 71].Caption.ToString();
                    dataGridView2.Rows[19].Cells[i].Value = aaa.Rows[0][i + 70].ToString().Replace("\"", "");

                    //dataGridView2.Rows[20].Cells[i].Value = aaa.Columns[i + 83].Caption.ToString();
                    dataGridView2.Rows[21].Cells[i].Value = aaa.Rows[0][i + 82].ToString().Replace("\"", "");

                    // dataGridView2.Rows[22].Cells[i].Value = aaa.Columns[i + 95].Caption.ToString();
                    dataGridView2.Rows[23].Cells[i].Value = aaa.Rows[0][i + 94].ToString().Replace("\"", "");


                    //dataGridView2.Rows[24].Cells[i].Value = aaa.Columns[i + 107].Caption.ToString();
                    dataGridView2.Rows[25].Cells[i].Value = aaa.Rows[0][i + 106].ToString().Replace("\"", "");

                    // dataGridView2.Rows[26].Cells[i].Value = aaa.Columns[i + 119].Caption.ToString();
                    dataGridView2.Rows[27].Cells[i].Value = aaa.Rows[0][i + 118].ToString().Replace("\"", "");

                    //dataGridView2.Rows[28].Cells[i].Value = aaa.Columns[i + 131].Caption.ToString();
                    dataGridView2.Rows[29].Cells[i].Value = aaa.Rows[0][i + 130].ToString().Replace("\"", "");


                    if (i < 9)
                    {
                        // dataGridView2.Rows[30].Cells[i].Value = aaa.Columns[i + 143].Caption.ToString();
                        dataGridView2.Rows[31].Cells[i].Value = aaa.Rows[0][i + 142].ToString().Replace("\"", "");

                        //dataGridView2.Rows[32].Cells[i].Value = aaa.Columns[i + 151].Caption.ToString();
                        dataGridView2.Rows[33].Cells[i].Value = aaa.Rows[0][i + 150].ToString().Replace("\"", "");

                    }

                    if (i < 5)
                    {
                        //dataGridView2.Rows[34].Cells[i].Value = aaa.Columns[i + 159].Caption.ToString();
                        dataGridView2.Rows[35].Cells[i].Value = aaa.Rows[0][i + 158].ToString().Replace("\"", "");
                    }

                    //dataGridView2.Rows[36].Cells[i].Value = aaa.Columns[i + 163].Caption.ToString();
                    dataGridView2.Rows[37].Cells[i].Value = aaa.Rows[0][i + 162].ToString().Replace("\"", "");

                    //dataGridView2.Rows[38].Cells[i].Value = aaa.Columns[i + 175].Caption.ToString();
                    dataGridView2.Rows[39].Cells[i].Value = aaa.Rows[0][i + 174].ToString().Replace("\"", "");

                    //dataGridView2.Rows[40].Cells[i].Value = aaa.Columns[i + 187].Caption.ToString();
                    dataGridView2.Rows[41].Cells[i].Value = aaa.Rows[0][i + 186].ToString().Replace("\"", "");

                    //dataGridView2.Rows[42].Cells[i].Value = aaa.Columns[i + 199].Caption.ToString();
                    dataGridView2.Rows[43].Cells[i].Value = aaa.Rows[0][i + 198].ToString().Replace("\"", "");

                    if (i < 6)
                    {
                        //dataGridView2.Rows[44].Cells[i].Value = aaa.Columns[i + 211].Caption.ToString();
                        dataGridView2.Rows[45].Cells[i].Value = aaa.Rows[0][i + 210].ToString().Replace("\"", "");
                    }

                }
                #endregion
            }
        }
        #endregion

        private void button4_Click(object sender, EventArgs e)
        {
            if (CommonTags.LocalLoginName == "未登录")
            {
                MessageBox.Show("当前未登录，请先登录");
                return;
            }

            if (!FormatCheck())
            {
                return;
            }

            StringBuilder sb1 = new StringBuilder();
            DataTable dt1 = new DataTable();

            sb1.Append("select CellectTime");
            for (int i = 0; i < 215; i++)
            {
                sb1.Append(",Parameter");
                sb1.Append((i + 1).ToString());
            }

            sb1.Append(" from table1 where Parameter1 like" + "'" + dataGridView2.Rows[1].Cells[1].Value.ToString() + "'");
            dt1 = SQLite.FormulaGetDataSet(sb1.ToString()).Tables[0];

            if (dt1.Rows.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                #region SQL语句拼接
                sb.Append("UPDATE table1 SET ");
                sb.Append("CellectTime='"); sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.Append("',Parameter1='");  sb.Append(dataGridView2.Rows[1].Cells[1].Value.ToString());
                sb.Append("',Parameter2='");  sb.Append(dataGridView2.Rows[1].Cells[2].Value.ToString());
                sb.Append("',Parameter3='");  sb.Append(dataGridView2.Rows[3].Cells[1].Value.ToString());
                sb.Append("',Parameter4='");  sb.Append(dataGridView2.Rows[3].Cells[2].Value.ToString());
                sb.Append("',Parameter5='");  sb.Append(dataGridView2.Rows[3].Cells[3].Value.ToString());
                sb.Append("',Parameter6='");  sb.Append(dataGridView2.Rows[3].Cells[4].Value.ToString());
                sb.Append("',Parameter7='");  sb.Append(dataGridView2.Rows[3].Cells[5].Value.ToString());
                sb.Append("',Parameter8='");  sb.Append(dataGridView2.Rows[3].Cells[6].Value.ToString());
                sb.Append("',Parameter9='");  sb.Append(dataGridView2.Rows[3].Cells[7].Value.ToString());
                sb.Append("',Parameter10='"); sb.Append(dataGridView2.Rows[5].Cells[1].Value.ToString());
                sb.Append("',Parameter11='"); sb.Append(dataGridView2.Rows[5].Cells[2].Value.ToString());
                sb.Append("',Parameter12='"); sb.Append(dataGridView2.Rows[5].Cells[3].Value.ToString());
                sb.Append("',Parameter13='"); sb.Append(dataGridView2.Rows[5].Cells[4].Value.ToString());
                sb.Append("',Parameter14='"); sb.Append(dataGridView2.Rows[5].Cells[5].Value.ToString());
                sb.Append("',Parameter15='"); sb.Append(dataGridView2.Rows[5].Cells[6].Value.ToString());
                sb.Append("',Parameter16='"); sb.Append(dataGridView2.Rows[5].Cells[7].Value.ToString());
                sb.Append("',Parameter17='"); sb.Append(dataGridView2.Rows[7].Cells[1].Value.ToString());
                sb.Append("',Parameter18='"); sb.Append(dataGridView2.Rows[7].Cells[2].Value.ToString());
                sb.Append("',Parameter19='"); sb.Append(dataGridView2.Rows[7].Cells[3].Value.ToString());
                sb.Append("',Parameter20='"); sb.Append(dataGridView2.Rows[7].Cells[4].Value.ToString());
                sb.Append("',Parameter21='"); sb.Append(dataGridView2.Rows[7].Cells[5].Value.ToString());
                sb.Append("',Parameter22='"); sb.Append(dataGridView2.Rows[7].Cells[6].Value.ToString());
                sb.Append("',Parameter23='"); sb.Append(dataGridView2.Rows[7].Cells[7].Value.ToString());
                sb.Append("',Parameter24='"); sb.Append(dataGridView2.Rows[7].Cells[8].Value.ToString());
                sb.Append("',Parameter25='"); sb.Append(dataGridView2.Rows[7].Cells[9].Value.ToString());
                sb.Append("',Parameter26='"); sb.Append(dataGridView2.Rows[7].Cells[10].Value.ToString());
                sb.Append("',Parameter27='"); sb.Append(dataGridView2.Rows[7].Cells[11].Value.ToString());
                sb.Append("',Parameter28='"); sb.Append(dataGridView2.Rows[7].Cells[12].Value.ToString());
                sb.Append("',Parameter29='"); sb.Append(dataGridView2.Rows[9].Cells[1].Value.ToString());
                sb.Append("',Parameter30='"); sb.Append(dataGridView2.Rows[9].Cells[2].Value.ToString());
                sb.Append("',Parameter31='"); sb.Append(dataGridView2.Rows[9].Cells[3].Value.ToString());
                sb.Append("',Parameter32='"); sb.Append(dataGridView2.Rows[9].Cells[4].Value.ToString());
                sb.Append("',Parameter33='"); sb.Append(dataGridView2.Rows[9].Cells[5].Value.ToString());
                sb.Append("',Parameter34='"); sb.Append(dataGridView2.Rows[9].Cells[6].Value.ToString());
                sb.Append("',Parameter35='"); sb.Append(dataGridView2.Rows[9].Cells[7].Value.ToString());
                sb.Append("',Parameter36='"); sb.Append(dataGridView2.Rows[9].Cells[8].Value.ToString());
                sb.Append("',Parameter37='"); sb.Append(dataGridView2.Rows[9].Cells[9].Value.ToString());
                sb.Append("',Parameter38='"); sb.Append(dataGridView2.Rows[9].Cells[10].Value.ToString());
                sb.Append("',Parameter39='"); sb.Append(dataGridView2.Rows[9].Cells[11].Value.ToString());
                sb.Append("',Parameter40='"); sb.Append(dataGridView2.Rows[9].Cells[12].Value.ToString());
                sb.Append("',Parameter41='"); sb.Append(dataGridView2.Rows[11].Cells[1].Value.ToString());
                sb.Append("',Parameter42='"); sb.Append(dataGridView2.Rows[11].Cells[2].Value.ToString());
                sb.Append("',Parameter43='"); sb.Append(dataGridView2.Rows[11].Cells[3].Value.ToString());
                sb.Append("',Parameter44='"); sb.Append(dataGridView2.Rows[13].Cells[1].Value.ToString());
                sb.Append("',Parameter45='"); sb.Append(dataGridView2.Rows[13].Cells[2].Value.ToString());
                sb.Append("',Parameter46='"); sb.Append(dataGridView2.Rows[13].Cells[3].Value.ToString());
                sb.Append("',Parameter47='"); sb.Append(dataGridView2.Rows[13].Cells[4].Value.ToString());
                sb.Append("',Parameter48='"); sb.Append(dataGridView2.Rows[13].Cells[5].Value.ToString());
                sb.Append("',Parameter49='"); sb.Append(dataGridView2.Rows[13].Cells[6].Value.ToString());
                sb.Append("',Parameter50='"); sb.Append(dataGridView2.Rows[13].Cells[7].Value.ToString());
                sb.Append("',Parameter51='"); sb.Append(dataGridView2.Rows[13].Cells[8].Value.ToString());
                sb.Append("',Parameter52='"); sb.Append(dataGridView2.Rows[13].Cells[9].Value.ToString());
                sb.Append("',Parameter53='"); sb.Append(dataGridView2.Rows[13].Cells[10].Value.ToString());
                sb.Append("',Parameter54='"); sb.Append(dataGridView2.Rows[13].Cells[11].Value.ToString());
                sb.Append("',Parameter55='"); sb.Append(dataGridView2.Rows[13].Cells[12].Value.ToString());
                sb.Append("',Parameter56='"); sb.Append(dataGridView2.Rows[15].Cells[1].Value.ToString());
                sb.Append("',Parameter57='"); sb.Append(dataGridView2.Rows[15].Cells[2].Value.ToString());
                sb.Append("',Parameter58='"); sb.Append(dataGridView2.Rows[15].Cells[3].Value.ToString());
                sb.Append("',Parameter59='"); sb.Append(dataGridView2.Rows[15].Cells[4].Value.ToString());
                sb.Append("',Parameter60='"); sb.Append(dataGridView2.Rows[15].Cells[5].Value.ToString());
                sb.Append("',Parameter61='"); sb.Append(dataGridView2.Rows[15].Cells[6].Value.ToString());
                sb.Append("',Parameter62='"); sb.Append(dataGridView2.Rows[15].Cells[7].Value.ToString());
                sb.Append("',Parameter63='"); sb.Append(dataGridView2.Rows[15].Cells[8].Value.ToString());
                sb.Append("',Parameter64='"); sb.Append(dataGridView2.Rows[15].Cells[9].Value.ToString());
                sb.Append("',Parameter65='"); sb.Append(dataGridView2.Rows[15].Cells[10].Value.ToString());
                sb.Append("',Parameter66='"); sb.Append(dataGridView2.Rows[15].Cells[11].Value.ToString());
                sb.Append("',Parameter67='"); sb.Append(dataGridView2.Rows[15].Cells[12].Value.ToString());
                sb.Append("',Parameter68='"); sb.Append(dataGridView2.Rows[17].Cells[1].Value.ToString());
                sb.Append("',Parameter69='"); sb.Append(dataGridView2.Rows[17].Cells[2].Value.ToString());
                sb.Append("',Parameter70='"); sb.Append(dataGridView2.Rows[17].Cells[3].Value.ToString());
                sb.Append("',Parameter71='"); sb.Append(dataGridView2.Rows[19].Cells[1].Value.ToString());
                sb.Append("',Parameter72='"); sb.Append(dataGridView2.Rows[19].Cells[2].Value.ToString());
                sb.Append("',Parameter73='"); sb.Append(dataGridView2.Rows[19].Cells[3].Value.ToString());
                sb.Append("',Parameter74='"); sb.Append(dataGridView2.Rows[19].Cells[4].Value.ToString());
                sb.Append("',Parameter75='"); sb.Append(dataGridView2.Rows[19].Cells[5].Value.ToString());
                sb.Append("',Parameter76='"); sb.Append(dataGridView2.Rows[19].Cells[6].Value.ToString());
                sb.Append("',Parameter77='"); sb.Append(dataGridView2.Rows[19].Cells[7].Value.ToString());
                sb.Append("',Parameter78='"); sb.Append(dataGridView2.Rows[19].Cells[8].Value.ToString());
                sb.Append("',Parameter79='"); sb.Append(dataGridView2.Rows[19].Cells[9].Value.ToString());
                sb.Append("',Parameter80='"); sb.Append(dataGridView2.Rows[19].Cells[10].Value.ToString());
                sb.Append("',Parameter81='"); sb.Append(dataGridView2.Rows[19].Cells[11].Value.ToString());
                sb.Append("',Parameter82='"); sb.Append(dataGridView2.Rows[19].Cells[12].Value.ToString());
                sb.Append("',Parameter83='"); sb.Append(dataGridView2.Rows[21].Cells[1].Value.ToString());
                sb.Append("',Parameter84='"); sb.Append(dataGridView2.Rows[21].Cells[2].Value.ToString());
                sb.Append("',Parameter85='"); sb.Append(dataGridView2.Rows[21].Cells[3].Value.ToString());
                sb.Append("',Parameter86='"); sb.Append(dataGridView2.Rows[21].Cells[4].Value.ToString());
                sb.Append("',Parameter87='"); sb.Append(dataGridView2.Rows[21].Cells[5].Value.ToString());
                sb.Append("',Parameter88='"); sb.Append(dataGridView2.Rows[21].Cells[6].Value.ToString());
                sb.Append("',Parameter89='"); sb.Append(dataGridView2.Rows[21].Cells[7].Value.ToString());
                sb.Append("',Parameter90='"); sb.Append(dataGridView2.Rows[21].Cells[8].Value.ToString());
                sb.Append("',Parameter91='"); sb.Append(dataGridView2.Rows[21].Cells[9].Value.ToString());
                sb.Append("',Parameter92='"); sb.Append(dataGridView2.Rows[21].Cells[10].Value.ToString());
                sb.Append("',Parameter93='"); sb.Append(dataGridView2.Rows[21].Cells[11].Value.ToString());
                sb.Append("',Parameter94='"); sb.Append(dataGridView2.Rows[21].Cells[12].Value.ToString());
                sb.Append("',Parameter95='"); sb.Append(dataGridView2.Rows[23].Cells[1].Value.ToString());
                sb.Append("',Parameter96='"); sb.Append(dataGridView2.Rows[23].Cells[2].Value.ToString());
                sb.Append("',Parameter97='"); sb.Append(dataGridView2.Rows[23].Cells[3].Value.ToString());
                sb.Append("',Parameter98='"); sb.Append(dataGridView2.Rows[23].Cells[4].Value.ToString());
                sb.Append("',Parameter99='"); sb.Append(dataGridView2.Rows[23].Cells[5].Value.ToString());
                sb.Append("',Parameter100='"); sb.Append(dataGridView2.Rows[23].Cells[6].Value.ToString());
                sb.Append("',Parameter101='"); sb.Append(dataGridView2.Rows[23].Cells[7].Value.ToString());
                sb.Append("',Parameter102='"); sb.Append(dataGridView2.Rows[23].Cells[8].Value.ToString());
                sb.Append("',Parameter103='"); sb.Append(dataGridView2.Rows[23].Cells[9].Value.ToString());
                sb.Append("',Parameter104='"); sb.Append(dataGridView2.Rows[23].Cells[10].Value.ToString());
                sb.Append("',Parameter105='"); sb.Append(dataGridView2.Rows[23].Cells[11].Value.ToString());
                sb.Append("',Parameter106='"); sb.Append(dataGridView2.Rows[23].Cells[12].Value.ToString());
                sb.Append("',Parameter107='"); sb.Append(dataGridView2.Rows[25].Cells[1].Value.ToString());
                sb.Append("',Parameter108='"); sb.Append(dataGridView2.Rows[25].Cells[2].Value.ToString());
                sb.Append("',Parameter109='"); sb.Append(dataGridView2.Rows[25].Cells[3].Value.ToString());
                sb.Append("',Parameter110='"); sb.Append(dataGridView2.Rows[25].Cells[4].Value.ToString());
                sb.Append("',Parameter111='"); sb.Append(dataGridView2.Rows[25].Cells[5].Value.ToString());
                sb.Append("',Parameter112='"); sb.Append(dataGridView2.Rows[25].Cells[6].Value.ToString());
                sb.Append("',Parameter113='"); sb.Append(dataGridView2.Rows[25].Cells[7].Value.ToString());
                sb.Append("',Parameter114='"); sb.Append(dataGridView2.Rows[25].Cells[8].Value.ToString());
                sb.Append("',Parameter115='"); sb.Append(dataGridView2.Rows[25].Cells[9].Value.ToString());
                sb.Append("',Parameter116='"); sb.Append(dataGridView2.Rows[25].Cells[10].Value.ToString());
                sb.Append("',Parameter117='"); sb.Append(dataGridView2.Rows[25].Cells[11].Value.ToString());
                sb.Append("',Parameter118='"); sb.Append(dataGridView2.Rows[25].Cells[12].Value.ToString());
                sb.Append("',Parameter119='"); sb.Append(dataGridView2.Rows[27].Cells[1].Value.ToString());
                sb.Append("',Parameter120='"); sb.Append(dataGridView2.Rows[27].Cells[2].Value.ToString());
                sb.Append("',Parameter121='"); sb.Append(dataGridView2.Rows[27].Cells[3].Value.ToString());
                sb.Append("',Parameter122='"); sb.Append(dataGridView2.Rows[27].Cells[4].Value.ToString());
                sb.Append("',Parameter123='"); sb.Append(dataGridView2.Rows[27].Cells[5].Value.ToString());
                sb.Append("',Parameter124='"); sb.Append(dataGridView2.Rows[27].Cells[6].Value.ToString());
                sb.Append("',Parameter125='"); sb.Append(dataGridView2.Rows[27].Cells[7].Value.ToString());
                sb.Append("',Parameter126='"); sb.Append(dataGridView2.Rows[27].Cells[8].Value.ToString());
                sb.Append("',Parameter127='"); sb.Append(dataGridView2.Rows[27].Cells[9].Value.ToString());
                sb.Append("',Parameter128='"); sb.Append(dataGridView2.Rows[27].Cells[10].Value.ToString());
                sb.Append("',Parameter129='"); sb.Append(dataGridView2.Rows[27].Cells[11].Value.ToString());
                sb.Append("',Parameter130='"); sb.Append(dataGridView2.Rows[27].Cells[12].Value.ToString());
                sb.Append("',Parameter131='"); sb.Append(dataGridView2.Rows[29].Cells[1].Value.ToString());
                sb.Append("',Parameter132='"); sb.Append(dataGridView2.Rows[29].Cells[2].Value.ToString());
                sb.Append("',Parameter133='"); sb.Append(dataGridView2.Rows[29].Cells[3].Value.ToString());
                sb.Append("',Parameter134='"); sb.Append(dataGridView2.Rows[29].Cells[4].Value.ToString());
                sb.Append("',Parameter135='"); sb.Append(dataGridView2.Rows[29].Cells[5].Value.ToString());
                sb.Append("',Parameter136='"); sb.Append(dataGridView2.Rows[29].Cells[6].Value.ToString());
                sb.Append("',Parameter137='"); sb.Append(dataGridView2.Rows[29].Cells[7].Value.ToString());
                sb.Append("',Parameter138='"); sb.Append(dataGridView2.Rows[29].Cells[8].Value.ToString());
                sb.Append("',Parameter139='"); sb.Append(dataGridView2.Rows[29].Cells[9].Value.ToString());
                sb.Append("',Parameter140='"); sb.Append(dataGridView2.Rows[29].Cells[10].Value.ToString());
                sb.Append("',Parameter141='"); sb.Append(dataGridView2.Rows[29].Cells[11].Value.ToString());
                sb.Append("',Parameter142='"); sb.Append(dataGridView2.Rows[29].Cells[12].Value.ToString());
                sb.Append("',Parameter143='"); sb.Append(dataGridView2.Rows[31].Cells[1].Value.ToString());
                sb.Append("',Parameter144='"); sb.Append(dataGridView2.Rows[31].Cells[2].Value.ToString());
                sb.Append("',Parameter145='"); sb.Append(dataGridView2.Rows[31].Cells[3].Value.ToString());
                sb.Append("',Parameter146='"); sb.Append(dataGridView2.Rows[31].Cells[4].Value.ToString());
                sb.Append("',Parameter147='"); sb.Append(dataGridView2.Rows[31].Cells[5].Value.ToString());
                sb.Append("',Parameter148='"); sb.Append(dataGridView2.Rows[31].Cells[6].Value.ToString());
                sb.Append("',Parameter149='"); sb.Append(dataGridView2.Rows[31].Cells[7].Value.ToString());
                sb.Append("',Parameter150='"); sb.Append(dataGridView2.Rows[31].Cells[8].Value.ToString());
                sb.Append("',Parameter151='"); sb.Append(dataGridView2.Rows[33].Cells[1].Value.ToString());
                sb.Append("',Parameter152='"); sb.Append(dataGridView2.Rows[33].Cells[2].Value.ToString());
                sb.Append("',Parameter153='"); sb.Append(dataGridView2.Rows[33].Cells[3].Value.ToString());
                sb.Append("',Parameter154='"); sb.Append(dataGridView2.Rows[33].Cells[4].Value.ToString());
                sb.Append("',Parameter155='"); sb.Append(dataGridView2.Rows[33].Cells[5].Value.ToString());
                sb.Append("',Parameter156='"); sb.Append(dataGridView2.Rows[33].Cells[6].Value.ToString());
                sb.Append("',Parameter157='"); sb.Append(dataGridView2.Rows[33].Cells[7].Value.ToString());
                sb.Append("',Parameter158='"); sb.Append(dataGridView2.Rows[33].Cells[8].Value.ToString());
                sb.Append("',Parameter159='"); sb.Append(dataGridView2.Rows[35].Cells[1].Value.ToString());
                sb.Append("',Parameter160='"); sb.Append(dataGridView2.Rows[35].Cells[2].Value.ToString());
                sb.Append("',Parameter161='"); sb.Append(dataGridView2.Rows[35].Cells[3].Value.ToString());
                sb.Append("',Parameter162='"); sb.Append(dataGridView2.Rows[35].Cells[4].Value.ToString());
                sb.Append("' WHERE Parameter1='" + dataGridView2.Rows[1].Cells[1].Value.ToString());
                sb.Append("'");
                #endregion

                SQLite.FormulaInsert(sb.ToString());

                MessageBox.Show("配方保存成功");
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                #region sql语句拼接
                sb.Append("Insert into table1(CellectTime");

                for (int i = 0; i < 162; i++)
                {
                    sb.Append(",Parameter");
                    sb.Append((i + 1).ToString());
                }
                sb.Append(") values(");

                sb.Append("'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" + "," + "'" + dataGridView2.Rows[1].Cells[1].Value.ToString() + "'");

                sb.Append("," + "'" +dataGridView2.Rows[1].Cells[2].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[3].Cells[1].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[3].Cells[2].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[3].Cells[3].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[3].Cells[4].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[3].Cells[5].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[3].Cells[6].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[3].Cells[7].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[5].Cells[1].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[5].Cells[2].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[5].Cells[3].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[5].Cells[4].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[5].Cells[5].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[5].Cells[6].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[5].Cells[7].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[7].Cells[1].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[7].Cells[2].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[7].Cells[3].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[7].Cells[4].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[7].Cells[5].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[7].Cells[6].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[7].Cells[7].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[7].Cells[8].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[7].Cells[9].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[7].Cells[10].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[7].Cells[11].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[7].Cells[12].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[9].Cells[1].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[9].Cells[2].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[9].Cells[3].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[9].Cells[4].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[9].Cells[5].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[9].Cells[6].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[9].Cells[7].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[9].Cells[8].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[9].Cells[9].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[9].Cells[10].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[9].Cells[11].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[9].Cells[12].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[11].Cells[1].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[11].Cells[2].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[11].Cells[3].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[13].Cells[1].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[13].Cells[2].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[13].Cells[3].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[13].Cells[4].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[13].Cells[5].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[13].Cells[6].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[13].Cells[7].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[13].Cells[8].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[13].Cells[9].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[13].Cells[10].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[13].Cells[11].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[13].Cells[12].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[15].Cells[1].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[15].Cells[2].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[15].Cells[3].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[15].Cells[4].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[15].Cells[5].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[15].Cells[6].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[15].Cells[7].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[15].Cells[8].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[15].Cells[9].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[15].Cells[10].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[15].Cells[11].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[15].Cells[12].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[17].Cells[1].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[17].Cells[2].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[17].Cells[3].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[19].Cells[1].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[19].Cells[2].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[19].Cells[3].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[19].Cells[4].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[19].Cells[5].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[19].Cells[6].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[19].Cells[7].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[19].Cells[8].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[19].Cells[9].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[19].Cells[10].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[19].Cells[11].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[19].Cells[12].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[21].Cells[1].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[21].Cells[2].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[21].Cells[3].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[21].Cells[4].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[21].Cells[5].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[21].Cells[6].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[21].Cells[7].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[21].Cells[8].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[21].Cells[9].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[21].Cells[10].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[21].Cells[11].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[21].Cells[12].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[23].Cells[1].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[23].Cells[2].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[23].Cells[3].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[23].Cells[4].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[23].Cells[5].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[23].Cells[6].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[23].Cells[7].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[23].Cells[8].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[23].Cells[9].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[23].Cells[10].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[23].Cells[11].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[23].Cells[12].Value.ToString()+"'");
                sb.Append("," + "'" +dataGridView2.Rows[25].Cells[1].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[25].Cells[2].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[25].Cells[3].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[25].Cells[4].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[25].Cells[5].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[25].Cells[6].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[25].Cells[7].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[25].Cells[8].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[25].Cells[9].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[25].Cells[10].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[25].Cells[11].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[25].Cells[12].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[27].Cells[1].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[27].Cells[2].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[27].Cells[3].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[27].Cells[4].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[27].Cells[5].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[27].Cells[6].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[27].Cells[7].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[27].Cells[8].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[27].Cells[9].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[27].Cells[10].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[27].Cells[11].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[27].Cells[12].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[29].Cells[1].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[29].Cells[2].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[29].Cells[3].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[29].Cells[4].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[29].Cells[5].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[29].Cells[6].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[29].Cells[7].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[29].Cells[8].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[29].Cells[9].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[29].Cells[10].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[29].Cells[11].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[29].Cells[12].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[31].Cells[1].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[31].Cells[2].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[31].Cells[3].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[31].Cells[4].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[31].Cells[5].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[31].Cells[6].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[31].Cells[7].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[31].Cells[8].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[33].Cells[1].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[33].Cells[2].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[33].Cells[3].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[33].Cells[4].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[33].Cells[5].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[33].Cells[6].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[33].Cells[7].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[33].Cells[8].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[35].Cells[1].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[35].Cells[2].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[35].Cells[3].Value.ToString() +"'");
                sb.Append("," + "'" +dataGridView2.Rows[35].Cells[4].Value.ToString() + "'");
                sb.Append(")");

                #endregion 

                SQLite.FormulaInsert(sb.ToString());
                ComboxInit();
                MessageBox.Show("配方保存成功");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CommonTags.LocalLoginName == "未登录")
            {
                MessageBox.Show("当前未登录，请先登录");
                return;
            }

            StringBuilder sb1 = new StringBuilder();
            sb1.Append("select CellectTime");
            for (int i = 0; i < 215; i++)
            {
                sb1.Append(",Parameter");
                sb1.Append((i + 1).ToString());
            }

            sb1.Append(" from table1 where Parameter1 like" + "'" + comboBox1.Text.Trim() + "'");
            DataTable dt1 = SQLite.FormulaGetDataSet(sb1.ToString()).Tables[0];

            if (dt1.Rows.Count > 0)
            {
                #region 赋值
                dataGridView2.Rows[1].Cells[1].Value = dt1.Rows[0][1].ToString().Replace("\"", "");
                dataGridView2.Rows[1].Cells[2].Value = dt1.Rows[0][2].ToString().Replace("\"", "");

                for (int i = 1; i < 8; i++)
                {
                    //dataGridView2.Rows[2].Cells[i].Value = dt1.Columns[i+3].Caption.ToString();//
                    dataGridView2.Rows[3].Cells[i].Value = dt1.Rows[0][i + 2].ToString().Replace("\"", "");
                    //dataGridView2.Rows[4].Cells[i].Value = dt1.Columns[i + 10].Caption.ToString();
                    dataGridView2.Rows[5].Cells[i].Value = dt1.Rows[0][i + 9].ToString().Replace("\"", "");

                }


                for (int i = 1; i < 13; i++)
                {
                    // dataGridView2.Rows[6].Cells[i].Value = dt1.Columns[i + 17].Caption.ToString();
                    dataGridView2.Rows[7].Cells[i].Value = dt1.Rows[0][i + 16].ToString().Replace("\"", "");

                    //dataGridView2.Rows[8].Cells[i].Value = dt1.Columns[i + 29].Caption.ToString();
                    dataGridView2.Rows[9].Cells[i].Value = dt1.Rows[0][i + 28].ToString().Replace("\"", "");

                    if (i < 4)
                    {
                        //  dataGridView2.Rows[10].Cells[i].Value = dt1.Columns[i + 41].Caption.ToString();
                        dataGridView2.Rows[11].Cells[i].Value = dt1.Rows[0][i + 40].ToString().Replace("\"", "");
                    }

                    //dataGridView2.Rows[12].Cells[i].Value = dt1.Columns[i + 44].Caption.ToString();
                    dataGridView2.Rows[13].Cells[i].Value = dt1.Rows[0][i + 43].ToString().Replace("\"", "");

                    //dataGridView2.Rows[14].Cells[i].Value = dt1.Columns[i + 56].Caption.ToString();
                    dataGridView2.Rows[15].Cells[i].Value = dt1.Rows[0][i + 55].ToString().Replace("\"", "");

                    if (i < 4)
                    {
                        //dataGridView2.Rows[16].Cells[i].Value = dt1.Columns[i + 68].Caption.ToString();
                        dataGridView2.Rows[17].Cells[i].Value = dt1.Rows[0][i + 67].ToString().Replace("\"", "");
                    }

                    //dataGridView2.Rows[18].Cells[i].Value = dt1.Columns[i + 71].Caption.ToString();
                    dataGridView2.Rows[19].Cells[i].Value = dt1.Rows[0][i + 70].ToString().Replace("\"", "");

                    //dataGridView2.Rows[20].Cells[i].Value = dt1.Columns[i + 83].Caption.ToString();
                    dataGridView2.Rows[21].Cells[i].Value = dt1.Rows[0][i + 82].ToString().Replace("\"", "");

                    // dataGridView2.Rows[22].Cells[i].Value = dt1.Columns[i + 95].Caption.ToString();
                    dataGridView2.Rows[23].Cells[i].Value = dt1.Rows[0][i + 94].ToString().Replace("\"", "");


                    //dataGridView2.Rows[24].Cells[i].Value = dt1.Columns[i + 107].Caption.ToString();
                    dataGridView2.Rows[25].Cells[i].Value = dt1.Rows[0][i + 106].ToString().Replace("\"", "");

                    // dataGridView2.Rows[26].Cells[i].Value = dt1.Columns[i + 119].Caption.ToString();
                    dataGridView2.Rows[27].Cells[i].Value = dt1.Rows[0][i + 118].ToString().Replace("\"", "");

                    //dataGridView2.Rows[28].Cells[i].Value = dt1.Columns[i + 131].Caption.ToString();
                    dataGridView2.Rows[29].Cells[i].Value = dt1.Rows[0][i + 130].ToString().Replace("\"", "");


                    if (i < 9)
                    {
                        // dataGridView2.Rows[30].Cells[i].Value = dt1.Columns[i + 143].Caption.ToString();
                        dataGridView2.Rows[31].Cells[i].Value = dt1.Rows[0][i + 142].ToString().Replace("\"", "");

                        //dataGridView2.Rows[32].Cells[i].Value = dt1.Columns[i + 151].Caption.ToString();
                        dataGridView2.Rows[33].Cells[i].Value = dt1.Rows[0][i + 150].ToString().Replace("\"", "");

                    }

                    if (i < 5)
                    {
                        //dataGridView2.Rows[34].Cells[i].Value = dt1.Columns[i + 159].Caption.ToString();
                        dataGridView2.Rows[35].Cells[i].Value = dt1.Rows[0][i + 158].ToString().Replace("\"", "");
                    }

                    //dataGridView2.Rows[36].Cells[i].Value = dt1.Columns[i + 163].Caption.ToString();
                    dataGridView2.Rows[37].Cells[i].Value = dt1.Rows[0][i + 162].ToString().Replace("\"", "");

                    //dataGridView2.Rows[38].Cells[i].Value = dt1.Columns[i + 175].Caption.ToString();
                    dataGridView2.Rows[39].Cells[i].Value = dt1.Rows[0][i + 174].ToString().Replace("\"", "");

                    //dataGridView2.Rows[40].Cells[i].Value = dt1.Columns[i + 187].Caption.ToString();
                    dataGridView2.Rows[41].Cells[i].Value = dt1.Rows[0][i + 186].ToString().Replace("\"", "");

                    //dataGridView2.Rows[42].Cells[i].Value = dt1.Columns[i + 199].Caption.ToString();
                    dataGridView2.Rows[43].Cells[i].Value = dt1.Rows[0][i + 198].ToString().Replace("\"", "");

                    if (i < 6)
                    {
                        //dataGridView2.Rows[44].Cells[i].Value = dt1.Columns[i + 211].Caption.ToString();
                        dataGridView2.Rows[45].Cells[i].Value = dt1.Rows[0][i + 210].ToString().Replace("\"", "");
                    }

                }
                #endregion
            }
        }

        public bool FormatCheck()
        {
            #region 空格检测
            for (int i = 1; i < 37; i++)
            {
                for (int j = 1; j < 13; j++)
                {
                    if (i == 1 && j < 3)
                    {
                        if (dataGridView2.Rows[i].Cells[j].Value == null)
                        {
                            MessageBox.Show("第" + (i + 2).ToString() + "行第" + (j + 1).ToString() + "列为空！");
                            return false;
                        }
                    }

                    if ((i == 3 || i == 5) && j < 8)
                    {
                        if (dataGridView2.Rows[i].Cells[j].Value == null)
                        {
                            MessageBox.Show("第" + (i + 2).ToString() + "行第" + (j + 1).ToString() + "列为空！");
                            return false;
                        }
                    }

                    if ((i == 7 || i == 9 || i == 13 || i == 15 || i == 19 || i == 21 || i == 23 || i == 25 || i == 27 || i == 29 || i == 37 || i == 39 || i == 41 || i == 43) && j < 13)
                    {
                        if (dataGridView2.Rows[i].Cells[j].Value == null)
                        {
                            MessageBox.Show("第" + (i + 2).ToString() + "行第" + (j + 1).ToString() + "列为空！");
                            return false;
                        }
                    }

                    if ((i == 11 || i == 17) && j < 4)
                    {
                        if (dataGridView2.Rows[i].Cells[j].Value == null)
                        {
                            MessageBox.Show("第" + (i + 2).ToString() + "行第" + (j + 1).ToString() + "列为空！");
                            return false;
                        }
                    }

                    if ((i == 31 || i == 33) && j < 9)
                    {
                        if (dataGridView2.Rows[i].Cells[j].Value == null)
                        {
                            MessageBox.Show("第" + (i + 2).ToString() + "行第" + (j + 1).ToString() + "列为空！");
                            return false;
                        }
                    }

                    if (i == 35  && j < 5)
                    {
                        if (dataGridView2.Rows[i].Cells[j].Value == null)
                        {
                            MessageBox.Show("第" + (i + 2).ToString() + "行第" + (j + 1).ToString() + "列为空！");
                            return false;
                        }
                    }

                    if (i == 45 && j < 6)
                    {
                        if (dataGridView2.Rows[i].Cells[j].Value == null)
                        {
                            MessageBox.Show("第" + (i + 2).ToString() + "行第" + (j + 1).ToString() + "列为空！");
                            return false;
                        }
                    }
                }
            }
            #endregion

            #region 格式检测
            for (int i = 1; i < 37; i++)
            {
                for (int j = 1; j < 13; j++)
                {
                    if (i == 1 && j == 2)
                    {
                        if (commonMean.NumberOnlyCheck(dataGridView2.Rows[i].Cells[j].Value.ToString()))
                        {
                            MessageBox.Show("第" + (i + 2).ToString() + "行第" + (j + 1).ToString() + "列格式错误，请检查！");
                            return false;
                        }
                    }

                    if ((i == 3 || i == 5) && j < 8)
                    {
                        if (commonMean.NumberOnlyCheck(dataGridView2.Rows[i].Cells[j].Value.ToString()))
                        {
                            MessageBox.Show("第" + (i + 2).ToString() + "行第" + (j + 1).ToString() + "列格式错误，请检查！");
                            return false;
                        }
                    }

                    if ((i == 7 || i == 9 || i == 13 || i == 15 || i == 19 || i == 21 || i == 23 || i == 25 || i == 27 || i == 29 || i == 37 || i == 39 || i == 41 || i == 43) && j < 13)
                    {
                        if (commonMean.NumberOnlyCheck(dataGridView2.Rows[i].Cells[j].Value.ToString()))
                        {
                            MessageBox.Show("第" + (i + 2).ToString() + "行第" + (j + 1).ToString() + "列格式错误，请检查！");
                            return false;
                        }
                    }

                    if ((i == 11 || i == 17) && j < 4)
                    {
                        if (commonMean.NumberOnlyCheck(dataGridView2.Rows[i].Cells[j].Value.ToString()))
                        {
                            MessageBox.Show("第" + (i + 2).ToString() + "行第" + (j + 1).ToString() + "列格式错误，请检查！");
                            return false;
                        }
                    }

                    if ((i == 31 || i == 33) && j < 9)
                    {
                        if (commonMean.NumberOnlyCheck(dataGridView2.Rows[i].Cells[j].Value.ToString()))
                        {
                            MessageBox.Show("第" + (i + 2).ToString() + "行第" + (j + 1).ToString() + "列格式错误，请检查！");
                            return false;
                        }
                    }

                    if (i == 35 && j < 5)
                    {
                        if (commonMean.NumberOnlyCheck(dataGridView2.Rows[i].Cells[j].Value.ToString()))
                        {
                            MessageBox.Show("第" + (i + 2).ToString() + "行第" + (j + 1).ToString() + "列格式错误，请检查！");
                            return false;
                        }
                    }

                    if (i == 45 && j < 6)
                    {
                        if (commonMean.NumberOnlyCheck(dataGridView2.Rows[i].Cells[j].Value.ToString()))
                        {
                            MessageBox.Show("第" + (i + 2).ToString() + "行第" + (j + 1).ToString() + "列格式错误，请检查！");
                            return false;
                        }
                    }
                }
            }
            #endregion

            return true;
               
        }

    }
}
