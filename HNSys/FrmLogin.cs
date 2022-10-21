using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using INIOpration;
namespace HNSys
{
    public partial class FrmLogin : Form
    {
        Dictionary<string, string> RoleDic = new Dictionary<string, string>();
        Dictionary<string, string> modeDic = new Dictionary<string, string>();

        //public string[] AdminName = new string[5];
        //public string[] AdminPass = new string[5];
        public string ConfigPath = Application.StartupPath + "\\HNSet\\User.ini";
        public FrmLogin()
        {           
            InitializeComponent();
        }

        private void FrmLogin_Load(object sender, EventArgs e)
        {
            Splasher.Status = "正在加载系统配置文件......";

            for (int j = 0; j < 5; j++)
            {
                CommonTags.AdminName[j] = INIOperationClass.INIGetStringValue(ConfigPath, "AdminName_Set", j.ToString(), null);
                CommonTags.AdminPass[j] = INIOperationClass.INIGetStringValue(ConfigPath, "AdminPass_Set", j.ToString(), null);
            }
        
        }


        private void button1_Click(object sender, EventArgs e)
        {        
            if (txt_ID.Text != "")
            {
                for (int i = 0; i < 5; i++)
                {
                    if (txt_ID.Text == CommonTags.AdminName[i])
                    {
                        if (txt_Pwd.Text == CommonTags.AdminPass[i])
                        {
                            CommonTags.LocalLoginName = txt_ID.Text;
                           
                            this.DialogResult = DialogResult.OK;
                            break;
                        }                       
                        else
                        {
                            MessageBox.Show("密码错误");
                            break;
                        }
                    }
                    else
                    {
                        if (i >= 6)
                        {
                            MessageBox.Show("账号错误");
                        }

                    }
                }
            }
            else
            {
                MessageBox.Show("账号不能为空");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
