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
    public partial class FrmSplash : Form, ISplashForm
    {
        public FrmSplash()
        {
            InitializeComponent();
        }

        public void SetStatusInfo(string NewStatusInfo)
        {
            lbStatusInfo.Text = NewStatusInfo;
        }
    }
}
