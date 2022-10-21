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
using System.IO;
using Microsoft.Win32;

using HslCommunication.Profinet.Siemens;
using HslCommunication.LogNet;
using INIOpration;
using System.Data.SQLite;

namespace HNSys
{
    public partial class FrmMain : Form
    {
      //  public DeDeviceStatus statusLogTrant;

        FrmReport frmReport = new FrmReport();
        FrmOperator frmOperator = new FrmOperator();
        FrmRTCurvecs  frmRTCurvecs = new FrmRTCurvecs();
        FrmCurvecsChoice frmCurvecsChoice = new FrmCurvecsChoice();
        FrmCurvecsChoice1 frmCurvecsChoice1 = new FrmCurvecsChoice1();
        FrmSet frmSet = new FrmSet();



        CommonMean commonMean = new CommonMean();
     
        private Thread PLCTagRead = null;
        private Thread AlarmMonitor = null;
        private Thread ParameterSave = null;

        private Thread CurvecsParamete = null;

        private Thread CurvecsParamete1 = null;

        private Thread CurvecsParameteSave = null;

        private Thread CurvecsParameteSave1 = null;

        private Thread UIUpdate = null;

        private System.Timers.Timer LoginOutTime;

        private System.Timers.Timer DelayStart;

        private bool PLCFirstOnline = false;
        private Dictionary<string, string> dicDeviceAlarm = new Dictionary<string, string>();

        private ScanerHook listener = new ScanerHook();  

        public FrmMain()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;//使新创建的线程能访问UI线程创建的窗口控件
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            this.Visible = false;
            FileCreate();
            ConfigInit();
           
            PLCReadWrite.PLCConnectInit();         

            ThreadInit(); 
            
            InterfaceLoad();

            SubscribeInit();

            Splasher.Status = "初始化完毕......";
            Splasher.Close();

            this.Visible = true;
            label6.Text = CommonTags.LocalLoginName;

            this.LoginOutTime = new System.Timers.Timer();
            this.LoginOutTime.Interval = 180000;
            this.LoginOutTime.AutoReset = true;
            this.LoginOutTime.Elapsed += LoginOutTime_Elapsed;

            this.DelayStart = new System.Timers.Timer();
            this.DelayStart.Interval = 3000;
            this.DelayStart.AutoReset = true;
            this.DelayStart.Elapsed += DelayStartTime_Elapsed;
            this.DelayStart.Start();

            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;

            #region 开机自启动
            //string commonStartup = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);
            //string AppName = "HNSys";
            //string AppFile = Application.ExecutablePath;
            //ShortcutTool.Create(commonStartup, AppName, AppFile);

            //string startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            //ShortcutTool.Create(startup, AppName, AppFile);
            #endregion

            #region 读卡器登录监听启动
            
            listener.Start();

            #endregion
        }

        #region 订阅事件初始化
        public void SubscribeInit()
        {
            //历史
            frmCurvecsChoice.ControlSwitch1 += SubscribeEvent1;
            frmCurvecsChoice.ControlSwitch2 += SubscribeEvent2;
            frmCurvecsChoice.ControlSwitch3 += SubscribeEvent3;
            frmCurvecsChoice.ControlSwitch4 += SubscribeEvent7;
            frmCurvecsChoice.ControlSwitch5 += SubscribeEvent8;
            frmCurvecsChoice.ControlSwitch6 += SubscribeEvent9;
            frmCurvecsChoice.ControlSwitch7 += SubscribeEvent10;

            //实时
            frmCurvecsChoice1.ControlSwitch1 += SubscribeEvent4;
            frmCurvecsChoice1.ControlSwitch2 += SubscribeEvent5;
            frmCurvecsChoice1.ControlSwitch3 += SubscribeEvent6;
            frmCurvecsChoice1.ControlSwitch4 += SubscribeEvent11;
            frmCurvecsChoice1.ControlSwitch5 += SubscribeEvent12;
            frmCurvecsChoice1.ControlSwitch6 += SubscribeEvent13;
            frmCurvecsChoice1.ControlSwitch7 += SubscribeEvent14;


            listener.ScanerEvent += Listener_ScanerEvent;
        }
        #endregion

        #region 创建定时导出EXCEL文件夹
        public void FileCreate()
        {           
            if (!Directory.Exists(@"D:\CATLParmeter"))
            {
                Directory.CreateDirectory(@"D:\CATLParmeter");
            }
        }
        #endregion 

        #region 配置文件加载

        private void ConfigInit()
        {
            int i;

            #region 系统配置加载
            CommonTags.PLCIP = IniHelper.ReadIniData("PLC_Set", "PLCIP ", "", CommonTags.SystemPath);
    
            #endregion

            #region 报警描述加载
            for (i = 0; i < 528; i++)
            {
                CommonTags.AlarmInformation[i] = IniHelper.ReadIniData("Alarm_Set", i.ToString(), "", CommonTags.AarmDescribePath);
                CommonTags.AlarmID[i] = IniHelper.ReadIniData("AlarmID_Set", i.ToString(), "", CommonTags.AarmIDPath);
            }
            #endregion

            #region 提示描述加载
            for (i = 0; i < 1304; i++)
            {
                if (i < 680)
                {
                    CommonTags.TipsInformation[i] = IniHelper.ReadIniData("Tips_Set", i.ToString(), "", CommonTags.TipsDescribe1Path);
                    CommonTags.TipsID[i] = IniHelper.ReadIniData("TipsID_Set", i.ToString(), "", CommonTags.TipsID1Path);
                }
                else
                {
                    CommonTags.TipsInformation[i] = IniHelper.ReadIniData("Tips_Set", (i - 680).ToString(), "", CommonTags.TipsDescribe2Path);
                    CommonTags.TipsID[i] = IniHelper.ReadIniData("TipsID_Set", (i - 680).ToString(), "", CommonTags.TipsID2Path);

                }
            }
            #endregion

            #region 归档参数名称加载
            for (i = 0; i < 100; i++)
            {
                CommonTags.CompleteMachine_PShow[i] = IniHelper.ReadIniData("整机参数", i.ToString(), "", CommonTags.ShowParamNamePath);
                CommonTags.Unwind_PShow[i] = IniHelper.ReadIniData("放卷参数", i.ToString(), "", CommonTags.ShowParamNamePath);
                CommonTags.MachineHeadA_PShow[i] = IniHelper.ReadIniData("机头A参数", i.ToString(), "", CommonTags.ShowParamNamePath);
                CommonTags.OvenA_PShow[i] = IniHelper.ReadIniData("烘箱A参数", i.ToString(), "", CommonTags.ShowParamNamePath);
                CommonTags.DragA_PShow[i] = IniHelper.ReadIniData("牵引A参数", i.ToString(), "", CommonTags.ShowParamNamePath);
                CommonTags.MachineHeadB_PShow[i] = IniHelper.ReadIniData("机头B参数", i.ToString(), "", CommonTags.ShowParamNamePath);
                CommonTags.OvenB_PShow[i] = IniHelper.ReadIniData("烘箱B参数", i.ToString(), "", CommonTags.ShowParamNamePath);
                CommonTags.DragB_PShow[i] = IniHelper.ReadIniData("牵引B参数", i.ToString(), "", CommonTags.ShowParamNamePath);
                CommonTags.BakingOven_PShow[i] = IniHelper.ReadIniData("baking烘箱参数", i.ToString(), "", CommonTags.ShowParamNamePath);
                CommonTags.BakingDrag_PShow[i] = IniHelper.ReadIniData("baking牵引参数", i.ToString(), "", CommonTags.ShowParamNamePath);
                CommonTags.DragD_PShow[i] = IniHelper.ReadIniData("牵引D参数", i.ToString(), "", CommonTags.ShowParamNamePath);
                CommonTags.Wind_PShow[i] = IniHelper.ReadIniData("收卷参数", i.ToString(), "", CommonTags.ShowParamNamePath);
                CommonTags.InfraredOvenA_PShow[i] = IniHelper.ReadIniData("红外烘箱A参数", i.ToString(), "", CommonTags.ShowParamNamePath);
                CommonTags.InfraredOvenB_PShow[i] = IniHelper.ReadIniData("红外烘箱B参数", i.ToString(), "", CommonTags.ShowParamNamePath);

                CommonTags.CompleteMachine_PSet[i] = IniHelper.ReadIniData("整机参数", i.ToString(), "", CommonTags.SetParamNamePath);
                CommonTags.Unwind_PSet[i] = IniHelper.ReadIniData("放卷参数", i.ToString(), "", CommonTags.SetParamNamePath);
                CommonTags.MachineHeadA_PSet[i] = IniHelper.ReadIniData("机头A参数", i.ToString(), "", CommonTags.SetParamNamePath);
                CommonTags.OvenA_PSet[i] = IniHelper.ReadIniData("烘箱A参数", i.ToString(), "", CommonTags.SetParamNamePath);
                CommonTags.DragA_PSet[i] = IniHelper.ReadIniData("牵引A参数", i.ToString(), "", CommonTags.SetParamNamePath);
                CommonTags.MachineHeadB_PSet[i] = IniHelper.ReadIniData("机头B参数", i.ToString(), "", CommonTags.SetParamNamePath);
                CommonTags.OvenB_PSet[i] = IniHelper.ReadIniData("烘箱B参数", i.ToString(), "", CommonTags.SetParamNamePath);
                CommonTags.DragB_PSet[i] = IniHelper.ReadIniData("牵引B参数", i.ToString(), "", CommonTags.SetParamNamePath);
                CommonTags.BakingOven_PSet[i] = IniHelper.ReadIniData("baking烘箱参数", i.ToString(), "", CommonTags.SetParamNamePath);
                CommonTags.BakingDrag_PSet[i] = IniHelper.ReadIniData("baking牵引参数", i.ToString(), "", CommonTags.SetParamNamePath);
                CommonTags.DragD_PSet[i] = IniHelper.ReadIniData("牵引D参数", i.ToString(), "", CommonTags.SetParamNamePath);
                CommonTags.Wind_PSet[i] = IniHelper.ReadIniData("收卷参数", i.ToString(), "", CommonTags.SetParamNamePath);
                CommonTags.InfraredOvenA_PSet[i] = IniHelper.ReadIniData("红外烘箱A参数", i.ToString(), "", CommonTags.SetParamNamePath);
                CommonTags.InfraredOvenB_PSet[i] = IniHelper.ReadIniData("红外烘箱B参数", i.ToString(), "", CommonTags.SetParamNamePath);
            }

            for (i = 0; i < 325; i++)
            {
                CommonTags.DeviceParameterName[i] = IniHelper.ReadIniData("采集参数", i.ToString(), "", CommonTags.DeviceParamNamePath);
            }
            #endregion

            #region 配方参数名称加载
            for (i = 0; i < 215; i++)
            {
                CommonTags.FormulaParam[i] = IniHelper.ReadIniData("配方参数", i.ToString(), "", CommonTags.FormulaParamNamePath);
            }
            #endregion 

            #region 过辊速度名称加载
            for (i = 0; i <168; i++)
            {
                CommonTags.OverRollName[i] = IniHelper.ReadIniData("整机参数", i.ToString(), "", CommonTags.OverRollNamePath);
            }
            #endregion

            #region 用户信息加载
            string ConfigPath = Application.StartupPath + "\\HNSet\\User.ini";
            for (int j = 0; j < 5; j++)
            {
                CommonTags.AdminName[j] = INIOperationClass.INIGetStringValue(ConfigPath, "AdminName_Set", j.ToString(), null);
                CommonTags.AdminPass[j] = INIOperationClass.INIGetStringValue(ConfigPath, "AdminPass_Set", j.ToString(), null);
            }
            #endregion 


        }

        #endregion

        #region 界面加载
        public void InterfaceLoad()
        {
            frmOperator.TopLevel = false;
            frmOperator.FormBorderStyle = FormBorderStyle.None;
            frmOperator.Dock = DockStyle.Fill;
            frmOperator.Parent = this.panel3;
            frmOperator.Show();

            frmCurvecsChoice.TopLevel = false;
            frmCurvecsChoice.FormBorderStyle = FormBorderStyle.None;
            frmCurvecsChoice.Dock = DockStyle.Fill;
            frmCurvecsChoice.Parent = this.panel3;
            frmCurvecsChoice.Show();

            frmCurvecsChoice1.TopLevel = false;
            frmCurvecsChoice1.FormBorderStyle = FormBorderStyle.None;
            frmCurvecsChoice1.Dock = DockStyle.Fill;
            frmCurvecsChoice1.Parent = this.panel3;
            frmCurvecsChoice1.Show();

            frmReport.TopLevel = false;
            frmReport.FormBorderStyle = FormBorderStyle.None;
            frmReport.Dock = DockStyle.Fill;
            frmReport.Parent = this.panel3;
            frmReport.Show();
            frmReport.Visible = false;

           frmSet.TopLevel = false;
           frmSet.FormBorderStyle = FormBorderStyle.None;
           frmSet.Dock = DockStyle.Fill;
           frmSet.Parent = this.panel3;
           frmSet.Show();
           frmSet.Visible = false;


        }


        #endregion    

        #region 线程初始化
        private void ThreadInit()
        {
            UIUpdate = new Thread(new System.Threading.ThreadStart(UIUpdateThreadFunction));
            UIUpdate.IsBackground = true;
            UIUpdate.Priority = ThreadPriority.AboveNormal;
            UIUpdate.Start();

            PLCTagRead = new Thread(new System.Threading.ThreadStart(PLCTagReadFunction));
            PLCTagRead.IsBackground = true;
            PLCTagRead.Priority = ThreadPriority.AboveNormal;
            PLCTagRead.Start();

            AlarmMonitor = new Thread(new System.Threading.ThreadStart(AlarmMonitorFunction));
            AlarmMonitor.IsBackground = true;
            AlarmMonitor.Priority = ThreadPriority.AboveNormal;

            ParameterSave = new Thread(new System.Threading.ThreadStart(ParameterSaveFunction));
            ParameterSave.IsBackground = true;
            ParameterSave.Priority = ThreadPriority.AboveNormal;

            CurvecsParamete = new Thread(new System.Threading.ThreadStart(CurvecsParameteFunction));
            CurvecsParamete.IsBackground = true;
            CurvecsParamete.Priority = ThreadPriority.AboveNormal;

            CurvecsParamete1 = new Thread(new System.Threading.ThreadStart(CurvecsParameteFunction1));
            CurvecsParamete1.IsBackground = true;
            CurvecsParamete1.Priority = ThreadPriority.AboveNormal;

            CurvecsParameteSave = new Thread(new System.Threading.ThreadStart(CurvecsParameteSaveFunction));
            CurvecsParameteSave.IsBackground = true;
            CurvecsParameteSave.Priority = ThreadPriority.AboveNormal;

            CurvecsParameteSave1 = new Thread(new System.Threading.ThreadStart(CurvecsParameteSaveFunction1));
            CurvecsParameteSave1.IsBackground = true;
            CurvecsParameteSave1.Priority = ThreadPriority.AboveNormal;

        }
        #endregion

        #region 线程函数

        #region 界面更新函数

        private void UIUpdateThreadFunction()
        {
            string date1 = DateTime.Now.ToString("yyyy-MM-dd");
            string time1 = DateTime.Now.ToString("HH");
            bool Heart_F = false;
            while (true)
            {
                #region PLC连接状态显示
                if (CommonTags.PLCOnline ==0)
                {
                    this.BeginInvoke((EventHandler)delegate
                    {
                        label8.BackColor = Color.LightGreen;
                    });
                }
                if (CommonTags.PLCOnline >5 )
                {
                    this.BeginInvoke((EventHandler)delegate
                    {
                        label8.BackColor = Color.Red;
                    });
                }
                #endregion

                #region 线程开启
                if (!PLCFirstOnline && CommonTags.PLCOnline == 0)
                {
                   
                    PLCFirstOnline = true;
                }
                #endregion

                #region PLC心跳
                if (Heart_F)
                {
                    PLCReadWrite.PLC1Connect.Write("DB308.0", (float)1);
                    Heart_F = false;
                }
                else
                {
                    PLCReadWrite.PLC1Connect.Write("DB308.0", (float)0);
                    Heart_F = true;
                }
                #endregion 

                #region 数据库数据定期删除

                string date2 = DateTime.Now.ToString("yyyy-MM-dd");
                if (date1 != date2)
                {
                    SQLite.CurvecsDelate(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd HH:mm:ss"));
                   
                    SQLite.AlarmStopDelate(DateTime.Now.AddDays(-60).ToString("yyyy-MM-dd HH:mm:ss"));

                    SQLite.AlarmTipDelate(DateTime.Now.AddDays(-60).ToString("yyyy-MM-dd HH:mm:ss"));

                    SQLite.ParameterDelate(DateTime.Now.AddDays(-60).ToString("yyyy-MM-dd HH:mm:ss"));

                    date1 = date2;
                }
                #endregion

                #region 定期导出数据到CSV

                string time2 = DateTime.Now.ToString("HH");
                if (time1 != time2)
                {

                    if (time1 == "11")
                    {
                        commonMean.DataGridViewToCSV1("D:\\CATLParmeter\\" + DateTime.Now.ToString("yyyyMMdd") + "12_参数.CSV");
                        commonMean.DataGridViewToCSV2("D:\\CATLParmeter\\" + DateTime.Now.ToString("yyyyMMdd") + "12_过辊速度.CSV");
                    }

                    if (time1 == "23")
                    {
                        commonMean.DataGridViewToCSV1("D:\\CATLParmeter\\" + DateTime.Now.AddDays(-1).ToString("yyyyMMdd") + "24_参数.CSV");
                        commonMean.DataGridViewToCSV2("D:\\CATLParmeter\\" + DateTime.Now.AddDays(-1).ToString("yyyyMMdd") + "24_过辊速度.CSV");
                    }
                    time1 = time2;
                }
                #endregion

                Thread.Sleep(1500);
            }
        }
        #endregion

        #region PLC数据读取
        private void PLCTagReadFunction()
        {
            HslCommunication.OperateResult<byte[]> readbool1;
            HslCommunication.OperateResult<byte[]> readbool2;
            HslCommunication.OperateResult<float[]> readfloat1;
            HslCommunication.OperateResult<float[]> readfloat2;
            HslCommunication.OperateResult<float[]> readfloat3;
            HslCommunication.OperateResult<float[]> readfloat4;
            HslCommunication.OperateResult<float[]> readfloat5;

            HslCommunication.OperateResult<string> FormulaName;

            int Count1 = 0;
            while (true)
            {               
                switch (Count1)//分时区访问PLC
                {
                    #region 停机报警读取
                    case 1:
                        readbool1 = PLCReadWrite.PLC1Connect.Read("M1000.0", (ushort)66);
                        if (readbool1.IsSuccess)
                        {
                            CommonTags.PLC1_BOOL = commonMean.GetBool(readbool1, 0, 66);
                        }
                        break;

                    case 11:
                        readbool1 = PLCReadWrite.PLC1Connect.Read("M1000.0", (ushort)66);
                        if (readbool1.IsSuccess)
                        {
                            CommonTags.PLC1_BOOL = commonMean.GetBool(readbool1, 0, 66);
                        }
                        break;
                    #endregion

                    #region 提示报警读取
                    case 3:
                        readbool2 = PLCReadWrite.PLC1Connect.Read("M1074.0", (ushort)163);
                        if (readbool2.IsSuccess)
                        {
                            CommonTags.PLC1_BOOL1 = commonMean.GetBool(readbool2, 0, 163);
                        }
                        break;

                    #endregion

                    #region 过辊速度读取
                    case 7:
                        readfloat4 = PLCReadWrite.PLC1Connect.ReadFloat("DB96.0", 84);
                        if (readfloat4.IsSuccess)
                        {
                            CommonTags.PLC1_REAL4 = readfloat4.Content;
                            CommonTags.PLCOnline = 0;
                        }
                        else
                        {
                            CommonTags.PLCOnline++;
                        }
                        break;
                    case 17:
                        readfloat5 = PLCReadWrite.PLC1Connect.ReadFloat("DB96.672", 84);
                        if (readfloat5.IsSuccess)
                        {
                            CommonTags.PLC1_REAL5 = readfloat5.Content;
                            CommonTags.PLCOnline = 0;
                        }
                        else
                        {
                            CommonTags.PLCOnline++;
                        }
                        break;

                    #endregion

                    #region 设定参数读取

                    case 5:
                        readfloat2 = PLCReadWrite.PLC1Connect.ReadFloat("DB308.0", 566);
                        if (readfloat2.IsSuccess)
                        {
                            CommonTags.PLC1_REAL2 = readfloat2.Content;
                            CommonTags.PLCOnline = 0;
                        }
                        else
                        {
                            CommonTags.PLCOnline++;
                        }
                        break;

                    #endregion

                    #region 配方参数读取

                    case 18:
                        readfloat3 = PLCReadWrite.PLC1Connect.ReadFloat("DB309.1892", 161);
                        if (readfloat3.IsSuccess)
                        {
                            CommonTags.PLC1_REAL1 = readfloat3.Content;
                            CommonTags.PLCOnline = 0;
                        }
                        else
                        {
                            CommonTags.PLCOnline++;
                        }
                        break;

                    #endregion

                    #region 配方名称读取

                    case 9:
                        FormulaName = PLCReadWrite.PLC1Connect.ReadString("DB310.0", 20);
                        if (FormulaName.IsSuccess)
                        {
                            string SSS = String.Format(FormulaName.Content).Trim();
                            string AAA = SSS.Substring(2);
                            string[] imgAr = new string[100];
                            for (int k = 0; k < AAA.Length; k++)
                            {
                                imgAr[k] = AAA.Substring(k, 1);
                            }
                            string BBB = null;
                            for (int k = 0; k < AAA.Length; k++)
                            {
                                if (imgAr[k] == "\0")
                                {
                                    goto WriteFalt;
                                }
                                BBB = BBB + imgAr[k];
                            }
                            WriteFalt:;
                            CommonTags.PLC1_String = BBB;

                            int aaa = 1;
                        }

                        break;
                    #endregion 

                    default: break;
                }

                Count1++;        

                if (Count1 > 20)
                {
                    Count1 = 1;
                }
                Thread.Sleep(100);
            }

        }
        #endregion      

        #region 报警监控
        private void AlarmMonitorFunction()
        {
            bool[] Alarm1 = new bool[528];
            bool[] Alarm2 = new bool[528];
            bool[] Tips1 = new bool[1304];
            bool[] Tips2 = new bool[1304];
           
            Alarm1 = Alarm2 = CommonTags.PLC1_BOOL;
            Tips2 = CommonTags.PLC1_BOOL1;
            Tips1 = CommonTags.PLC1_BOOL1;

            while (true)
            {
                #region 停机报警
                Alarm1 = CommonTags.PLC1_BOOL;
              
                for (int i = 0; i < 528; i++)
                {
                    if (Alarm2[i] != Alarm1[i])
                    {
                        Alarm2[i] = Alarm1[i];
                        if (Alarm1[i])
                        {
                            SQLite.AlarmStopInsert(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), CommonTags.AlarmInformation[i], true, CommonTags.AlarmID[i]);
                        }
                        else
                        {
                            SQLite.AlarmStopInsert(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), CommonTags.AlarmInformation[i], false, CommonTags.AlarmID[i]);
                        }
                    }                  
                
                }
                #endregion

                #region 非停机报警
                Tips1 = CommonTags.PLC1_BOOL1;

                for (int K = 0; K < 1304; K++)
                {
                    if (Tips2[K] != Tips1[K])
                    {
                        Tips2[K] = Tips1[K];
                        if (Tips1[K])
                        {
                            SQLite.AlarmTipInsert(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), CommonTags.TipsInformation[K], true, CommonTags.TipsID[K]);
                        }
                        else
                        {
                            SQLite.AlarmTipInsert(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), CommonTags.TipsInformation[K], false, CommonTags.TipsID[K]);
                        }
                    }

                }
                #endregion

                Thread.Sleep(2000);
            }
        }
        #endregion

        #region 参数保存1分钟
        private void ParameterSaveFunction()
        { 
            while (true)
            {
                #region  设备参数表1
                StringBuilder sb = new StringBuilder();
                sb.Append("Insert into table1(CellectTime,Operator,DeviceCode");

                for (int i = 0; i < 310; i++)
                {
                    sb.Append(",Parameter");
                    sb.Append((i + 1).ToString());
                }

                sb.Append(") values(");

                sb.Append("'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" + "," + "'" + "test" + "'" + "," + "'" + "HNTB" + "'");

             
                #region 采集参数值
                sb.Append("," + "'" + CommonTags.PLC1_REAL[12] + "'");//放卷张力显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[148] + "'");//牵引A张力显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[162] + "'");//机头B张力显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[276] + "'");//牵引B张力显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[1] + "'");//牵引C张力显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[370] + "'");//牵引D张力显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[384] + "'");//收卷张力显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[11] + "'");//放卷张力设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[206] + "'");//牵引A张力设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[222] + "'");//机头B张力设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[394] + "'");//牵引B张力设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[1] + "'");//牵引C张力设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[487] + "'");//牵引D张力设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[503] + "'");//收卷张力设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[11] + "'");//放卷摆辊显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[147] + "'");//牵引A摆辊显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[161] + "'");//机头B摆辊显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[275] + "'");//牵引B摆辊显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[1] + "'");//牵引C摆辊显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[369] + "'");//牵引D摆辊显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[383] + "'");//收卷摆辊显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[81] + "'");//烘箱A鼓风频率A1显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[82] + "'");//鼓风频率A2显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[83] + "'");//鼓风频率A3显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[84] + "'");//鼓风频率A4显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[85] + "'");//鼓风频率A5显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[86] + "'");//鼓风频率A6显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[87] + "'");//鼓风频率A7显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[88] + "'");//鼓风频率A8显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[89] + "'");//鼓风频率A9显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[90] + "'");//鼓风频率A10显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[91] + "'");//鼓风频率A11显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[92] + "'");//鼓风频率A12显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[111] + "'");//鼓风频率A1设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[112] + "'");//鼓风频率A2设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[113] + "'");//鼓风频率A3设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[114] + "'");//鼓风频率A4设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[115] + "'");//鼓风频率A5设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[116] + "'");//鼓风频率A6设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[117] + "'");//鼓风频率A7设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[118] + "'");//鼓风频率A8设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[119] + "'");//鼓风频率A9设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[120] + "'");//鼓风频率A10设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[121] + "'");//鼓风频率A11设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[122] + "'");//鼓风频率A12设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[211] + "'");//鼓风频率B1显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[212] + "'");//鼓风频率B2显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[213] + "'");//鼓风频率B3显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[214] + "'");//鼓风频率B4显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[215] + "'");//鼓风频率B5显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[216] + "'");//鼓风频率B6显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[217] + "'");//鼓风频率B7显示
             
                
                sb.Append("," + "'" + CommonTags.PLC1_REAL[218] + "'");//鼓风频率B8显示
                
                sb.Append("," + "'" + CommonTags.PLC1_REAL[219] + "'");//鼓风频率B9显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[220] + "'");//鼓风频率B10显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[221] + "'");//鼓风频率B11显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[222] + "'");//鼓风频率B12显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[301] + "'");//鼓风频率B1设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[302] + "'");//鼓风频率B2设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[303] + "'");//鼓风频率B3设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[304] + "'");//鼓风频率B4设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[305] + "'");//鼓风频率B5设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[306] + "'");//鼓风频率B6设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[307] + "'");//鼓风频率B7设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[308] + "'");//鼓风频率B8设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[309] + "'");//鼓风频率B9设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[310] + "'");//鼓风频率B10设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[311] + "'");//鼓风频率B11设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[312] + "'");//鼓风频率B12设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[289] + "'");//鼓风频率Baking1显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[290] + "'");//鼓风频率Baking2显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[291] + "'");//鼓风频率Baking3显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[292] + "'");//鼓风频率Baking4显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[293] + "'");//鼓风频率Baking5显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[294] + "'");//鼓风频率Baking6显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[295] + "'");//鼓风频率Baking7显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[296] + "'");//鼓风频率Baking8显示
                
                sb.Append("," + "'" + CommonTags.PLC1_REAL[297] + "'");//鼓风频率Baking9显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[298] + "'");//鼓风频率Baking10显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[299] + "'");//鼓风频率Baking11显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[300] + "'");//鼓风频率Baking12显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[410] + "'");//鼓风频率Baking1设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[411] + "'");//鼓风频率Baking2设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[412] + "'");//鼓风频率Baking3设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[413] + "'");//鼓风频率Baking4设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[414] + "'");//鼓风频率Baking5设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[415] + "'");//鼓风频率Baking6设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[416] + "'");//鼓风频率Baking7设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[417] + "'");//鼓风频率Baking8设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[418] + "'");//鼓风频率Baking9设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[419] + "'");//鼓风频率Baking10设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[420] + "'");//鼓风频率Baking11设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[421] + "'");//鼓风频率Baking12设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[105] + "'");//频率抽风A1显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[106] + "'");//频率串联风机A1显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[107] + "'");//频率外部新风A1显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[135] + "'");//频率抽风A1设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[136] + "'");//频率串联风机A1设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[137] + "'");//频率外部新风A1设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[235] + "'");//频率抽风B1显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[236] + "'");//频率串联风机B1显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[237] + "'");//频率外部新风B1显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[325] + "'");//频率抽风B1设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[326] + "'");//频率串联风机B1设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[327] + "'");//频率外部新风B1设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[93] + "'");//温度A1显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[94] + "'");//温度A2显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[95] + "'");//温度A3显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[96] + "'");//温度A4显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[97] + "'");//温度A5显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[98] + "'");//温度A6显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[99] + "'");//温度A7显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[100] + "'");//温度A8显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[101] + "'");//温度A9显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[102] + "'");//温度A10显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[103] + "'");//温度A11显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[104] + "'");//温度A12显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[123] + "'");//温度A1设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[124] + "'");//温度A2设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[125] + "'");//温度A3设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[126] + "'");//温度A4设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[127] + "'");//温度A5设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[128] + "'");//温度A6设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[129] + "'");//温度A7设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[130] + "'");//温度A8设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[131] + "'");//温度A9设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[132] + "'");//温度A10设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[133] + "'");//温度A11设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[134] + "'");//温度A12设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[223] + "'");//温度B1显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[224] + "'");//温度B2显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[225] + "'");//温度B3显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[226] + "'");//温度B4显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[227] + "'");//温度B5显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[228] + "'");//温度B6显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[229] + "'");//温度B7显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[230] + "'");//温度B8显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[231] + "'");//温度B9显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[232] + "'");//温度B10显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[233] + "'");//温度B11显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[234] + "'");//温度B12显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[313] + "'");//温度B1设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[314] + "'");//温度B2设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[315] + "'");//温度B3设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[316] + "'");//温度B4设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[317] + "'");//温度B5设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[318] + "'");//温度B6设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[319] + "'");//温度B7设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[320] + "'");//温度B8设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[321] + "'");//温度B9设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[322] + "'");//温度B10设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[323] + "'");//温度B11设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[324] + "'");//温度B12设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[301] + "'");//温度Baking1显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[302] + "'");//温度Baking2显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[303] + "'");//温度Baking3显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[304] + "'");//温度Baking4显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[305] + "'");//温度Baking5显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[306] + "'");//温度Baking6显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[307] + "'");//温度Baking7显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[308] + "'");//温度Baking8显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[309] + "'");//温度Baking9显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[310] + "'");//温度Baking10显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[311] + "'");//温度Baking11显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[312] + "'");//温度Baking12显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[422] + "'");//温度Baking1设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[423] + "'");//温度Baking2设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[424] + "'");//温度Baking3设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[425] + "'");//温度Baking4设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[426] + "'");//温度Baking5设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[427] + "'");//温度Baking6设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[428] + "'");//温度Baking7设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[429] + "'");//温度Baking8设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[430] + "'");//温度Baking9设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[431] + "'");//温度Baking10设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[432] + "'");//温度Baking11设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[433] + "'");//温度Baking12设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[405] + "'");//温度红外A1显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[406] + "'");//温度红外A2显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[407] + "'");//温度红外A3显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[408] + "'");//温度红外A4显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[409] + "'");//温度红外A5显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[410] + "'");//温度红外A6显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[411] + "'");//温度红外A7显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[412] + "'");//温度红外A8显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[413] + "'");//温度红外A9显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[414] + "'");//温度红外A10显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[415] + "'");//温度红外A11显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[416] + "'");//温度红外A12显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[522] + "'");//温度红外A1设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[523] + "'");//温度红外A2设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[524] + "'");//温度红外A3设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[525] + "'");//温度红外A4设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[526] + "'");//温度红外A5设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[527] + "'");//温度红外A6设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[528] + "'");//温度红外A7设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[529] + "'");//温度红外A8设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[530] + "'");//温度红外A9设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[531] + "'");//温度红外A10设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[532] + "'");//温度红外A11设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[533] + "'");//温度红外A12设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[439] + "'");//温度红外B1显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[440] + "'");//温度红外B2显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[441] + "'");//温度红外B3显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[442] + "'");//温度红外B4显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[443] + "'");//温度红外B5显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[444] + "'");//温度红外B6显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[445] + "'");//温度红外B7显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[446] + "'");//温度红外B8显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[447] + "'");//温度红外B9显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[448] + "'");//温度红外B10显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[449] + "'");//温度红外B11显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[450] + "'");//温度红外B12显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[544] + "'");//温度红外B1设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[545] + "'");//温度红外B2设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[546] + "'");//温度红外B3设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[547] + "'");//温度红外B4设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[548] + "'");//温度红外B5设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[549] + "'");//温度红外B6设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[550] + "'");//温度红外B7设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[551] + "'");//温度红外B8设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[552] + "'");//温度红外B9设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[553] + "'");//温度红外B10设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[554] + "'");//温度红外B11设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[555] + "'");//温度红外B12设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[1] + "'");//运行速度显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[28] + "'");//运行速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[29] + "'");//倒带速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[30] + "'");//引带速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[34] + "'");//机头调刀速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[35] + "'");//机头A上供料泵1泵速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[1] + "'");//机头A上供料泵2泵速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[36] + "'");//机头A下供料泵1泵速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[1] + "'");//机头A下供料泵2泵速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[37] + "'");//机头A_AT供料泵1泵速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[38] + "'");//机头A_AT供料泵2泵速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[39] + "'");//机头A_AT供料泵3泵速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[40] + "'");//机头A_AT供料泵4泵速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[38] + "'");//机头点胶速度显示1
                sb.Append("," + "'" + CommonTags.PLC1_REAL[39] + "'");//机头点胶速度显示2
                sb.Append("," + "'" + CommonTags.PLC1_REAL[40] + "'");//机头点胶速度显示3
                sb.Append("," + "'" + CommonTags.PLC1_REAL[41] + "'");//机头点胶速度显示4
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[229] + "'");//机尾调刀速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[231] + "'");//机尾A上供料泵1泵速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[1] + "'");//机尾A上供料泵2泵速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[232] + "'");//机尾A下供料泵1泵速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[1] + "'");//机尾A下供料泵2泵速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[233] + "'");//机尾A_AT供料泵1泵速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[234] + "'");//机尾A_AT供料泵2泵速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[235] + "'");//机尾A_AT供料泵3泵速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[236] + "'");//机尾A_AT供料泵4泵速度设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[168] + "'");//机尾点胶速度显示1
                sb.Append("," + "'" + CommonTags.PLC1_REAL[169] + "'");//机尾点胶速度显示2
                sb.Append("," + "'" + CommonTags.PLC1_REAL[170] + "'");//机尾点胶速度显示3
                sb.Append("," + "'" + CommonTags.PLC1_REAL[171] + "'");//机尾点胶速度显示4
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[32] + "'");//机头A左刀GAP设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[33] + "'");//机头A右刀GAP设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[227] + "'");//机尾B左刀GAP设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL2[228] + "'");//机尾B右刀GAP设定
                sb.Append("," + "'" + CommonTags.PLC1_REAL[34] + "'");//机头A左刀GAP显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[35] + "'");//机头A右刀GAP显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[164] + "'");//机尾B左刀GAP显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[165] + "'");//机尾B右刀GAP显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[19] + "'");//放卷A轴电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[20] + "'");//放卷B轴电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[149] + "'");//牵引A轴电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[277] + "'");//牵引B轴电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[371] + "'");//牵引D轴电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[391] + "'");//收卷A轴电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[392] + "'");//收卷B轴电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[55] + "'");//机头A供料1电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[56] + "'");//机头A供料2电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[57] + "'");//机头A供料3电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[58] + "'");//机头A供料4电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[59] + "'");//机头A点胶1电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[60] + "'");//机头A点胶2电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[61] + "'");//机头A点胶3电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[62] + "'");//机头A点胶4电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[185] + "'");//机头B供料1电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[186] + "'");//机头B供料2电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[187] + "'");//机头B供料3电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[188] + "'");//机头B供料4电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[189] + "'");//机头B点胶1电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[190] + "'");//机头B点胶2电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[191] + "'");//机头B点胶3电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[192] + "'");//机头B点胶4电流
                sb.Append("," + "'" + CommonTags.PLC1_REAL[108] + "'");//烘箱A浓度A1
                sb.Append("," + "'" + CommonTags.PLC1_REAL[109] + "'");//烘箱A浓度A2
                sb.Append("," + "'" + CommonTags.PLC1_REAL[110] + "'");//烘箱A浓度A3
                sb.Append("," + "'" + CommonTags.PLC1_REAL[111] + "'");//烘箱A浓度A4
                sb.Append("," + "'" + CommonTags.PLC1_REAL[112] + "'");//烘箱A浓度A5
                sb.Append("," + "'" + CommonTags.PLC1_REAL[113] + "'");//烘箱A浓度A6
                sb.Append("," + "'" + CommonTags.PLC1_REAL[114] + "'");//烘箱A浓度A7
                sb.Append("," + "'" + CommonTags.PLC1_REAL[115] + "'");//烘箱A浓度A8
                sb.Append("," + "'" + CommonTags.PLC1_REAL[116] + "'");//烘箱A浓度A9
                sb.Append("," + "'" + CommonTags.PLC1_REAL[117] + "'");//烘箱A浓度A10
                sb.Append("," + "'" + CommonTags.PLC1_REAL[118] + "'");//烘箱A浓度A11
                sb.Append("," + "'" + CommonTags.PLC1_REAL[119] + "'");//烘箱A浓度A12
                sb.Append("," + "'" + CommonTags.PLC1_REAL[238] + "'");//烘箱B浓度B1
                sb.Append("," + "'" + CommonTags.PLC1_REAL[239] + "'");//烘箱B浓度B2
                sb.Append("," + "'" + CommonTags.PLC1_REAL[240] + "'");//烘箱B浓度B3
                sb.Append("," + "'" + CommonTags.PLC1_REAL[241] + "'");//烘箱B浓度B4
                sb.Append("," + "'" + CommonTags.PLC1_REAL[242] + "'");//烘箱B浓度B5
                sb.Append("," + "'" + CommonTags.PLC1_REAL[243] + "'");//烘箱B浓度B6
                sb.Append("," + "'" + CommonTags.PLC1_REAL[244] + "'");//烘箱B浓度B7
                sb.Append("," + "'" + CommonTags.PLC1_REAL[245] + "'");//烘箱B浓度B8
                sb.Append("," + "'" + CommonTags.PLC1_REAL[246] + "'");//烘箱B浓度B9
                sb.Append("," + "'" + CommonTags.PLC1_REAL[247] + "'");//烘箱B浓度B10
                sb.Append("," + "'" + CommonTags.PLC1_REAL[248] + "'");//烘箱B浓度B11
                sb.Append("," + "'" + CommonTags.PLC1_REAL[249] + "'");//烘箱B浓度B12

                #endregion


                sb.Append(")");

                SQLite.ParameterInsert(sb.ToString());
                #endregion

                #region 过辊速度

                StringBuilder sb1 = new StringBuilder();
                sb1.Append("Insert into table1(CellectTime");

                for (int i = 0; i <168; i++)
                {
                    sb1.Append(",Parameter");
                    sb1.Append((i + 1).ToString());
                }

                sb1.Append(") values(");

                sb1.Append("'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" );

                #region 采集参数值

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[0] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[1] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[2] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[0] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[1] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[2] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[3] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[4] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[5] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[3] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[4] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[5] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[6] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[7] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[8] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[6] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[7] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[8] + "'");


                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[9] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[10] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[11] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[9] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[10] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[11] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[12] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[13] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[14] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[12] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[13] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[14] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[15] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[16] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[17] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[15] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[16] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[17] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[18] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[19] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[20] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[18] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[19] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[20] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[21] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[22] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[23] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[21] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[22] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[23] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[24] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[25] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[26] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[24] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[25] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[26] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[27] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[28] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[29] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[27] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[28] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[29] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[30] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[31] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[32] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[30] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[31] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[32] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[33] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[34] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[35] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[33] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[34] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[35] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[36] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[37] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[38] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[36] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[37] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[38] + "'");


                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[39] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[40] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[41] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[39] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[40] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[41] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[42] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[43] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[44] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[42] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[43] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[44] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[45] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[46] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[47] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[45] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[46] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[47] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[48] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[49] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[50] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[48] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[49] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[50] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[51] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[52] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[53] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[51] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[52] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[53] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[54] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[55] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[56] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[54] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[55] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[56] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[57] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[58] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[59] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[57] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[58] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[59] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[60] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[61] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[62] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[60] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[61] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[62] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[63] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[64] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[65] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[63] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[64] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[65] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[66] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[67] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[68] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[66] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[67] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[68] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[69] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[70] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[71] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[69] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[70] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[71] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[72] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[73] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[74] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[72] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[73] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[74] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[75] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[76] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[77] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[75] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[76] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[77] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[78] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[79] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[80] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[78] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[79] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[80] + "'");

                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[81] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[82] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL4[83] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[81] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[82] + "'");
                sb1.Append("," + "'" + CommonTags.PLC1_REAL5[83] + "'");

                #endregion

                sb1.Append(")");

                SQLite.OverRollInsert(sb1.ToString());

                #endregion 

                Thread.Sleep(60000);
            }
        }
        #endregion

        #region PLC实时参数高速读取400ms

        private void CurvecsParameteFunction()
        {
            HslCommunication.OperateResult<float[]> readfloat1;
            while (true)
            {

                readfloat1 = PLCReadWrite.PLC1Connect.ReadFloat("DB309.0", 473);
                if (readfloat1.IsSuccess)
                {
                    CommonTags.PLC1_REAL = readfloat1.Content;
                    CommonTags.PLCOnline = 0;
                }
                else
                {
                    CommonTags.PLCOnline++;
                }

                Thread.Sleep(400);
            }
        }
        #endregion

        #region 比例阀开度参数定时读取5s
        private void CurvecsParameteFunction1()
        {
            HslCommunication.OperateResult<float[]> readfloat1;
            while (true)
            {

                readfloat1 = PLCReadWrite.PLC1Connect.ReadFloat("DB80.64", 32);
                if (readfloat1.IsSuccess)
                {
                    CommonTags.PLC1_REAL6= readfloat1.Content;
                }
               

                Thread.Sleep(3000);
            }
        }
        #endregion

        #region  曲线参数存入
        private void CurvecsParameteSaveFunction()
        {           
            while (true)
            {
                #region  设备参数表1
                StringBuilder sb = new StringBuilder();
                sb.Append("Insert into table1(CollectTime");

                for (int i = 0; i < 20; i++)
                {
                    sb.Append(",Parameter");
                    sb.Append((i + 1).ToString());
                }

                sb.Append(") values(");

                sb.Append("'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ff") + "'");


                #region 采集参数值

                sb.Append("," + "'" + CommonTags.PLC1_REAL[12] + "'");//放卷张力显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[148] + "'");//牵引A张力显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[162] + "'");//机头B张力显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[276] + "'");//牵引B张力显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[1] + "'");//牵引C张力显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[370] + "'");//牵引D张力显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[384] + "'");//收卷张力显示

                sb.Append("," + "'" + CommonTags.PLC1_REAL[11] + "'");//放卷摆辊显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[147] + "'");//牵引A摆辊显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[161] + "'");//机头B摆辊显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[275] + "'");//牵引B摆辊显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[1] + "'");//牵引C摆辊显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[369] + "'");//牵引D摆辊显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[383] + "'");//收卷摆辊显示
                                                                       //

                sb.Append("," + "'" + CommonTags.PLC1_REAL[42] + "'");//机头A上层液位
                sb.Append("," + "'" + CommonTags.PLC1_REAL[43] + "'");//机头A下层液位
                sb.Append("," + "'" + CommonTags.PLC1_REAL[44] + "'");//机头A点胶液位

                sb.Append("," + "'" + CommonTags.PLC1_REAL[172] + "'");//机头B上层液位
                sb.Append("," + "'" + CommonTags.PLC1_REAL[173] + "'");//机头B下层液位
                sb.Append("," + "'" + CommonTags.PLC1_REAL[174] + "'");//机头B点胶液位




                #endregion


                sb.Append(")");

                SQLite.CurvecsInsert(sb.ToString());
                #endregion 


                Thread.Sleep(400);
            }
        }
        #endregion 

        #region  曲线参数存入1
        private void CurvecsParameteSaveFunction1()
        {
            while (true)
            {
                #region  设备参数表1
                StringBuilder sb = new StringBuilder();
                sb.Append("Insert into table1(CollectTime");

                for (int i = 0; i < 64; i++)
                {
                    sb.Append(",Parameter");
                    sb.Append((i + 1).ToString());
                }

                sb.Append(") values(");

                sb.Append("'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ff") + "'");

                #region 采集参数值

                sb.Append("," + "'" + CommonTags.PLC1_REAL6[0] + "'");//温控输出电流_A1
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[1] + "'");//温控输出电流_A2
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[2] + "'");//温控输出电流_A3
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[3] + "'");//温控输出电流_A4
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[4] + "'");//温控输出电流_A5
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[5] + "'");//温控输出电流_A6
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[6] + "'");//温控输出电流_A7
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[7] + "'");//温控输出电流_A8
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[8] + "'");//温控输出电流_A9
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[9] + "'");//温控输出电流_A10
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[10] + "'");//温控输出电流_A11
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[11] + "'");//温控输出电流_A12
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[12] + "'");//温控输出电流_A13
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[13] + "'");//温控输出电流_A14
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[14] + "'");//温控输出电流_A15
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[15] + "'");//温控输出电流_A16

                sb.Append("," + "'" + CommonTags.PLC1_REAL6[16] + "'");//温控输出电流_B1
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[17] + "'");//温控输出电流_B2
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[18] + "'");//温控输出电流_B3
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[19] + "'");//温控输出电流_B4
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[20] + "'");//温控输出电流_B5
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[21] + "'");//温控输出电流_B6
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[22] + "'");//温控输出电流_B7
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[23] + "'");//温控输出电流_B8
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[24] + "'");//温控输出电流_B9
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[25] + "'");//温控输出电流_B10
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[26] + "'");//温控输出电流_B11
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[27] + "'");//温控输出电流_B12
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[28] + "'");//温控输出电流_B13
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[29] + "'");//温控输出电流_B14
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[30] + "'");//温控输出电流_B15
                sb.Append("," + "'" + CommonTags.PLC1_REAL6[31] + "'");//温控输出电流_B16


                sb.Append("," + "'" + CommonTags.PLC1_REAL[93] + "'");//温度A1显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[94] + "'");//温度A2显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[95] + "'");//温度A3显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[96] + "'");//温度A4显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[97] + "'");//温度A5显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[98] + "'");//温度A6显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[99] + "'");//温度A7显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[100] + "'");//温度A8显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[101] + "'");//温度A9显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[102] + "'");//温度A10显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[103] + "'");//温度A11显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[104] + "'");//温度A12显示

                sb.Append("," + "'" + (float)0.0 + "'");//备用
                sb.Append("," + "'" + (float)0.0 + "'");//备用
                sb.Append("," + "'" + (float)0.0 + "'");//备用
                sb.Append("," + "'" + (float)0.0 + "'");//备用

                sb.Append("," + "'" + CommonTags.PLC1_REAL[223] + "'");//温度B1显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[224] + "'");//温度B2显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[225] + "'");//温度B3显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[226] + "'");//温度B4显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[227] + "'");//温度B5显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[228] + "'");//温度B6显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[229] + "'");//温度B7显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[230] + "'");//温度B8显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[231] + "'");//温度B9显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[232] + "'");//温度B10显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[233] + "'");//温度B11显示
                sb.Append("," + "'" + CommonTags.PLC1_REAL[234] + "'");//温度B12显示

                sb.Append("," + "'" + (float)0.0 + "'");//备用
                sb.Append("," + "'" + (float)0.0 + "'");//备用
                sb.Append("," + "'" + (float)0.0 + "'");//备用
                sb.Append("," + "'" + (float)0.0 + "'");//备用


                #endregion


                sb.Append(")");

                SQLite.CurvecsInsert1(sb.ToString());
                #endregion 


                Thread.Sleep(3000);
            }
        }
        #endregion 

        #endregion

        #region 界面操作触发函数

        #region 配方界面
        private void button1_Click(object sender, EventArgs e)
        { 
            frmReport.Visible = false;
            frmCurvecsChoice.Visible = false;
            frmCurvecsChoice1.Visible = false;
            frmSet.Visible = false;    
            frmOperator.Visible = true;
            panel3.BringToFront();


            FrmNull frmNull = new FrmNull();
            if (!CloseWindow(frmNull))
            {
                OpenWindow(frmNull);
            }
        }
        #endregion

        #region 报表界面
        private void button2_Click(object sender, EventArgs e)
        {
            frmReport.Visible = true;
            frmOperator.Visible = false;
            frmCurvecsChoice1.Visible = false;
            frmCurvecsChoice.Visible = false;
            frmSet.Visible = false;
            panel3.BringToFront();
            FrmNull frmNull = new FrmNull();
            if (!CloseWindow(frmNull))
            {
                OpenWindow(frmNull);
            }
        }
        #endregion

        #region 实时报表画面

        private void button6_Click(object sender, EventArgs e)
        {
            panel4.BringToFront();
            FrmRTUReport frmRTUReport = new FrmRTUReport();

            if (!CloseWindow(frmRTUReport))
            {
                OpenWindow(frmRTUReport);
            }
        }
        #endregion 

        #region 实时曲线
        private void button4_Click(object sender, EventArgs e)
        {
            frmReport.Visible = false;
            frmOperator.Visible = false;
            frmCurvecsChoice1.Visible = true;
            frmCurvecsChoice.Visible = false;
            frmSet.Visible = false;
            panel3.BringToFront();
            FrmNull frmNull = new FrmNull();
            if (!CloseWindow(frmNull))
            {
                OpenWindow(frmNull);
            }
            //panel4.BringToFront();
            //FrmRTCurvecs frmRTCurvecs = new FrmRTCurvecs();

            //if (!CloseWindow(frmRTCurvecs))
            //{
            //    OpenWindow(frmRTCurvecs);
            //}
        }
        #endregion

        #region 历史曲线
        private void button5_Click(object sender, EventArgs e)
        {
            //panel4.BringToFront();
            //FrmHistoryCurvecs frmHistoryCurvecs = new FrmHistoryCurvecs();

            //if (!CloseWindow(frmHistoryCurvecs))
            //{
            //    OpenWindow(frmHistoryCurvecs);
            //}
            frmReport.Visible = false;
            frmOperator.Visible = false;
            frmCurvecsChoice1.Visible = false;
            frmSet.Visible = false;
            frmCurvecsChoice.Visible = true;
            panel3.BringToFront();
            FrmNull frmNull = new FrmNull();
            if (!CloseWindow(frmNull))
            {
                OpenWindow(frmNull);
            }



        }

        #endregion

        #region 系统参数
        private void button3_Click(object sender, EventArgs e)
        {
            if (CommonTags.LocalLoginName == "未登录")
            {
                MessageBox.Show("当前未登录，请先登录");
                return;
            }

            frmReport.Visible = false;
            frmOperator.Visible = false;
            frmCurvecsChoice1.Visible = false;
            frmSet.Visible = true;
            frmCurvecsChoice.Visible = false;
            panel3.BringToFront();
            FrmNull frmNull = new FrmNull();
            if (!CloseWindow(frmNull))
            {
                OpenWindow(frmNull);
            }

        }

        #endregion

        #region  登录按钮
        private void button7_Click(object sender, EventArgs e)
        {
            new System.Threading.Tasks.TaskFactory().StartNew(() =>
            {
                FrmLogin objLogin = new FrmLogin();
                objLogin.TopMost = true;
                DialogResult dr = objLogin.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    label6.Text = CommonTags.LocalLoginName;
                    this.LoginOutTime.Start();
                }

            });
        }

        #endregion

        #region 退出登录
        private void button8_Click(object sender, EventArgs e)
        {
            label6.Text = CommonTags.LocalLoginName = "未登录";
        }
        #endregion 

        #region 打开窗体通用方法

        private bool CloseWindow(Form Frm)
        {
            bool Res = false;
            foreach (Control ct in this.panel4.Controls)
            {

                if (ct is Form)
                {
                    Form frm = ct as Form;
                    if (frm.Name == Frm.Name)
                    {
                        Res = true;
                        break;
                    }
                    else
                    {

                        frm.Dispose();
                    }

                }

            }
            return Res;
        }

        private void OpenWindow(Form Frm)
        {
            Frm.TopMost = true;
            Frm.TopLevel = false;
            Frm.FormBorderStyle = FormBorderStyle.None;
            Frm.Dock = DockStyle.Fill;
            Frm.Parent = this.panel4;

            Frm.Show();
        }
        #endregion

        #region 系统关闭提醒

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CommonTags.LocalLoginName == "未登录")
            {
                MessageBox.Show("当前未登录，请先登录");
                e.Cancel = true;
            }
            else
            {
                DialogResult result = MessageBox.Show("确认退出吗？", "操作提示",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (result != DialogResult.OK)
                {
                    e.Cancel = true;
                }
            }
        }
        #endregion

        #endregion

        #region 订阅事件

        public void SubscribeEvent1()
        {
            panel4.BringToFront();
            FrmHistoryCurvecs frmHistoryCurvecs = new FrmHistoryCurvecs();

            if (!CloseWindow(frmHistoryCurvecs))
            {
                OpenWindow(frmHistoryCurvecs);
            }

        }
        public void SubscribeEvent2()
        {
            panel4.BringToFront();
            FrmHistoryCurvecs1 frmHistoryCurvecs1 = new FrmHistoryCurvecs1();

            if (!CloseWindow(frmHistoryCurvecs1))
            {
                OpenWindow(frmHistoryCurvecs1);
            }

        }

        public void SubscribeEvent3()
        {
            panel4.BringToFront();
            FrmHistoryCurvecs2 frmHistoryCurvecs2 = new FrmHistoryCurvecs2();

            if (!CloseWindow(frmHistoryCurvecs2))
            {
                OpenWindow(frmHistoryCurvecs2);
            }
        }

        public void SubscribeEvent4()
        {
            panel4.BringToFront();
            FrmRTCurvecs frmRTCurvecs = new FrmRTCurvecs();

            if (!CloseWindow(frmRTCurvecs))
            {
                OpenWindow(frmRTCurvecs);
            }
        }
        public void SubscribeEvent5()
        {
            panel4.BringToFront();
            FrmRTCurvecs1 frmRTCurvecs1 = new FrmRTCurvecs1();

            if (!CloseWindow(frmRTCurvecs1))
            {
                OpenWindow(frmRTCurvecs1);
            }

        }
        public void SubscribeEvent6()
        {
            panel4.BringToFront();
            FrmRTCurvecs2 frmRTCurvecs2 = new FrmRTCurvecs2();

            if (!CloseWindow(frmRTCurvecs2))
            {
                OpenWindow(frmRTCurvecs2);
            }

        }

        public void SubscribeEvent11()
        {
            panel4.BringToFront();
            FrmRTCurvecs3 frmRTCurvecs3 = new FrmRTCurvecs3();

            if (!CloseWindow(frmRTCurvecs3))
            {
                OpenWindow(frmRTCurvecs3);
            }

        }

        public void SubscribeEvent12()
        {
            panel4.BringToFront();
            FrmRTCurvecs4 frmRTCurvecs4 = new FrmRTCurvecs4();

            if (!CloseWindow(frmRTCurvecs4))
            {
                OpenWindow(frmRTCurvecs4);
            }

        }

        public void SubscribeEvent13()
        {
            panel4.BringToFront();
            FrmRTCurvecs5 frmRTCurvecs5 = new FrmRTCurvecs5();

            if (!CloseWindow(frmRTCurvecs5))
            {
                OpenWindow(frmRTCurvecs5);
            }

        }

        public void SubscribeEvent14()
        {
            panel4.BringToFront();
            FrmRTCurvecs6 frmRTCurvecs6 = new FrmRTCurvecs6();

            if (!CloseWindow(frmRTCurvecs6))
            {
                OpenWindow(frmRTCurvecs6);
            }

        }

        public void SubscribeEvent7()
        {
            panel4.BringToFront();
            FrmHistoryCurvecs3 frmHistoryCurvecs3 = new FrmHistoryCurvecs3();

            if (!CloseWindow(frmHistoryCurvecs3))
            {
                OpenWindow(frmHistoryCurvecs3);
            }
        }

        public void SubscribeEvent8()
        {
            panel4.BringToFront();
            FrmHistoryCurvecs4 frmHistoryCurvecs4 = new FrmHistoryCurvecs4();

            if (!CloseWindow(frmHistoryCurvecs4))
            {
                OpenWindow(frmHistoryCurvecs4);
            }
        }


        public void SubscribeEvent9()
        {
            panel4.BringToFront();
            FrmHistoryCurvecs5 frmHistoryCurvecs5 = new FrmHistoryCurvecs5();

            if (!CloseWindow(frmHistoryCurvecs5))
            {
                OpenWindow(frmHistoryCurvecs5);
            }
        }

        public void SubscribeEvent10()
        {
            panel4.BringToFront();
            FrmHistoryCurvecs6 frmHistoryCurvecs6 = new FrmHistoryCurvecs6();

            if (!CloseWindow(frmHistoryCurvecs6))
            {
                OpenWindow(frmHistoryCurvecs6);
            }
        }

        private void Listener_ScanerEvent(ScanerHook.ScanerCodes codes)
        {
   
   
            for (int i = 0; i < 5; i++)
            {
                if (codes.Result == CommonTags.AdminName[i])
                {

                    CommonTags.LocalLoginName=label6.Text = codes.Result;
                    //SendKeys.Flush;
                    this.LoginOutTime.Start();

                  

                    break;
                }

            }
        }

        #endregion

        #region 定时函数

        private void LoginOutTime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            label6.Text = CommonTags.LocalLoginName = "未登录";
            this.LoginOutTime.Stop();
        }

        private void DelayStartTime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SQLite.SQLiteInit();
            ParameterSave.Start();
            AlarmMonitor.Start();
            CurvecsParamete.Start();
            CurvecsParamete1.Start();
            CurvecsParameteSave.Start();
            CurvecsParameteSave1.Start();

            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = true;

            DelayStart.Stop();
        }


        #endregion

      
    }
}
