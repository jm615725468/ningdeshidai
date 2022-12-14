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
using System.Windows.Forms;

using System.Runtime.InteropServices;
using System.Windows.Forms.DataVisualization.Charting;

namespace HNSys
{
    public partial class FrmHistoryCurvecs2 : Form
    {
        private Thread PeriodicEvent = null;

        private ChartArea chartArea1 = new ChartArea("C1");



        int Ymax = 500;
        int Ymin = -50;


        public FrmHistoryCurvecs2()
        {
            InitializeComponent();
        }

        private void FrmHistoryCurvecs_Load(object sender, EventArgs e)
        {
            InitSet();
            InitChart();
        }

        #region 设置初始化
        private void InitSet()
        {
            string SetSavePath = Application.StartupPath + "\\HNSet\\SetSave.ini";
            string 时间范围 = IniHelper.ReadIniData("FrmHistoryCurvecs2_Set", "时间范围", "", SetSavePath);
            if (时间范围 != "")
            {
                numericUpDown1.Value = decimal.Parse(时间范围);
            }

            string Y轴最大值 = IniHelper.ReadIniData("FrmHistoryCurvecs2_Set", "Y轴最大值", "", SetSavePath);
            if (Y轴最大值 != "")
            {
                numericUpDown3.Value = decimal.Parse(Y轴最大值);
                Ymax = int.Parse(Y轴最大值);
            }
            string Y轴最小值 = IniHelper.ReadIniData("FrmHistoryCurvecs2_Set", "Y轴最小值", "", SetSavePath);
            if (Y轴最小值 != "")
            {
                numericUpDown2.Value = decimal.Parse(Y轴最小值);
                Ymin = int.Parse(Y轴最小值);
            }
            string 调整范围 = IniHelper.ReadIniData("FrmHistoryCurvecs2_Set", "调整范围", "", SetSavePath);
            if (调整范围 != "")
            {
                numericUpDown4.Value = decimal.Parse(调整范围);
            }

        }
        #endregion



        #region 曲线图属性初始化

        private void InitChart()
        {
            //CreateChart();

            //chart1.ChartAreas[0].AxisY.ScaleView.Size = 100D;
            //chart1.ChartAreas[0].AxisX.Interval = 1D;

            //chart1.ChartAreas[0].AxisX.ScaleView.Size = 10D;

            //AddSeriesLine("A1膜长", Color.Black);


            //定义图表区域
            this.chart1.ChartAreas.Clear();
            //  ChartArea chartArea1 = new ChartArea("C1");
            this.chart1.ChartAreas.Add(chartArea1);
            //this.chart1.ChartAreas.Add(chartArea2);
            //this.chart1.ChartAreas.Add(chartArea3);
            //this.chart1.ChartAreas.Add(chartArea4);
            //this.chart1.ChartAreas.Add(chartArea5);
            //this.chart1.ChartAreas.Add(chartArea6);
            //this.chart1.ChartAreas.Add(chartArea7);



            //定义存储和显示点的容器
            this.chart1.Series.Clear();

            Series series1 = new Series(checkBox1.Text);
            series1.BorderWidth = 2;
            series1.CustomProperties = "PointWidth=2";

            Series series2 = new Series(checkBox2.Text);
            series2.BorderWidth = 2;
            series2.CustomProperties = "PointWidth=2";

            Series series3 = new Series(checkBox3.Text);   
            series3.BorderWidth = 2;
            series3.CustomProperties = "PointWidth=2";

            Series series4 = new Series(checkBox4.Text);
            series4.BorderWidth = 2;
            series4.CustomProperties = "PointWidth=2";

            Series series5 = new Series(checkBox5.Text);
            series5.BorderWidth = 2;
            series5.CustomProperties = "PointWidth=2";

            Series series6 = new Series(checkBox6.Text);
            series6.BorderWidth = 2;
            series6.CustomProperties = "PointWidth=2";

            //Series series7 = new Series("收卷张力");
            //series7.BorderWidth = 2;
            //series7.CustomProperties = "PointWidth=2";

            //series1.MarkerStyle = MarkerStyle.Circle;
            //series1.MarkerColor = Color.Black;
            //series1.MarkerSize = 6;



            this.chart1.Series.Add(series1);
            this.chart1.Series.Add(series2);
            this.chart1.Series.Add(series3);
            this.chart1.Series.Add(series4);
            this.chart1.Series.Add(series5);
            this.chart1.Series.Add(series6);
            //this.chart1.Series.Add(series7);


            /***************************允许X轴放大******************************/
            chartArea1.CursorX.IsUserEnabled = true;//
            chartArea1.CursorX.IsUserSelectionEnabled = true;
            chartArea1.CursorX.Interval = 0;
            chartArea1.CursorX.IntervalOffset = 0;
            chartArea1.CursorX.IntervalType = DateTimeIntervalType.Minutes;
            chartArea1.AxisX.ScaleView.Zoomable = true;
            chartArea1.AxisX.ScrollBar.IsPositionedInside = false;








            /***************************允许X轴放大******************************/


            //设置图表显示样式
            this.chart1.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";         //毫秒格式： hh:mm:ss.fff ，后面几个f则保留几位毫秒小数，此时要注意轴的最大值和最小值不要差太大
            this.chart1.ChartAreas[0].AxisX.LabelStyle.IntervalType = DateTimeIntervalType.Seconds;
            this.chart1.ChartAreas[0].AxisX.LabelStyle.Interval = 5;                //两个坐标值之间有多少个点
            this.chart1.ChartAreas[0].AxisX.LabelStyle.IsEndLabelVisible = false;   //防止X轴坐标跳跃

            this.chart1.ChartAreas[0].AxisX.MajorGrid.IntervalType = DateTimeIntervalType.Seconds;
            this.chart1.ChartAreas[0].AxisX.MajorGrid.Interval = 5;
            chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;



            //网格间隔
            chart1.ChartAreas[0].AxisY.ScaleView.Size = 120D;
            chart1.ChartAreas[0].AxisY.MajorGrid.Interval = 10;

            chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            //this.chart1.ChartAreas[0].AxisX.Minimum = DateTime.Now.ToOADate();      //当前时间
            //this.chart1.ChartAreas[0].AxisX.Maximum = DateTime.Now.ToOADate();
            //设置标题
            //this.chart1.Titles.Clear();
            //this.chart1.Titles.Add("S01");
            ////this.chart1.Titles[0].Text = "XXX显示";
            //this.chart1.Titles[0].ForeColor = Color.RoyalBlue;
            //this.chart1.Titles[0].Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            ////设置图表显示样式


            this.chart1.Series[0].Color = Color.Red;
            this.chart1.Series[0].ChartType = SeriesChartType.FastLine;

            this.chart1.Series[1].Color = Color.Black;
            this.chart1.Series[1].ChartType = SeriesChartType.FastLine;

            this.chart1.Series[2].Color = Color.Orange;
            this.chart1.Series[2].ChartType = SeriesChartType.FastLine;

            this.chart1.Series[3].Color = Color.Blue;
            this.chart1.Series[3].ChartType = SeriesChartType.FastLine;

            this.chart1.Series[4].Color = Color.Green;
            this.chart1.Series[4].ChartType = SeriesChartType.FastLine;

            this.chart1.Series[5].Color = Color.Yellow;
            this.chart1.Series[5].ChartType = SeriesChartType.FastLine;

            //this.chart1.Series[6].Color = Color.Gray;
            //this.chart1.Series[6].ChartType = SeriesChartType.Spline;

        }
        #endregion

     

        private void button1_Click(object sender, EventArgs e)
        {           
            dtp_Start.Value = dtp_Start.Value.AddMinutes(-(int)numericUpDown4.Value);
            Curvecs(); 
        }

        private void button2_Click(object sender, EventArgs e)
        {
          
            dtp_Start.Value = dtp_Start.Value.AddMinutes((int)numericUpDown4.Value);
            Curvecs();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            dtp_Start.Value = dtp_Start.Value;
            Curvecs();
        }


        public void Curvecs()
        {
            new System.Threading.Tasks.TaskFactory().StartNew(() =>
            {
                decimal dataNumDec = numericUpDown1.Value * 2;
                string dataNum = dataNumDec.ToString();

                StringBuilder sb = new StringBuilder();
                sb.Append("select CollectTime");

                for (int i = 0; i < 20; i++)
                {
                    sb.Append(",Parameter");
                    sb.Append((i + 1).ToString());
                }

                sb.Append(" from table1 where CollectTime < " + "'" + dtp_Start.Text + "'" + "order by ID desc limit 0," + dataNum);
                try
                {

                    DataTable dt1 = SQLite.CurvecsGetDataSet(sb.ToString()).Tables[0];

                    if (dt1.Rows.Count > 0)
                    {
                        this.BeginInvoke((EventHandler)delegate
                        {
                            this.chart1.ChartAreas[0].AxisX.LabelStyle.Interval = (int)(numericUpDown1.Value / 30);                //两个坐标值之间有多少个点
                            this.chart1.ChartAreas[0].AxisX.MajorGrid.Interval = (int)(numericUpDown1.Value / 20);

                            DateTime Parmeter;

                            DateTime Parmeter1;

                            chart1.Series[0].Points.Clear();

                            for (int ChartSum1 = 0; ChartSum1 < dt1.Rows.Count - 1; ChartSum1++)
                            {
                                Parmeter = DateTime.ParseExact(dt1.Rows[ChartSum1][0].ToString(), "yyyy-MM-dd HH:mm:ss:ff", null);

                                if (checkBox1.CheckState == CheckState.Checked)
                                {
                                    chart1.Series[0].Points.AddXY(Parmeter.ToOADate(), float.Parse(dt1.Rows[ChartSum1][15].ToString()));
                                }

                                if (checkBox2.CheckState == CheckState.Checked)
                                {
                                    chart1.Series[1].Points.AddXY(Parmeter.ToOADate(), float.Parse(dt1.Rows[ChartSum1][16].ToString()));
                                }

                                if (checkBox3.CheckState == CheckState.Checked)
                                {
                                    chart1.Series[2].Points.AddXY(Parmeter.ToOADate(), float.Parse(dt1.Rows[ChartSum1][17].ToString()));
                                }


                                if (checkBox4.CheckState == CheckState.Checked)
                                {
                                    chart1.Series[3].Points.AddXY(Parmeter.ToOADate(), float.Parse(dt1.Rows[ChartSum1][18].ToString()));
                                }

                                if (checkBox5.CheckState == CheckState.Checked)
                                {
                                    chart1.Series[4].Points.AddXY(Parmeter.ToOADate(), float.Parse(dt1.Rows[ChartSum1][19].ToString()));
                                }


                                if (checkBox6.CheckState == CheckState.Checked)
                                {
                                    chart1.Series[5].Points.AddXY(Parmeter.ToOADate(), float.Parse(dt1.Rows[ChartSum1][20].ToString()));
                                }

                            }

                            chartArea1.AxisY.Minimum = Ymin;
                            chartArea1.AxisY.Maximum = Ymax;

                            chart1.ChartAreas[0].AxisY.ScaleView.Size = Ymax - Ymin;

                            Parmeter1 = DateTime.ParseExact(dt1.Rows[0][0].ToString(), "yyyy-MM-dd HH:mm:ss:ff", null);
                            chart1.ChartAreas[0].AxisX.Maximum = Parmeter1.ToOADate();   //X坐标后移1秒

                            chart1.ChartAreas[0].AxisX.Minimum = Parmeter1.AddSeconds(-(int)numericUpDown1.Value).ToOADate();//此刻后10分钟作为最初X轴，

                        });

                    }
                    else
                    {
                        MessageBox.Show("时间段内无数据！");
                    }
                }
                catch
                { }
            });

        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown3.Value <= numericUpDown2.Value)
            {
                numericUpDown3.Value = numericUpDown2.Value + 1;
            }
            Ymax = (int)numericUpDown3.Value;
            string SetSavePath = Application.StartupPath + "\\HNSet\\SetSave.ini";
            IniHelper.WriteIniData("FrmHistoryCurvecs2_Set", "Y轴最大值", numericUpDown3.Value.ToString(), SetSavePath);
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown2.Value >= numericUpDown3.Value)
            {
                numericUpDown2.Value = numericUpDown3.Value - 1;
            }
            Ymin = (int)numericUpDown2.Value;
            string SetSavePath = Application.StartupPath + "\\HNSet\\SetSave.ini";
            IniHelper.WriteIniData("FrmHistoryCurvecs2_Set", "Y轴最小值", numericUpDown2.Value.ToString(), SetSavePath);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            string SetSavePath = Application.StartupPath + "\\HNSet\\SetSave.ini";
            IniHelper.WriteIniData("FrmHistoryCurvecs2_Set", "时间范围", numericUpDown1.Value.ToString(), SetSavePath);
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            string SetSavePath = Application.StartupPath + "\\HNSet\\SetSave.ini";
            IniHelper.WriteIniData("FrmHistoryCurvecs2_Set", "调整范围", numericUpDown4.Value.ToString(), SetSavePath);
        }



        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var area = chart1.ChartAreas[0];
                double xValue = area.AxisX.PixelPositionToValue(e.X);
                double yValue = area.AxisY.PixelPositionToValue(e.Y);
                // textBox1.Text = string.Format("{0:F0},{1:F0}", xValue, yValue);

                label7.Text = string.Format("{0:0.00}", yValue);
            }
            catch { }
            
        }

      
    }
}
