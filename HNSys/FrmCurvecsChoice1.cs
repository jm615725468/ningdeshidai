using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HNSys
{
    public partial class FrmCurvecsChoice1: Form
    {
        public event Action ControlSwitch1;
        public event Action ControlSwitch2;
        public event Action ControlSwitch3;
        public event Action ControlSwitch4;
        public event Action ControlSwitch5;
        public event Action ControlSwitch6;
        public event Action ControlSwitch7;
        public FrmCurvecsChoice1()
        {
            InitializeComponent();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ControlSwitch1?.Invoke();//触发事件
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ControlSwitch2?.Invoke();//触发事件
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ControlSwitch3?.Invoke();//触发事件
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ControlSwitch4?.Invoke();//触发事件
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ControlSwitch5?.Invoke();//触发事件
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ControlSwitch6?.Invoke();//触发事件
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ControlSwitch7?.Invoke();//触发事件
        }
    }
}
