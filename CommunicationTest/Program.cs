using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Vultrue.Communication
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            byte[] b = new byte[] {0x02, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00 };
            ushort s = ByteString.CRC16(b, 0, 6, out b[6], out b[7]);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new UDPTest());
        }
    }
}
