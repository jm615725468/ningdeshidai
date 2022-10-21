using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace HNSys
{
    public  class CommonTags
    {
        public static string AarmDescribePath = Application.StartupPath + "\\HNSet\\AarmDescribe.ini";
        public static string AarmIDPath = Application.StartupPath + "\\HNSet\\AarmID.ini";
        public static string TipsDescribe1Path = Application.StartupPath + "\\HNSet\\TipsDescribe1.ini";
        public static string TipsDescribe2Path = Application.StartupPath + "\\HNSet\\TipsDescribe2.ini";
        public static string TipsID1Path = Application.StartupPath + "\\HNSet\\TipsID1.ini";
        public static string TipsID2Path = Application.StartupPath + "\\HNSet\\TipsID2.ini";

        public static string SetParamNamePath = Application.StartupPath + "\\HNSet\\ParamSetName.ini";
        public static string ShowParamNamePath = Application.StartupPath + "\\HNSet\\ParamShowName.ini";

        public static string DeviceParamNamePath = Application.StartupPath + "\\HNSet\\DeviceParameterName.ini";
        public static string FormulaParamNamePath = Application.StartupPath + "\\HNSet\\FormulaParaName.ini";
        public static string OverRollNamePath = Application.StartupPath + "\\HNSet\\OverRollName.ini";


        public static string SystemPath = Application.StartupPath + "\\HNSet\\System.ini";

        #region PLC
        public static string PLCIP;
        public static int PLCPort;
        public static int PLCOnline =999;

        public static float[] PLC1_REAL = new float[473];
        public static float[] PLC1_REAL1 = new float[161];
        public static float[] PLC1_REAL2 = new float[556];
        public static float[] PLC1_REAL4 = new float[84];
        public static float[] PLC1_REAL5 = new float[84];
        public static float[] PLC1_REAL6 = new float[32];

        public static bool[] PLC1_BOOL = new bool[528];
        public static bool[] PLC1_BOOL1 = new bool[1304];

        public static string PLC1_String ="";
        public static bool StoryFlag;


        public static bool RunFlag;
        public static bool StopFlag;
        public static string ProductCode;

        public static string[] AlarmInformation = new string[528];
        public static string[] TipsInformation = new string[1304];
        public static string[] AlarmID = new string[528];
        public static string[] TipsID = new string[1304];

        public static string[] DeviceParameter = new string[100];

        public static string[] CompleteMachine_PShow = new string[100];
        public static string[] Unwind_PShow = new string[100];






        public static string[] MachineHeadA_PShow = new string[100];

        public static string[] OvenA_PShow = new string[100];

        public static string[] DragA_PShow = new string[100];

        public static string[] MachineHeadB_PShow = new string[100];

        public static string[] OvenB_PShow = new string[100];

        public static string[] DragB_PShow = new string[100];

        public static string[] BakingOven_PShow = new string[100];

        public static string[] BakingDrag_PShow = new string[100];

        public static string[] DragD_PShow = new string[100];

        public static string[] Wind_PShow = new string[100];

        public static string[] InfraredOvenA_PShow = new string[100];

        public static string[] InfraredOvenB_PShow = new string[100];

        public static string[] CompleteMachine_PSet = new string[100];

        public static string[] Unwind_PSet = new string[100];

        public static string[] MachineHeadA_PSet = new string[100];

        public static string[] OvenA_PSet = new string[100];

        public static string[] DragA_PSet = new string[100];

        public static string[] MachineHeadB_PSet = new string[100];

        public static string[] OvenB_PSet = new string[100];

        public static string[] DragB_PSet = new string[100];

        public static string[] BakingOven_PSet = new string[100];

        public static string[] BakingDrag_PSet = new string[100];

        public static string[] DragD_PSet = new string[100];

        public static string[] Wind_PSet = new string[100];

        public static string[] InfraredOvenA_PSet = new string[100];

        public static string[] InfraredOvenB_PSet = new string[100];

        public static string[] FormulaParam = new string[215];

        #endregion

        public static string[] DeviceParameterName = new string[325];
        public static string LocalLoginName ="未登录";

        public static string[] OverRollName = new string[168];

        public static string[] AdminName = new string[5];
        public static string[] AdminPass = new string[5];



    }


 }
