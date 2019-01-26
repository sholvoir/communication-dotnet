using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Vultrue.Communication
{
    public partial class SeiralTest : Form
    {
        public SeiralTest()
        {
            InitializeComponent();
            serialPort.Open();
            serialPort.Write("AT\r\n");
        }

        private void serialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {

        }
    }
}
