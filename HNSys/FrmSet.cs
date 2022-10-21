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
    public partial class FrmSet : Form
    {
        public FrmSet()
        {
            InitializeComponent();
        }

        private void FrmSet_Load(object sender, EventArgs e)
        {
            InitControl();
        }

        private void InitControl()
        {
            DGVInit();


        }

        public void DGVInit()
        {

            DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
            headerStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;

            dataGridView2.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.ColumnHeadersDefaultCellStyle = headerStyle;
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.ClearSelection();
            dataGridView2.ReadOnly = false;


            for (int i = 0; i < 2; i++)
            {
                DataGridViewTextBoxColumn dgvc = new DataGridViewTextBoxColumn();
                if (i == 0)
                {
                    dgvc.HeaderText = "用户名";
                }
                else
                {
                    dgvc.HeaderText = "密码";
                }
                dgvc.ReadOnly = false;
                dgvc.Width = 150;
                dgvc.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                this.dataGridView2.Columns.Add(dgvc);
              
            }

            for (int i = 1; i <5; i++)
            {
                dataGridView2.Rows.Add();
            }

            for (int i = 0; i < 5; i++)
            {             
               dataGridView2.Rows[i].Cells[0].Value = CommonTags.AdminName[i];
               dataGridView2.Rows[i].Cells[1].Value = CommonTags.AdminPass[i];
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            string ConfigPath = Application.StartupPath + "\\HNSet\\User.ini";
            for (int i = 0; i < 5; i++)
            {
                INIOperationClass.INIWriteValue(ConfigPath, "AdminName_Set", i.ToString(), dataGridView2.Rows[i].Cells[0].Value.ToString());
                INIOperationClass.INIWriteValue(ConfigPath, "AdminPass_Set", i.ToString(), dataGridView2.Rows[i].Cells[1].Value.ToString());
                CommonTags.AdminName[i] = dataGridView2.Rows[i].Cells[0].Value.ToString();
                CommonTags.AdminPass[i] = dataGridView2.Rows[i].Cells[1].Value.ToString();
            }
            MessageBox.Show("保存完毕！");
        }
    }
}
