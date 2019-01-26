using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ServiceProcess;
using System.ComponentModel;

namespace Vultrue.Communication
{
    static class Program
    {
        static bool isServices = false;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++) if (args[i] == "-S") isServices = true;

            if (isServices) ServiceBase.Run(new ServiceBase[] { new Service() });
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Component deamon = new Deamon();
                deamon.Disposed += (object sender, EventArgs e) => { Application.Exit(); };
                Application.Run();
            }
        }
    }
}
