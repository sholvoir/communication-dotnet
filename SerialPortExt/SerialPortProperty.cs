using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace Vultrue.Communication {
	public partial class SerialPortProperty : UserControl {
		private SerialPort serialPort;

		/// <summary>
		/// 构造串口通信设置界面
		/// </summary>
		public SerialPortProperty() {
			InitializeComponent();
            foreach (int baudRate in new int[] { 110, 300, 1200, 4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600 })
                comboBoxBaudRate.Items.Add(baudRate);
			foreach (int dataBit in new int[] { 5, 6, 7, 8 })
                comboBoxDataBits.Items.Add(dataBit);
            foreach (Parity paritie in (Parity[])Enum.GetValues(typeof(Parity)))
                comboBoxParity.Items.Add(paritie);
            foreach (StopBits stopBits in (StopBits[])Enum.GetValues(typeof(StopBits)))
                comboBoxStopBits.Items.Add(stopBits);
            foreach (Handshake handshake in (Handshake[])Enum.GetValues(typeof(Handshake)))
                comboBoxHandshake.Items.Add(handshake);
		}

		/// <summary>
		/// 串口通信设置
		/// </summary>
		public SerialPort SerialPort {
			set {
				serialPort = value;
				textBoxPortName.Text = serialPort.PortName;
				comboBoxBaudRate.SelectedItem = serialPort.BaudRate;
				comboBoxDataBits.SelectedItem = serialPort.DataBits;
				comboBoxParity.SelectedItem = serialPort.Parity;
				comboBoxStopBits.SelectedItem = serialPort.StopBits;
				comboBoxHandshake.SelectedItem = serialPort.Handshake;
			}
		}

		/// <summary>
		/// 修改串口通信设置
		/// </summary>
		public void ModifySerialPort() {
			bool isopen = serialPort.IsOpen;
			if (isopen) { serialPort.Handshake = Handshake.None; serialPort.Close(); }
			serialPort.PortName = textBoxPortName.Text;
			serialPort.BaudRate = (int)comboBoxBaudRate.SelectedItem;
			serialPort.DataBits = (int)comboBoxDataBits.SelectedItem;
			serialPort.Parity = (Parity)comboBoxParity.SelectedItem;
			serialPort.StopBits = (StopBits)comboBoxStopBits.SelectedItem;
			serialPort.Handshake = (Handshake)comboBoxHandshake.SelectedItem;
			if (isopen) serialPort.Open();
		}
	}
}
