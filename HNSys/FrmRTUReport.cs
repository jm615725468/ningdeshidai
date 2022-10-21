using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace HNSys
{
    public partial class FrmRTUReport : Form
    {

        private Thread UIUpdate = null;
        List<Alarm> alarmModel = new List<Alarm>();
        List<Alarm> alarmModel1 = new List<Alarm>();
        //字典集合,名称和描述
        private Dictionary<string, string> dicDeviceParm = new Dictionary<string, string>();
       
        private Dictionary<string, string> dicOverRoll = new Dictionary<string, string>();

        List<HNSys.Model.Parameter> parameter = new List<HNSys.Model.Parameter>();
        List<HNSys.Model.OverRoll> overRolls = new List<HNSys.Model.OverRoll>();

        CommonMean commonMean = new CommonMean();



        public FrmRTUReport()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;//使新创建的线程能访问UI线程创建的窗口控件
            Control.CheckForIllegalCrossThreadCalls = false;//允许非UI线程访问控件
        }
        private void FrmRTUReport_Load(object sender, EventArgs e)
        {
            DGVInit();
            InitialDeviceParmDataGridView();
            InitialDeviceOverRollataGridView();

            commonMean.SetDoubleBuffering(dgv, true);
            commonMean.SetDoubleBuffering(dataGridView1, true);
            commonMean.SetDoubleBuffering(dataGridView2, true);
            commonMean.SetDoubleBuffering(dataGridView3, true);



            ThreadInit();
        }


        #region 线程参数初始化
        private void ThreadInit()
        {
            UIUpdate = new Thread(new System.Threading.ThreadStart(UIUpdateThreadFunction));
            UIUpdate.IsBackground = true;
            UIUpdate.Priority = ThreadPriority.AboveNormal;
            UIUpdate.Start();

        
        }
        #endregion

        #region 界面刷新
        private void UIUpdateThreadFunction()
        {
            while (true)
            {
                #region 停机报警

                StringBuilder sb = new StringBuilder();
                sb.Append("select AlarmTime,AlarmInfo,AlarmState,AlarmID");
                sb.Append(" from Alarm order by ID desc limit 0,20");

                DataTable dt = SQLite.AlarmStopGetDataSet(sb.ToString()).Tables[0];

                if (dt.Rows.Count > 0)
                {
                    alarmModel.Clear();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Alarm alarm = new Alarm();
                        alarm.AlarmTime = (dt.Rows[i][0]).ToString();
                        alarm.AlarmInfo = (dt.Rows[i][1]).ToString();
                        alarm.AlarmState = (dt.Rows[i][2]).ToString();
                        alarm.AlarmID = (dt.Rows[i][3]).ToString();
                        alarmModel.Add(alarm);
                    }

                    ControlInvoker.Invoke(this, delegate
                    {
                        if (alarmModel != null && alarmModel.Count > 0)
                        {
                            dgv.VirtualMode = false;
                            dgv.AutoGenerateColumns = false;
                            dgv.DataSource = alarmModel;
                            dgv.ClearSelection();
                        }
                    });

                }
                #endregion 

                #region 提示报警

                StringBuilder sb1 = new StringBuilder();
                sb1.Append("select AlarmTime,AlarmInfo,AlarmState,AlarmID");
                sb1.Append(" from Alarm order by ID desc limit 0,20");

                DataTable dt1 = SQLite.AlarmTipGetDataSet(sb1.ToString()).Tables[0];

                if (dt1.Rows.Count > 0)
                {
                    alarmModel1.Clear();

                    for (int i = 0; i < dt1.Rows.Count; i++)
                    {
                        Alarm alarm1 = new Alarm();
                        alarm1.AlarmTime = (dt1.Rows[i][0]).ToString();
                        alarm1.AlarmInfo = (dt1.Rows[i][1]).ToString();
                        alarm1.AlarmState = (dt1.Rows[i][2]).ToString();
                        alarm1.AlarmID = (dt1.Rows[i][3]).ToString();
                        alarmModel1.Add(alarm1);
                    }

                    ControlInvoker.Invoke(this, delegate
                    {
                        if (alarmModel1 != null && alarmModel1.Count > 0)
                        {
                            dataGridView1.VirtualMode = false;
                            dataGridView1.AutoGenerateColumns = false;
                            dataGridView1.DataSource = alarmModel1;
                            dataGridView1.ClearSelection();
                        }
                    });

                }
                #endregion 

                ParameterReport();

                OverRollReport();

                Thread.Sleep(3000);

            }
        }
        #endregion

        #region DGV初始化
        public void DGVInit()
        {
            //设置DataGridView文本居中
           DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
          
            headerStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;

            dgv.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle = headerStyle;
            dgv.RowHeadersVisible = false;
            dgv.ClearSelection();
            dgv.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.None;

        
            //dgv.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //dgv.ColumnHeadersDefaultCellStyle = headerStyle;
            //dgv.RowHeadersVisible = false;
            //dgv.ClearSelection();
            dgv.ColumnCount = 4;


            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle = headerStyle;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.ClearSelection();
            dataGridView1.ColumnCount = 4;
        }


        #region 参数表初始化

        private void InitialDeviceParmDataGridView()
        {
            DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
            headerStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.ColumnHeadersDefaultCellStyle = headerStyle;
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.ClearSelection();

            this.dataGridView2.AutoGenerateColumns = false;

            dicDeviceParm.Clear();
            dataGridView2.Columns.Clear();

            //额外添加
            dicDeviceParm.Add("CellectTime", "采集时间");
            dicDeviceParm.Add("Operator", "员工账号");
            dicDeviceParm.Add("DeviceCode", "设备编号");



            for (int i = 0; i < 310; i++)
            {
                dicDeviceParm.Add("Parameter" + (i + 1).ToString(), CommonTags.DeviceParameterName[i]);
            }

            foreach (var item in dicDeviceParm.Keys)
            {
                DataGridViewTextBoxColumn dgvc = new DataGridViewTextBoxColumn();
                dgvc.HeaderText = dicDeviceParm[item];
                dgvc.ReadOnly = true;
                dgvc.Width = 200;
                dgvc.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                this.dataGridView2.Columns.Add(dgvc);
            }


            this.dataGridView2.Columns[0].Width = 250;
            dataGridView2.Columns[0].DataPropertyName = "CellectTime";
            dataGridView2.Columns[1].DataPropertyName = "Operator";
            dataGridView2.Columns[2].DataPropertyName = "DeviceCode";

            for (int i = 0; i < 310; i++)
            {
                dataGridView2.Columns[i + 3].DataPropertyName = "Parameter" + (i + 1).ToString();
            }

        }
        #endregion


        #region 过棍速度表初始化
        private void InitialDeviceOverRollataGridView()
        {
            DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
            headerStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridView3.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView3.ColumnHeadersDefaultCellStyle = headerStyle;
            dataGridView3.RowHeadersVisible = false;
            dataGridView3.ClearSelection();

            this.dataGridView3.AutoGenerateColumns = false;

            dicOverRoll.Clear();
            dataGridView3.Columns.Clear();

            //额外添加
           dicOverRoll.Add("CellectTime", "采集时间");



            for (int i = 0; i <168; i++)
            {
                dicOverRoll.Add("Parameter" + (i + 1).ToString(), CommonTags.OverRollName[i]);
            }

            foreach (var item in dicOverRoll.Keys)
            {
                DataGridViewTextBoxColumn dgvc = new DataGridViewTextBoxColumn();
                dgvc.HeaderText = dicOverRoll[item];
                dgvc.ReadOnly = true;
                dgvc.Width = 230;
                dgvc.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                this.dataGridView3.Columns.Add(dgvc);
            }


            this.dataGridView3.Columns[0].Width = 250;
            dataGridView3.Columns[0].DataPropertyName = "CellectTime";


            for (int i = 0; i < 168; i++)
            {
                dataGridView3.Columns[i + 1].DataPropertyName = "Parameter" + (i + 1).ToString();
            }





        }
        #endregion

        #endregion

        #region 参数表更新
        public void ParameterReport()
        {         
            StringBuilder sb = new StringBuilder();
            sb.Append("select CellectTime,Operator,DeviceCode");

            for (int i = 0; i < 312; i++)
            {
                sb.Append(",Parameter");
                sb.Append((i + 1).ToString());
            }

            sb.Append(" from table1 order by ID desc limit 0,15");
            try
            {
                DataTable dt = SQLite.ParameterGetDataSet(sb.ToString()).Tables[0];

                if (dt.Rows.Count > 0)
                {
                    parameter.Clear();


                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Model.Parameter parameterOne = new Model.Parameter();

                        #region 参数赋值
                        parameterOne.CellectTime = (dt.Rows[i][0]).ToString();
                        parameterOne.Operator = (dt.Rows[i][1]).ToString();
                        parameterOne.DeviceCode = (dt.Rows[i][2]).ToString();
                        parameterOne.Parameter1 = float.Parse(dt.Rows[i][3].ToString());
                        parameterOne.Parameter2 = float.Parse(dt.Rows[i][4].ToString());
                        parameterOne.Parameter3 = float.Parse(dt.Rows[i][5].ToString());
                        parameterOne.Parameter4 = float.Parse(dt.Rows[i][6].ToString());
                        parameterOne.Parameter5 = float.Parse(dt.Rows[i][7].ToString());
                        parameterOne.Parameter6 = float.Parse(dt.Rows[i][8].ToString());
                        parameterOne.Parameter7 = float.Parse(dt.Rows[i][9].ToString());
                        parameterOne.Parameter8 = float.Parse(dt.Rows[i][10].ToString());
                        parameterOne.Parameter9 = float.Parse(dt.Rows[i][11].ToString());
                        parameterOne.Parameter10 = float.Parse(dt.Rows[i][12].ToString());
                        parameterOne.Parameter11 = float.Parse(dt.Rows[i][13].ToString());
                        parameterOne.Parameter12 = float.Parse(dt.Rows[i][14].ToString());
                        parameterOne.Parameter13 = float.Parse(dt.Rows[i][15].ToString());
                        parameterOne.Parameter14 = float.Parse(dt.Rows[i][16].ToString());
                        parameterOne.Parameter15 = float.Parse(dt.Rows[i][17].ToString());
                        parameterOne.Parameter16 = float.Parse(dt.Rows[i][18].ToString());
                        parameterOne.Parameter17 = float.Parse(dt.Rows[i][19].ToString());
                        parameterOne.Parameter18 = float.Parse(dt.Rows[i][20].ToString());
                        parameterOne.Parameter19 = float.Parse(dt.Rows[i][21].ToString());
                        parameterOne.Parameter20 = float.Parse(dt.Rows[i][22].ToString());
                        parameterOne.Parameter21 = float.Parse(dt.Rows[i][23].ToString());
                        parameterOne.Parameter22 = float.Parse(dt.Rows[i][24].ToString());
                        parameterOne.Parameter23 = float.Parse(dt.Rows[i][25].ToString());
                        parameterOne.Parameter24 = float.Parse(dt.Rows[i][26].ToString());
                        parameterOne.Parameter25 = float.Parse(dt.Rows[i][27].ToString());
                        parameterOne.Parameter26 = float.Parse(dt.Rows[i][28].ToString());
                        parameterOne.Parameter27 = float.Parse(dt.Rows[i][29].ToString());
                        parameterOne.Parameter28 = float.Parse(dt.Rows[i][30].ToString());
                        parameterOne.Parameter29 = float.Parse(dt.Rows[i][31].ToString());
                        parameterOne.Parameter30 = float.Parse(dt.Rows[i][32].ToString());
                        parameterOne.Parameter31 = float.Parse(dt.Rows[i][33].ToString());
                        parameterOne.Parameter32 = float.Parse(dt.Rows[i][34].ToString());
                        parameterOne.Parameter33 = float.Parse(dt.Rows[i][35].ToString());
                        parameterOne.Parameter34 = float.Parse(dt.Rows[i][36].ToString());
                        parameterOne.Parameter35 = float.Parse(dt.Rows[i][37].ToString());
                        parameterOne.Parameter36 = float.Parse(dt.Rows[i][38].ToString());
                        parameterOne.Parameter37 = float.Parse(dt.Rows[i][39].ToString());
                        parameterOne.Parameter38 = float.Parse(dt.Rows[i][40].ToString());
                        parameterOne.Parameter39 = float.Parse(dt.Rows[i][41].ToString());
                        parameterOne.Parameter40 = float.Parse(dt.Rows[i][42].ToString());
                        parameterOne.Parameter41 = float.Parse(dt.Rows[i][43].ToString());
                        parameterOne.Parameter42 = float.Parse(dt.Rows[i][44].ToString());
                        parameterOne.Parameter43 = float.Parse(dt.Rows[i][45].ToString());
                        parameterOne.Parameter44 = float.Parse(dt.Rows[i][46].ToString());
                        parameterOne.Parameter45 = float.Parse(dt.Rows[i][47].ToString());
                        parameterOne.Parameter46 = float.Parse(dt.Rows[i][48].ToString());
                        parameterOne.Parameter47 = float.Parse(dt.Rows[i][49].ToString());
                        parameterOne.Parameter48 = float.Parse(dt.Rows[i][50].ToString());
                        parameterOne.Parameter49 = float.Parse(dt.Rows[i][51].ToString());
                        parameterOne.Parameter50 = float.Parse(dt.Rows[i][52].ToString());
                        parameterOne.Parameter51 = float.Parse(dt.Rows[i][53].ToString());
                        parameterOne.Parameter52 = float.Parse(dt.Rows[i][54].ToString());
                        parameterOne.Parameter53 = float.Parse(dt.Rows[i][55].ToString());
                        parameterOne.Parameter54 = float.Parse(dt.Rows[i][56].ToString());
                        parameterOne.Parameter55 = float.Parse(dt.Rows[i][57].ToString());
                        parameterOne.Parameter56 = float.Parse(dt.Rows[i][58].ToString());
                        parameterOne.Parameter57 = float.Parse(dt.Rows[i][59].ToString());
                        parameterOne.Parameter58 = float.Parse(dt.Rows[i][60].ToString());
                        parameterOne.Parameter59 = float.Parse(dt.Rows[i][61].ToString());
                        parameterOne.Parameter60 = float.Parse(dt.Rows[i][62].ToString());
                        parameterOne.Parameter61 = float.Parse(dt.Rows[i][63].ToString());
                        parameterOne.Parameter62 = float.Parse(dt.Rows[i][64].ToString());
                        parameterOne.Parameter63 = float.Parse(dt.Rows[i][65].ToString());
                        parameterOne.Parameter64 = float.Parse(dt.Rows[i][66].ToString());
                        parameterOne.Parameter65 = float.Parse(dt.Rows[i][67].ToString());
                        parameterOne.Parameter66 = float.Parse(dt.Rows[i][68].ToString());
                        parameterOne.Parameter67 = float.Parse(dt.Rows[i][69].ToString());
                        parameterOne.Parameter68 = float.Parse(dt.Rows[i][70].ToString());
                        parameterOne.Parameter69 = float.Parse(dt.Rows[i][71].ToString());
                        parameterOne.Parameter70 = float.Parse(dt.Rows[i][72].ToString());
                        parameterOne.Parameter71 = float.Parse(dt.Rows[i][73].ToString());
                        parameterOne.Parameter72 = float.Parse(dt.Rows[i][74].ToString());
                        parameterOne.Parameter73 = float.Parse(dt.Rows[i][75].ToString());
                        parameterOne.Parameter74 = float.Parse(dt.Rows[i][76].ToString());
                        parameterOne.Parameter75 = float.Parse(dt.Rows[i][77].ToString());
                        parameterOne.Parameter76 = float.Parse(dt.Rows[i][78].ToString());
                        parameterOne.Parameter77 = float.Parse(dt.Rows[i][79].ToString());
                        parameterOne.Parameter78 = float.Parse(dt.Rows[i][80].ToString());
                        parameterOne.Parameter79 = float.Parse(dt.Rows[i][81].ToString());
                        parameterOne.Parameter80 = float.Parse(dt.Rows[i][82].ToString());
                        parameterOne.Parameter81 = float.Parse(dt.Rows[i][83].ToString());
                        parameterOne.Parameter82 = float.Parse(dt.Rows[i][84].ToString());
                        parameterOne.Parameter83 = float.Parse(dt.Rows[i][85].ToString());
                        parameterOne.Parameter84 = float.Parse(dt.Rows[i][86].ToString());
                        parameterOne.Parameter85 = float.Parse(dt.Rows[i][87].ToString());
                        parameterOne.Parameter86 = float.Parse(dt.Rows[i][88].ToString());
                        parameterOne.Parameter87 = float.Parse(dt.Rows[i][89].ToString());
                        parameterOne.Parameter88 = float.Parse(dt.Rows[i][90].ToString());
                        parameterOne.Parameter89 = float.Parse(dt.Rows[i][91].ToString());
                        parameterOne.Parameter90 = float.Parse(dt.Rows[i][92].ToString());
                        parameterOne.Parameter91 = float.Parse(dt.Rows[i][93].ToString());
                        parameterOne.Parameter92 = float.Parse(dt.Rows[i][94].ToString());
                        parameterOne.Parameter93 = float.Parse(dt.Rows[i][95].ToString());
                        parameterOne.Parameter94 = float.Parse(dt.Rows[i][96].ToString());
                        parameterOne.Parameter95 = float.Parse(dt.Rows[i][97].ToString());
                        parameterOne.Parameter96 = float.Parse(dt.Rows[i][98].ToString());
                        parameterOne.Parameter97 = float.Parse(dt.Rows[i][99].ToString());
                        parameterOne.Parameter98 = float.Parse(dt.Rows[i][100].ToString());
                        parameterOne.Parameter99 = float.Parse(dt.Rows[i][101].ToString());
                        parameterOne.Parameter100 = float.Parse(dt.Rows[i][102].ToString());
                        parameterOne.Parameter101 = float.Parse(dt.Rows[i][103].ToString());
                        parameterOne.Parameter102 = float.Parse(dt.Rows[i][104].ToString());
                        parameterOne.Parameter103 = float.Parse(dt.Rows[i][105].ToString());
                        parameterOne.Parameter104 = float.Parse(dt.Rows[i][106].ToString());
                        parameterOne.Parameter105 = float.Parse(dt.Rows[i][107].ToString());
                        parameterOne.Parameter106 = float.Parse(dt.Rows[i][108].ToString());
                        parameterOne.Parameter107 = float.Parse(dt.Rows[i][109].ToString());
                        parameterOne.Parameter108 = float.Parse(dt.Rows[i][110].ToString());
                        parameterOne.Parameter109 = float.Parse(dt.Rows[i][111].ToString());
                        parameterOne.Parameter110 = float.Parse(dt.Rows[i][112].ToString());
                        parameterOne.Parameter111 = float.Parse(dt.Rows[i][113].ToString());
                        parameterOne.Parameter112 = float.Parse(dt.Rows[i][114].ToString());
                        parameterOne.Parameter113 = float.Parse(dt.Rows[i][115].ToString());
                        parameterOne.Parameter114 = float.Parse(dt.Rows[i][116].ToString());
                        parameterOne.Parameter115 = float.Parse(dt.Rows[i][117].ToString());
                        parameterOne.Parameter116 = float.Parse(dt.Rows[i][118].ToString());
                        parameterOne.Parameter117 = float.Parse(dt.Rows[i][119].ToString());
                        parameterOne.Parameter118 = float.Parse(dt.Rows[i][120].ToString());
                        parameterOne.Parameter119 = float.Parse(dt.Rows[i][121].ToString());
                        parameterOne.Parameter120 = float.Parse(dt.Rows[i][122].ToString());
                        parameterOne.Parameter121 = float.Parse(dt.Rows[i][123].ToString());
                        parameterOne.Parameter122 = float.Parse(dt.Rows[i][124].ToString());
                        parameterOne.Parameter123 = float.Parse(dt.Rows[i][125].ToString());
                        parameterOne.Parameter124 = float.Parse(dt.Rows[i][126].ToString());
                        parameterOne.Parameter125 = float.Parse(dt.Rows[i][127].ToString());
                        parameterOne.Parameter126 = float.Parse(dt.Rows[i][128].ToString());
                        parameterOne.Parameter127 = float.Parse(dt.Rows[i][129].ToString());
                        parameterOne.Parameter128 = float.Parse(dt.Rows[i][130].ToString());
                        parameterOne.Parameter129 = float.Parse(dt.Rows[i][131].ToString());
                        parameterOne.Parameter130 = float.Parse(dt.Rows[i][132].ToString());
                        parameterOne.Parameter131 = float.Parse(dt.Rows[i][133].ToString());
                        parameterOne.Parameter132 = float.Parse(dt.Rows[i][134].ToString());
                        parameterOne.Parameter133 = float.Parse(dt.Rows[i][135].ToString());
                        parameterOne.Parameter134 = float.Parse(dt.Rows[i][136].ToString());
                        parameterOne.Parameter135 = float.Parse(dt.Rows[i][137].ToString());
                        parameterOne.Parameter136 = float.Parse(dt.Rows[i][138].ToString());
                        parameterOne.Parameter137 = float.Parse(dt.Rows[i][139].ToString());
                        parameterOne.Parameter138 = float.Parse(dt.Rows[i][140].ToString());
                        parameterOne.Parameter139 = float.Parse(dt.Rows[i][141].ToString());
                        parameterOne.Parameter140 = float.Parse(dt.Rows[i][142].ToString());
                        parameterOne.Parameter141 = float.Parse(dt.Rows[i][143].ToString());
                        parameterOne.Parameter142 = float.Parse(dt.Rows[i][144].ToString());
                        parameterOne.Parameter143 = float.Parse(dt.Rows[i][145].ToString());
                        parameterOne.Parameter144 = float.Parse(dt.Rows[i][146].ToString());
                        parameterOne.Parameter145 = float.Parse(dt.Rows[i][147].ToString());
                        parameterOne.Parameter146 = float.Parse(dt.Rows[i][148].ToString());
                        parameterOne.Parameter147 = float.Parse(dt.Rows[i][149].ToString());
                        parameterOne.Parameter148 = float.Parse(dt.Rows[i][150].ToString());
                        parameterOne.Parameter149 = float.Parse(dt.Rows[i][151].ToString());
                        parameterOne.Parameter150 = float.Parse(dt.Rows[i][152].ToString());
                        parameterOne.Parameter151 = float.Parse(dt.Rows[i][153].ToString());
                        parameterOne.Parameter152 = float.Parse(dt.Rows[i][154].ToString());
                        parameterOne.Parameter153 = float.Parse(dt.Rows[i][155].ToString());
                        parameterOne.Parameter154 = float.Parse(dt.Rows[i][156].ToString());
                        parameterOne.Parameter155 = float.Parse(dt.Rows[i][157].ToString());
                        parameterOne.Parameter156 = float.Parse(dt.Rows[i][158].ToString());
                        parameterOne.Parameter157 = float.Parse(dt.Rows[i][159].ToString());
                        parameterOne.Parameter158 = float.Parse(dt.Rows[i][160].ToString());
                        parameterOne.Parameter159 = float.Parse(dt.Rows[i][161].ToString());
                        parameterOne.Parameter160 = float.Parse(dt.Rows[i][162].ToString());
                        parameterOne.Parameter161 = float.Parse(dt.Rows[i][163].ToString());
                        parameterOne.Parameter162 = float.Parse(dt.Rows[i][164].ToString());
                        parameterOne.Parameter163 = float.Parse(dt.Rows[i][165].ToString());
                        parameterOne.Parameter164 = float.Parse(dt.Rows[i][166].ToString());
                        parameterOne.Parameter165 = float.Parse(dt.Rows[i][167].ToString());
                        parameterOne.Parameter166 = float.Parse(dt.Rows[i][168].ToString());
                        parameterOne.Parameter167 = float.Parse(dt.Rows[i][169].ToString());
                        parameterOne.Parameter168 = float.Parse(dt.Rows[i][170].ToString());
                        parameterOne.Parameter169 = float.Parse(dt.Rows[i][171].ToString());
                        parameterOne.Parameter170 = float.Parse(dt.Rows[i][172].ToString());
                        parameterOne.Parameter171 = float.Parse(dt.Rows[i][173].ToString());
                        parameterOne.Parameter172 = float.Parse(dt.Rows[i][174].ToString());
                        parameterOne.Parameter173 = float.Parse(dt.Rows[i][175].ToString());
                        parameterOne.Parameter174 = float.Parse(dt.Rows[i][176].ToString());
                        parameterOne.Parameter175 = float.Parse(dt.Rows[i][177].ToString());
                        parameterOne.Parameter176 = float.Parse(dt.Rows[i][178].ToString());
                        parameterOne.Parameter177 = float.Parse(dt.Rows[i][179].ToString());
                        parameterOne.Parameter178 = float.Parse(dt.Rows[i][180].ToString());
                        parameterOne.Parameter179 = float.Parse(dt.Rows[i][181].ToString());
                        parameterOne.Parameter180 = float.Parse(dt.Rows[i][182].ToString());
                        parameterOne.Parameter181 = float.Parse(dt.Rows[i][183].ToString());
                        parameterOne.Parameter182 = float.Parse(dt.Rows[i][184].ToString());
                        parameterOne.Parameter183 = float.Parse(dt.Rows[i][185].ToString());
                        parameterOne.Parameter184 = float.Parse(dt.Rows[i][186].ToString());
                        parameterOne.Parameter185 = float.Parse(dt.Rows[i][187].ToString());
                        parameterOne.Parameter186 = float.Parse(dt.Rows[i][188].ToString());
                        parameterOne.Parameter187 = float.Parse(dt.Rows[i][189].ToString());
                        parameterOne.Parameter188 = float.Parse(dt.Rows[i][190].ToString());
                        parameterOne.Parameter189 = float.Parse(dt.Rows[i][191].ToString());
                        parameterOne.Parameter190 = float.Parse(dt.Rows[i][192].ToString());
                        parameterOne.Parameter191 = float.Parse(dt.Rows[i][193].ToString());
                        parameterOne.Parameter192 = float.Parse(dt.Rows[i][194].ToString());
                        parameterOne.Parameter193 = float.Parse(dt.Rows[i][195].ToString());
                        parameterOne.Parameter194 = float.Parse(dt.Rows[i][196].ToString());
                        parameterOne.Parameter195 = float.Parse(dt.Rows[i][197].ToString());
                        parameterOne.Parameter196 = float.Parse(dt.Rows[i][198].ToString());
                        parameterOne.Parameter197 = float.Parse(dt.Rows[i][199].ToString());
                        parameterOne.Parameter198 = float.Parse(dt.Rows[i][200].ToString());
                        parameterOne.Parameter199 = float.Parse(dt.Rows[i][201].ToString());
                        parameterOne.Parameter200 = float.Parse(dt.Rows[i][202].ToString());
                        parameterOne.Parameter201 = float.Parse(dt.Rows[i][203].ToString());
                        parameterOne.Parameter202 = float.Parse(dt.Rows[i][204].ToString());
                        parameterOne.Parameter203 = float.Parse(dt.Rows[i][205].ToString());
                        parameterOne.Parameter204 = float.Parse(dt.Rows[i][206].ToString());
                        parameterOne.Parameter205 = float.Parse(dt.Rows[i][207].ToString());
                        parameterOne.Parameter206 = float.Parse(dt.Rows[i][208].ToString());
                        parameterOne.Parameter207 = float.Parse(dt.Rows[i][209].ToString());
                        parameterOne.Parameter208 = float.Parse(dt.Rows[i][210].ToString());
                        parameterOne.Parameter209 = float.Parse(dt.Rows[i][211].ToString());
                        parameterOne.Parameter210 = float.Parse(dt.Rows[i][212].ToString());
                        parameterOne.Parameter211 = float.Parse(dt.Rows[i][213].ToString());
                        parameterOne.Parameter212 = float.Parse(dt.Rows[i][214].ToString());
                        parameterOne.Parameter213 = float.Parse(dt.Rows[i][215].ToString());
                        parameterOne.Parameter214 = float.Parse(dt.Rows[i][216].ToString());
                        parameterOne.Parameter215 = float.Parse(dt.Rows[i][217].ToString());
                        parameterOne.Parameter216 = float.Parse(dt.Rows[i][218].ToString());
                        parameterOne.Parameter217 = float.Parse(dt.Rows[i][219].ToString());
                        parameterOne.Parameter218 = float.Parse(dt.Rows[i][220].ToString());
                        parameterOne.Parameter219 = float.Parse(dt.Rows[i][221].ToString());
                        parameterOne.Parameter220 = float.Parse(dt.Rows[i][222].ToString());
                        parameterOne.Parameter221 = float.Parse(dt.Rows[i][223].ToString());
                        parameterOne.Parameter222 = float.Parse(dt.Rows[i][224].ToString());
                        parameterOne.Parameter223 = float.Parse(dt.Rows[i][225].ToString());
                        parameterOne.Parameter224 = float.Parse(dt.Rows[i][226].ToString());
                        parameterOne.Parameter225 = float.Parse(dt.Rows[i][227].ToString());
                        parameterOne.Parameter226 = float.Parse(dt.Rows[i][228].ToString());
                        parameterOne.Parameter227 = float.Parse(dt.Rows[i][229].ToString());
                        parameterOne.Parameter228 = float.Parse(dt.Rows[i][230].ToString());
                        parameterOne.Parameter229 = float.Parse(dt.Rows[i][231].ToString());
                        parameterOne.Parameter230 = float.Parse(dt.Rows[i][232].ToString());
                        parameterOne.Parameter231 = float.Parse(dt.Rows[i][233].ToString());
                        parameterOne.Parameter232 = float.Parse(dt.Rows[i][234].ToString());
                        parameterOne.Parameter233 = float.Parse(dt.Rows[i][235].ToString());
                        parameterOne.Parameter234 = float.Parse(dt.Rows[i][236].ToString());
                        parameterOne.Parameter235 = float.Parse(dt.Rows[i][237].ToString());
                        parameterOne.Parameter236 = float.Parse(dt.Rows[i][238].ToString());
                        parameterOne.Parameter237 = float.Parse(dt.Rows[i][239].ToString());
                        parameterOne.Parameter238 = float.Parse(dt.Rows[i][240].ToString());
                        parameterOne.Parameter239 = float.Parse(dt.Rows[i][241].ToString());
                        parameterOne.Parameter240 = float.Parse(dt.Rows[i][242].ToString());
                        parameterOne.Parameter241 = float.Parse(dt.Rows[i][243].ToString());
                        parameterOne.Parameter242 = float.Parse(dt.Rows[i][244].ToString());
                        parameterOne.Parameter243 = float.Parse(dt.Rows[i][245].ToString());
                        parameterOne.Parameter244 = float.Parse(dt.Rows[i][246].ToString());
                        parameterOne.Parameter245 = float.Parse(dt.Rows[i][247].ToString());
                        parameterOne.Parameter246 = float.Parse(dt.Rows[i][248].ToString());
                        parameterOne.Parameter247 = float.Parse(dt.Rows[i][249].ToString());
                        parameterOne.Parameter248 = float.Parse(dt.Rows[i][250].ToString());
                        parameterOne.Parameter249 = float.Parse(dt.Rows[i][251].ToString());
                        parameterOne.Parameter250 = float.Parse(dt.Rows[i][252].ToString());
                        parameterOne.Parameter251 = float.Parse(dt.Rows[i][253].ToString());
                        parameterOne.Parameter252 = float.Parse(dt.Rows[i][254].ToString());
                        parameterOne.Parameter253 = float.Parse(dt.Rows[i][255].ToString());
                        parameterOne.Parameter254 = float.Parse(dt.Rows[i][256].ToString());
                        parameterOne.Parameter255 = float.Parse(dt.Rows[i][257].ToString());
                        parameterOne.Parameter256 = float.Parse(dt.Rows[i][258].ToString());
                        parameterOne.Parameter257 = float.Parse(dt.Rows[i][259].ToString());
                        parameterOne.Parameter258 = float.Parse(dt.Rows[i][260].ToString());
                        parameterOne.Parameter259 = float.Parse(dt.Rows[i][261].ToString());
                        parameterOne.Parameter260 = float.Parse(dt.Rows[i][262].ToString());
                        parameterOne.Parameter261 = float.Parse(dt.Rows[i][263].ToString());
                        parameterOne.Parameter262 = float.Parse(dt.Rows[i][264].ToString());
                        parameterOne.Parameter263 = float.Parse(dt.Rows[i][265].ToString());
                        parameterOne.Parameter264 = float.Parse(dt.Rows[i][266].ToString());
                        parameterOne.Parameter265 = float.Parse(dt.Rows[i][267].ToString());
                        parameterOne.Parameter266 = float.Parse(dt.Rows[i][268].ToString());
                        parameterOne.Parameter267 = float.Parse(dt.Rows[i][269].ToString());
                        parameterOne.Parameter268 = float.Parse(dt.Rows[i][270].ToString());
                        parameterOne.Parameter269 = float.Parse(dt.Rows[i][271].ToString());
                        parameterOne.Parameter270 = float.Parse(dt.Rows[i][272].ToString());
                        parameterOne.Parameter271 = float.Parse(dt.Rows[i][273].ToString());
                        parameterOne.Parameter272 = float.Parse(dt.Rows[i][274].ToString());
                        parameterOne.Parameter273 = float.Parse(dt.Rows[i][275].ToString());
                        parameterOne.Parameter274 = float.Parse(dt.Rows[i][276].ToString());
                        parameterOne.Parameter275 = float.Parse(dt.Rows[i][277].ToString());
                        parameterOne.Parameter276 = float.Parse(dt.Rows[i][278].ToString());
                        parameterOne.Parameter277 = float.Parse(dt.Rows[i][279].ToString());
                        parameterOne.Parameter278 = float.Parse(dt.Rows[i][280].ToString());
                        parameterOne.Parameter279 = float.Parse(dt.Rows[i][281].ToString());
                        parameterOne.Parameter280 = float.Parse(dt.Rows[i][282].ToString());
                        parameterOne.Parameter281 = float.Parse(dt.Rows[i][283].ToString());
                        parameterOne.Parameter282 = float.Parse(dt.Rows[i][284].ToString());
                        parameterOne.Parameter283 = float.Parse(dt.Rows[i][285].ToString());
                        parameterOne.Parameter284 = float.Parse(dt.Rows[i][286].ToString());
                        parameterOne.Parameter285 = float.Parse(dt.Rows[i][287].ToString());
                        parameterOne.Parameter286 = float.Parse(dt.Rows[i][288].ToString());
                        parameterOne.Parameter287 = float.Parse(dt.Rows[i][289].ToString());
                        parameterOne.Parameter288 = float.Parse(dt.Rows[i][290].ToString());
                        parameterOne.Parameter289 = float.Parse(dt.Rows[i][291].ToString());
                        parameterOne.Parameter290 = float.Parse(dt.Rows[i][292].ToString());
                        parameterOne.Parameter291 = float.Parse(dt.Rows[i][293].ToString());
                        parameterOne.Parameter292 = float.Parse(dt.Rows[i][294].ToString());
                        parameterOne.Parameter293 = float.Parse(dt.Rows[i][295].ToString());
                        parameterOne.Parameter294 = float.Parse(dt.Rows[i][296].ToString());
                        parameterOne.Parameter295 = float.Parse(dt.Rows[i][297].ToString());
                        parameterOne.Parameter296 = float.Parse(dt.Rows[i][298].ToString());
                        parameterOne.Parameter297 = float.Parse(dt.Rows[i][299].ToString());
                        parameterOne.Parameter298 = float.Parse(dt.Rows[i][300].ToString());
                        parameterOne.Parameter299 = float.Parse(dt.Rows[i][301].ToString());
                        parameterOne.Parameter300 = float.Parse(dt.Rows[i][302].ToString());
                        parameterOne.Parameter301 = float.Parse(dt.Rows[i][303].ToString());
                        parameterOne.Parameter302 = float.Parse(dt.Rows[i][304].ToString());
                        parameterOne.Parameter303 = float.Parse(dt.Rows[i][305].ToString());
                        parameterOne.Parameter304 = float.Parse(dt.Rows[i][306].ToString());
                        parameterOne.Parameter305 = float.Parse(dt.Rows[i][307].ToString());
                        parameterOne.Parameter306 = float.Parse(dt.Rows[i][308].ToString());
                        parameterOne.Parameter307 = float.Parse(dt.Rows[i][309].ToString());
                        parameterOne.Parameter308 = float.Parse(dt.Rows[i][310].ToString());
                        parameterOne.Parameter309 = float.Parse(dt.Rows[i][311].ToString());
                        parameterOne.Parameter310 = float.Parse(dt.Rows[i][312].ToString());

                        #endregion

                        parameter.Add(parameterOne);
                    }

                    ControlInvoker.Invoke(this, delegate
                    {
                        if (parameter != null && parameter.Count > 0)
                        {
                         
                                dataGridView2.DataSource = null;
                                dataGridView2.VirtualMode = false;
                                dataGridView2.AutoGenerateColumns = false;
                                dataGridView2.DataSource = parameter;
                                dataGridView2.ClearSelection();
                      
                        }
                    });
                }
            }
            catch
            { }

        }

        public void OverRollReport()
        {    
            StringBuilder sb = new StringBuilder();
            sb.Append("select CellectTime");


            for (int i = 0; i < 168; i++)
            {
                sb.Append(",Parameter");
                sb.Append((i + 1).ToString());
            }

            sb.Append(" from table1 order by ID desc limit 0,15");
            try
            {
                DataTable dt = SQLite.OverRollGetDataSet(sb.ToString()).Tables[0];

                if (dt.Rows.Count > 0)
                {
                    overRolls.Clear();


                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Model.OverRoll overRollOne = new Model.OverRoll();

                        #region 参数赋值
                        overRollOne.CellectTime = (dt.Rows[i][0]).ToString();
                        overRollOne.Parameter1 = float.Parse(dt.Rows[i][1].ToString());
                        overRollOne.Parameter2 = float.Parse(dt.Rows[i][2].ToString());
                        overRollOne.Parameter3 = float.Parse(dt.Rows[i][3].ToString());
                        overRollOne.Parameter4 = float.Parse(dt.Rows[i][4].ToString());
                        overRollOne.Parameter5 = float.Parse(dt.Rows[i][5].ToString());
                        overRollOne.Parameter6 = float.Parse(dt.Rows[i][6].ToString());
                        overRollOne.Parameter7 = float.Parse(dt.Rows[i][7].ToString());
                        overRollOne.Parameter8 = float.Parse(dt.Rows[i][8].ToString());
                        overRollOne.Parameter9 = float.Parse(dt.Rows[i][9].ToString());
                        overRollOne.Parameter10 = float.Parse(dt.Rows[i][10].ToString());
                        overRollOne.Parameter11 = float.Parse(dt.Rows[i][11].ToString());
                        overRollOne.Parameter12 = float.Parse(dt.Rows[i][12].ToString());
                        overRollOne.Parameter13 = float.Parse(dt.Rows[i][13].ToString());
                        overRollOne.Parameter14 = float.Parse(dt.Rows[i][14].ToString());
                        overRollOne.Parameter15 = float.Parse(dt.Rows[i][15].ToString());
                        overRollOne.Parameter16 = float.Parse(dt.Rows[i][16].ToString());
                        overRollOne.Parameter17 = float.Parse(dt.Rows[i][17].ToString());
                        overRollOne.Parameter18 = float.Parse(dt.Rows[i][18].ToString());
                        overRollOne.Parameter19 = float.Parse(dt.Rows[i][19].ToString());
                        overRollOne.Parameter20 = float.Parse(dt.Rows[i][20].ToString());
                        overRollOne.Parameter21 = float.Parse(dt.Rows[i][21].ToString());
                        overRollOne.Parameter22 = float.Parse(dt.Rows[i][22].ToString());
                        overRollOne.Parameter23 = float.Parse(dt.Rows[i][23].ToString());
                        overRollOne.Parameter24 = float.Parse(dt.Rows[i][24].ToString());
                        overRollOne.Parameter25 = float.Parse(dt.Rows[i][25].ToString());
                        overRollOne.Parameter26 = float.Parse(dt.Rows[i][26].ToString());
                        overRollOne.Parameter27 = float.Parse(dt.Rows[i][27].ToString());
                        overRollOne.Parameter28 = float.Parse(dt.Rows[i][28].ToString());
                        overRollOne.Parameter29 = float.Parse(dt.Rows[i][29].ToString());
                        overRollOne.Parameter30 = float.Parse(dt.Rows[i][30].ToString());
                        overRollOne.Parameter31 = float.Parse(dt.Rows[i][31].ToString());
                        overRollOne.Parameter32 = float.Parse(dt.Rows[i][32].ToString());
                        overRollOne.Parameter33 = float.Parse(dt.Rows[i][33].ToString());
                        overRollOne.Parameter34 = float.Parse(dt.Rows[i][34].ToString());
                        overRollOne.Parameter35 = float.Parse(dt.Rows[i][35].ToString());
                        overRollOne.Parameter36 = float.Parse(dt.Rows[i][36].ToString());
                        overRollOne.Parameter37 = float.Parse(dt.Rows[i][37].ToString());
                        overRollOne.Parameter38 = float.Parse(dt.Rows[i][38].ToString());
                        overRollOne.Parameter39 = float.Parse(dt.Rows[i][39].ToString());
                        overRollOne.Parameter40 = float.Parse(dt.Rows[i][40].ToString());
                        overRollOne.Parameter41 = float.Parse(dt.Rows[i][41].ToString());
                        overRollOne.Parameter42 = float.Parse(dt.Rows[i][42].ToString());
                        overRollOne.Parameter43 = float.Parse(dt.Rows[i][43].ToString());
                        overRollOne.Parameter44 = float.Parse(dt.Rows[i][44].ToString());
                        overRollOne.Parameter45 = float.Parse(dt.Rows[i][45].ToString());
                        overRollOne.Parameter46 = float.Parse(dt.Rows[i][46].ToString());
                        overRollOne.Parameter47 = float.Parse(dt.Rows[i][47].ToString());
                        overRollOne.Parameter48 = float.Parse(dt.Rows[i][48].ToString());
                        overRollOne.Parameter49 = float.Parse(dt.Rows[i][49].ToString());
                        overRollOne.Parameter50 = float.Parse(dt.Rows[i][50].ToString());
                        overRollOne.Parameter51 = float.Parse(dt.Rows[i][51].ToString());
                        overRollOne.Parameter52 = float.Parse(dt.Rows[i][52].ToString());
                        overRollOne.Parameter53 = float.Parse(dt.Rows[i][53].ToString());
                        overRollOne.Parameter54 = float.Parse(dt.Rows[i][54].ToString());
                        overRollOne.Parameter55 = float.Parse(dt.Rows[i][55].ToString());
                        overRollOne.Parameter56 = float.Parse(dt.Rows[i][56].ToString());
                        overRollOne.Parameter57 = float.Parse(dt.Rows[i][57].ToString());
                        overRollOne.Parameter58 = float.Parse(dt.Rows[i][58].ToString());
                        overRollOne.Parameter59 = float.Parse(dt.Rows[i][59].ToString());
                        overRollOne.Parameter60 = float.Parse(dt.Rows[i][60].ToString());
                        overRollOne.Parameter61 = float.Parse(dt.Rows[i][61].ToString());
                        overRollOne.Parameter62 = float.Parse(dt.Rows[i][62].ToString());
                        overRollOne.Parameter63 = float.Parse(dt.Rows[i][63].ToString());
                        overRollOne.Parameter64 = float.Parse(dt.Rows[i][64].ToString());
                        overRollOne.Parameter65 = float.Parse(dt.Rows[i][65].ToString());
                        overRollOne.Parameter66 = float.Parse(dt.Rows[i][66].ToString());
                        overRollOne.Parameter67 = float.Parse(dt.Rows[i][67].ToString());
                        overRollOne.Parameter68 = float.Parse(dt.Rows[i][68].ToString());
                        overRollOne.Parameter69 = float.Parse(dt.Rows[i][69].ToString());
                        overRollOne.Parameter70 = float.Parse(dt.Rows[i][70].ToString());
                        overRollOne.Parameter71 = float.Parse(dt.Rows[i][71].ToString());
                        overRollOne.Parameter72 = float.Parse(dt.Rows[i][72].ToString());
                        overRollOne.Parameter73 = float.Parse(dt.Rows[i][73].ToString());
                        overRollOne.Parameter74 = float.Parse(dt.Rows[i][74].ToString());
                        overRollOne.Parameter75 = float.Parse(dt.Rows[i][75].ToString());
                        overRollOne.Parameter76 = float.Parse(dt.Rows[i][76].ToString());
                        overRollOne.Parameter77 = float.Parse(dt.Rows[i][77].ToString());
                        overRollOne.Parameter78 = float.Parse(dt.Rows[i][78].ToString());
                        overRollOne.Parameter79 = float.Parse(dt.Rows[i][79].ToString());
                        overRollOne.Parameter80 = float.Parse(dt.Rows[i][80].ToString());
                        overRollOne.Parameter81 = float.Parse(dt.Rows[i][81].ToString());
                        overRollOne.Parameter82 = float.Parse(dt.Rows[i][82].ToString());
                        overRollOne.Parameter83 = float.Parse(dt.Rows[i][83].ToString());
                        overRollOne.Parameter84 = float.Parse(dt.Rows[i][84].ToString());
                        overRollOne.Parameter85 = float.Parse(dt.Rows[i][85].ToString());
                        overRollOne.Parameter86 = float.Parse(dt.Rows[i][86].ToString());
                        overRollOne.Parameter87 = float.Parse(dt.Rows[i][87].ToString());
                        overRollOne.Parameter88 = float.Parse(dt.Rows[i][88].ToString());
                        overRollOne.Parameter89 = float.Parse(dt.Rows[i][89].ToString());
                        overRollOne.Parameter90 = float.Parse(dt.Rows[i][90].ToString());
                        overRollOne.Parameter91 = float.Parse(dt.Rows[i][91].ToString());
                        overRollOne.Parameter92 = float.Parse(dt.Rows[i][92].ToString());
                        overRollOne.Parameter93 = float.Parse(dt.Rows[i][93].ToString());
                        overRollOne.Parameter94 = float.Parse(dt.Rows[i][94].ToString());
                        overRollOne.Parameter95 = float.Parse(dt.Rows[i][95].ToString());
                        overRollOne.Parameter96 = float.Parse(dt.Rows[i][96].ToString());
                        overRollOne.Parameter97 = float.Parse(dt.Rows[i][97].ToString());
                        overRollOne.Parameter98 = float.Parse(dt.Rows[i][98].ToString());
                        overRollOne.Parameter99 = float.Parse(dt.Rows[i][99].ToString());
                        overRollOne.Parameter100 = float.Parse(dt.Rows[i][100].ToString());
                        overRollOne.Parameter101 = float.Parse(dt.Rows[i][101].ToString());
                        overRollOne.Parameter102 = float.Parse(dt.Rows[i][102].ToString());
                        overRollOne.Parameter103 = float.Parse(dt.Rows[i][103].ToString());
                        overRollOne.Parameter104 = float.Parse(dt.Rows[i][104].ToString());
                        overRollOne.Parameter105 = float.Parse(dt.Rows[i][105].ToString());
                        overRollOne.Parameter106 = float.Parse(dt.Rows[i][106].ToString());
                        overRollOne.Parameter107 = float.Parse(dt.Rows[i][107].ToString());
                        overRollOne.Parameter108 = float.Parse(dt.Rows[i][108].ToString());
                        overRollOne.Parameter109 = float.Parse(dt.Rows[i][109].ToString());
                        overRollOne.Parameter110 = float.Parse(dt.Rows[i][110].ToString());
                        overRollOne.Parameter111 = float.Parse(dt.Rows[i][111].ToString());
                        overRollOne.Parameter112 = float.Parse(dt.Rows[i][112].ToString());
                        overRollOne.Parameter113 = float.Parse(dt.Rows[i][113].ToString());
                        overRollOne.Parameter114 = float.Parse(dt.Rows[i][114].ToString());
                        overRollOne.Parameter115 = float.Parse(dt.Rows[i][115].ToString());
                        overRollOne.Parameter116 = float.Parse(dt.Rows[i][116].ToString());
                        overRollOne.Parameter117 = float.Parse(dt.Rows[i][117].ToString());
                        overRollOne.Parameter118 = float.Parse(dt.Rows[i][118].ToString());
                        overRollOne.Parameter119 = float.Parse(dt.Rows[i][119].ToString());
                        overRollOne.Parameter120 = float.Parse(dt.Rows[i][120].ToString());
                        overRollOne.Parameter121 = float.Parse(dt.Rows[i][121].ToString());
                        overRollOne.Parameter122 = float.Parse(dt.Rows[i][122].ToString());
                        overRollOne.Parameter123 = float.Parse(dt.Rows[i][123].ToString());
                        overRollOne.Parameter124 = float.Parse(dt.Rows[i][124].ToString());
                        overRollOne.Parameter125 = float.Parse(dt.Rows[i][125].ToString());
                        overRollOne.Parameter126 = float.Parse(dt.Rows[i][126].ToString());
                        overRollOne.Parameter127 = float.Parse(dt.Rows[i][127].ToString());
                        overRollOne.Parameter128 = float.Parse(dt.Rows[i][128].ToString());
                        overRollOne.Parameter129 = float.Parse(dt.Rows[i][129].ToString());
                        overRollOne.Parameter130 = float.Parse(dt.Rows[i][130].ToString());
                        overRollOne.Parameter131 = float.Parse(dt.Rows[i][131].ToString());
                        overRollOne.Parameter132 = float.Parse(dt.Rows[i][132].ToString());
                        overRollOne.Parameter133 = float.Parse(dt.Rows[i][133].ToString());
                        overRollOne.Parameter134 = float.Parse(dt.Rows[i][134].ToString());
                        overRollOne.Parameter135 = float.Parse(dt.Rows[i][135].ToString());
                        overRollOne.Parameter136 = float.Parse(dt.Rows[i][136].ToString());
                        overRollOne.Parameter137 = float.Parse(dt.Rows[i][137].ToString());
                        overRollOne.Parameter138 = float.Parse(dt.Rows[i][138].ToString());
                        overRollOne.Parameter139 = float.Parse(dt.Rows[i][139].ToString());
                        overRollOne.Parameter140 = float.Parse(dt.Rows[i][140].ToString());
                        overRollOne.Parameter141 = float.Parse(dt.Rows[i][141].ToString());
                        overRollOne.Parameter142 = float.Parse(dt.Rows[i][142].ToString());
                        overRollOne.Parameter143 = float.Parse(dt.Rows[i][143].ToString());
                        overRollOne.Parameter144 = float.Parse(dt.Rows[i][144].ToString());
                        overRollOne.Parameter145 = float.Parse(dt.Rows[i][145].ToString());
                        overRollOne.Parameter146 = float.Parse(dt.Rows[i][146].ToString());
                        overRollOne.Parameter147 = float.Parse(dt.Rows[i][147].ToString());
                        overRollOne.Parameter148 = float.Parse(dt.Rows[i][148].ToString());
                        overRollOne.Parameter149 = float.Parse(dt.Rows[i][149].ToString());
                        overRollOne.Parameter150 = float.Parse(dt.Rows[i][150].ToString());
                        overRollOne.Parameter151 = float.Parse(dt.Rows[i][151].ToString());
                        overRollOne.Parameter152 = float.Parse(dt.Rows[i][152].ToString());
                        overRollOne.Parameter153 = float.Parse(dt.Rows[i][153].ToString());
                        overRollOne.Parameter154 = float.Parse(dt.Rows[i][154].ToString());
                        overRollOne.Parameter155 = float.Parse(dt.Rows[i][155].ToString());
                        overRollOne.Parameter156 = float.Parse(dt.Rows[i][156].ToString());
                        overRollOne.Parameter157 = float.Parse(dt.Rows[i][157].ToString());
                        overRollOne.Parameter158 = float.Parse(dt.Rows[i][158].ToString());
                        overRollOne.Parameter159 = float.Parse(dt.Rows[i][159].ToString());
                        overRollOne.Parameter160 = float.Parse(dt.Rows[i][160].ToString());
                        overRollOne.Parameter161 = float.Parse(dt.Rows[i][161].ToString());
                        overRollOne.Parameter162 = float.Parse(dt.Rows[i][162].ToString());
                        overRollOne.Parameter163 = float.Parse(dt.Rows[i][163].ToString());
                        overRollOne.Parameter164 = float.Parse(dt.Rows[i][164].ToString());
                        overRollOne.Parameter165 = float.Parse(dt.Rows[i][165].ToString());
                        overRollOne.Parameter166 = float.Parse(dt.Rows[i][166].ToString());
                        overRollOne.Parameter167 = float.Parse(dt.Rows[i][167].ToString());
                        overRollOne.Parameter168 = float.Parse(dt.Rows[i][168].ToString());



                        #endregion

                        overRolls.Add(overRollOne);
                    }

                    ControlInvoker.Invoke(this, delegate
                    {
                        if (overRolls != null && overRolls.Count > 0)
                        {
                     
                                dataGridView3.DataSource = null;
                                dataGridView3.VirtualMode = false;
                                dataGridView3.AutoGenerateColumns = false;
                                dataGridView3.DataSource = overRolls;
                                dataGridView3.ClearSelection();
                          

                        }
                    });
                }
            }
            catch
            { }
        }

        //提前关闭窗体，则会引发InvalidOperationException
        public static class ControlInvoker
        {
            public static void Invoke(Control ctl, MethodInvoker method)
            {
                if (!ctl.IsHandleCreated)
                    return;

                if (ctl.IsDisposed)
                    return;

                if (ctl.InvokeRequired)
                {
                    ctl.Invoke(method);
                }
                else
                {
                    method();
                }
            }
        }
        #endregion 

        #region 忽略Datagrideview报错
        private void dataGridView2_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void dataGridView3_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }
        #endregion

        private void dgv_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }
    }
}
