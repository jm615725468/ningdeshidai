using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNSys.Common
{
    class Record
    {
        #region tabControl
        //tabControl1.TabPages的隐藏与显现
        // this.tabControl1.TabPages.Remove(tabPage6);
        //this.tabControl1.TabPages.Add(tabPage6);
        //TabPages的切换tabControl1.SelectedTab = tabPage7;
        #endregion

        #region textBox
        //textBox4.Focus();//光标定位




        #endregion

        #region GroupBox
        // groupBox15.Enabled = false;//groupBox15内的控件不可操作

        #endregion

        #region 字符串操作
        //comboBox2.Text.Contains("=")//判断字符串中是否含有“=”
        //string aa = comboBox2.Text.Substring(0, comboBox2.Text.IndexOf('='));//判断“=”前是否有字符串
        //string bb = comboBox2.Text.Split('=')[1];//判断“=”后是否有字符串
        #endregion

        #region INI文件配置
        // textBox29.Text = INIOperationClass.INIGetStringValue(ConfigPath, "URLName_Set", "Heartbeat", null);//读取单一键值

        //获取节点内的所有键和键值
        //UserPass1 = INIOperationClass.INIGetAllItems(ConfigPath, "Job2_Set");
        //for (int i = 0; i < 10; i++)
        //{
        //    comboBox2.Items.Add(UserPass1[i]);
        //}


        #endregion

        #region 文件操作
        //文件夹存在判断，不存在创建
        //if (!Directory.Exists(Path.Combine(tempString, "XT")))
        //      Directory.CreateDirectory(Path.Combine(tempString, "XT"));

        //  if (!Directory.Exists(Path.Combine(tempString + "\\XT", "CONFIG")))
        //      Directory.CreateDirectory(Path.Combine(tempString + "\\XT", "CONFIG"));

        #endregion

        #region 时间处理

        //获取两个时间点的时间差
        //DateTime date1 = new DateTime();
        //DateTime date2 = new DateTime();
        //TimeSpan timeSpan = date2 - date1;
        //Console.WriteLine("两次时间相差" + timeSpan.TotalMinutes + "分钟");
        #endregion







    }
}
