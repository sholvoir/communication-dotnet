using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;

namespace Vultrue.Communication
{
    internal delegate void DealData(int i);

    public partial class NetSerialPortTest : Form
    {
        private const int BUFFERSIZE = 1024;
        private TcpClient tcp;
        private NetworkStream stream;
        private byte[] data;
        private Thread readStream;
        private Semaphore semaphore;

        public NetSerialPortTest()
        {
            InitializeComponent();
            data = new byte[BUFFERSIZE];
            semaphore = new Semaphore(0, 1);
            tcp = new TcpClient();
            comboBoxEncoding.DataSource = Encodings.GetEnCodings();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (stream == null)
            {
                try
                {
                    string[] iport = textBoxNetSerialPort.Text.Split(':');
                    tcp.Connect(iport[0], int.Parse(iport[1]));
                    stream = tcp.GetStream();
                    (readStream = new Thread(readData)).Start();
                    buttonConnect.Text = "断开连接";
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "发生异常"); }
            }
            else
            {
                readStream.Abort();
                stream.Close();
                tcp.Close();
                stream = null;
                buttonConnect.Text = "连接";
            }
            buttonSend.Enabled = stream != null;
        }

        private void readData()
        {
            while (stream != null)
            {
                int i = stream.Read(data, 0, BUFFERSIZE);
                if (i == 0) break;
                Invoke(new DealData(dealData), new object[] { i });
                semaphore.WaitOne();
            }
        }

        private void dealData(int i)
        {
            textBoxBinary.AppendText(ByteString.GetByteString(data, 0, i) + "\r\n");
            textBoxText.AppendText(((Encoding)comboBoxEncoding.SelectedValue).GetString(data, 0, i)
                .Replace("\r", "\\r").Replace("\n", "\\n") + "\r\n");
            semaphore.Release();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            try
            {
                string text = textBoxSend.Text.Replace("\\r", "\r").Replace("\\n", "\n");
                byte[] sendata = radioButtonBinary.Checked ? ByteString.GetBytes(text) :
                    ((Encoding)comboBoxEncoding.SelectedValue).GetBytes(text);
                stream.Write(sendata, 0, sendata.Length);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "发生异常"); }
        }
    }
}
