using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
namespace HNSys
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            string processName = Process.GetCurrentProcess().ProcessName;

            if (Process.GetProcessesByName(processName).Length > 1)
            {
                MessageBox.Show("上位机监控系统已经运行！", "系统运行", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                return;
            }
            else
            {
                Splasher.Show(typeof(FrmSplash));

                Application.Run(new FrmMain());

                //FrmLogin objLogin = new FrmLogin();
                //objLogin.TopMost = true;
                //DialogResult dr = objLogin.ShowDialog();

                //if (dr == DialogResult.OK)
                //{
                //    //登录成功，启动主窗体
                //    Splasher.Show(typeof(FrmSplash));

                //    Application.Run(new FrmMain());
                //}

            }

        }
    }
}
