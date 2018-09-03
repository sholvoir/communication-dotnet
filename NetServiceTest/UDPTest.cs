using System;
using System.Windows.Forms;

namespace Vultrue.Communication
{
    public partial class UDPTest : Form
    {
        public UDPTest()
        {
            InitializeComponent();
        }

        private void buttonSetting_Click(object sender, EventArgs e)
        {
            udp.LocalIPPort = textBoxLocal.Text;
            udp.DefaultRemote = textBoxRemote.Text;
            udp.Open();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            udp.Close();
        }

        void udp_DataReceived(object sender, DataTransEventArgs e)
        {
            if (InvokeRequired)
                Invoke(new EventHandler<DataTransEventArgs>(udp_DataReceived), new object[] { sender, e });
            else
                textBoxReceived.Text += string.Format("{0}:{1}\t{2}\r\n", e.IP, e.Port, e.Message);
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            udp.Send(textBoxSend.Text);
        }
    }
}
