using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace Vultrue.Communication
{
    public partial class NetSerialPort : Component
    {
        private const int BUFFERSIZE = 1024;
        private bool isOpened;
        private byte[] bufferSerial = new byte[BUFFERSIZE];
        private byte[] bufferNet = new byte[BUFFERSIZE];
        private byte[] serialError = Encoding.ASCII.GetBytes("SerialPort Closed Error\r\n");
        private TcpListener server;
        private TcpClient client;
        private NetworkStream netstream;
        private Thread netServer;

        public NetSerialPort()
        {
            InitializeComponent();
        }

        public NetSerialPort(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        internal void Start()
        {
            string[] serialSetting = Properties.Settings.Default.SerialSetting.Split(',');
            string[] iport = Properties.Settings.Default.IPPort.Split(':');
            if (serialSetting.Length < 6 || iport.Length == 0) throw new ArgumentException();

            if (serialPort.IsOpen) serialPort.Close();
            serialPort.PortName = serialSetting[0];
            serialPort.BaudRate = int.Parse(serialSetting[1]);
            serialPort.DataBits = int.Parse(serialSetting[2]);
            serialPort.Parity = (System.IO.Ports.Parity)int.Parse(serialSetting[3]);
            serialPort.StopBits = (System.IO.Ports.StopBits)int.Parse(serialSetting[4]);
            serialPort.Handshake = (System.IO.Ports.Handshake)int.Parse(serialSetting[5]);
            serialPort.Open();

            if (iport.Length == 1) server = new TcpListener(IPAddress.Any, int.Parse(iport[0]));
            else server = new TcpListener(IPAddress.Parse(iport[0]), int.Parse(iport[1]));
            server.Start();
            isOpened = true;
            (netServer = new Thread(runNetServer)).Start();
        }

        internal void Stop()
        {
            isOpened = false;
            netstream = null;
            client.Close();
            netServer.Abort();
            server.Stop();
            serialPort.Close();
        }

        private void runNetServer()
        {
            while (isOpened)
            {
                client = server.AcceptTcpClient();
                netstream = client.GetStream();
                int i;
                while ((i = netstream.Read(bufferNet, 0, BUFFERSIZE)) != 0)
                    if (serialPort.IsOpen) serialPort.Write(bufferNet, 0, i);
                    else netstream.Write(serialError, 0, serialError.Length);
                netstream = null;
                client.Close();
            }
        }

        private void serialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            int i = serialPort.Read(bufferSerial, 0, BUFFERSIZE);
            if (netstream != null) netstream.Write(bufferSerial, 0, i);
        }
    }
}
