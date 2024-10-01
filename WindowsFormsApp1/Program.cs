using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace UpperApp
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        [SupportedOSPlatform("windows10.0.19041.0")]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UpperApp());
        }
    }
}
