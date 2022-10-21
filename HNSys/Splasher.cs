using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HNSys
{
    public class Splasher
    {
        private delegate void SplashStatusChangedHandle(string NewStatusInfo);
        private static Form m_SplashForm = null;
        private static ISplashForm m_SplashInterface = null;
        private static Thread m_SplashThread = null;
        private static string m_TempStatus = string.Empty;

        /// <summary>
        /// 显示动画窗体
        /// </summary>
        public static void Show(Type splashFormType)
        {
            if (m_SplashThread != null)
                return;
            if (splashFormType == null)
                return;

            m_SplashThread = new Thread(new ThreadStart(delegate ()
            {
                CreateInstance(splashFormType);
                Application.Run(m_SplashForm);
            }));

            m_SplashThread.IsBackground = true;
            m_SplashThread.SetApartmentState(ApartmentState.STA);
            m_SplashThread.Start();
        }



        /// <summary>
        /// 设置Loading状态
        /// </summary>
        public static string Status
        {
            set
            {
                if (m_SplashInterface == null || m_SplashForm == null)
                {
                    m_TempStatus = value;
                    return;
                }
                m_SplashForm.Invoke(
                        new SplashStatusChangedHandle(delegate (string str) { m_SplashInterface.SetStatusInfo(str); }),
                        new object[] { value }
                    );
            }
        }

        /// <summary>
        /// 关闭动画窗体
        /// </summary>
        public static void Close()
        {
            if (m_SplashThread == null || m_SplashForm == null) return;
            try
            {
                m_SplashForm.Invoke(new MethodInvoker(m_SplashForm.Close));
            }
            catch (Exception)
            {
            }
            m_SplashThread = null;
            m_SplashForm = null;
        }

        /// <summary>
        /// 创建窗体
        /// </summary>
        /// <param name="FormType"></param>
        private static void CreateInstance(Type FormType)
        {

            object obj = FormType.InvokeMember(null,
                                BindingFlags.DeclaredOnly |
                                BindingFlags.Public | BindingFlags.NonPublic |
                                BindingFlags.Instance | BindingFlags.CreateInstance, null, null, null);
            m_SplashForm = obj as Form;
            m_SplashInterface = obj as ISplashForm;
            if (m_SplashForm == null)
            {
                throw (new Exception("动画窗体必须为Form窗体"));
            }
            if (m_SplashInterface == null)
            {
                throw (new Exception("动画窗体必须继承ISplashForm"));
            }

            if (!string.IsNullOrEmpty(m_TempStatus))
                m_SplashInterface.SetStatusInfo(m_TempStatus);
        }

    }
}
