using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HslCommunication.Profinet.Siemens;

namespace HNSys
{
   public  class PLCReadWrite
    {
        public static SiemensS7Net PLC1Connect;

        public static void PLCConnectInit()
        {
            PLC1Connect = new SiemensS7Net(SiemensPLCS.S1500);
            PLC1Connect.IpAddress = CommonTags.PLCIP;
            PLC1Connect.ConnectTimeOut = 100;
            HslCommunication.OperateResult connect = PLC1Connect.ConnectServer();
        }
   }
}
