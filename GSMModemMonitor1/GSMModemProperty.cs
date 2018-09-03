using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using Microsoft.Win32;

namespace Vultrue.Communication {
	public partial class GSMModemProperty : UserControl {
		private GSMModem modem;

		/// <summary>
		/// ���촮��ͨ�����ý���
		/// </summary>
		public GSMModemProperty() {
			InitializeComponent();
			//��ʼ�����comboBoxPortName
			RegistryKey hardware = Registry.LocalMachine.OpenSubKey("hardware");
			RegistryKey deviceMap = hardware.OpenSubKey("deviceMap");
			RegistryKey serialComm = deviceMap.OpenSubKey("serialComm");
			string[] names = serialComm.GetValueNames();
			for (int i = 0; i < names.Length; i++)
				this.comboBoxPortName.Items.Add((string)serialComm.GetValue(names[i]));
			serialComm.Close();
			deviceMap.Close();
			hardware.Close();
			//��ʼ�����comboBoxBaudRate
			int[] baudRates = new int[] { 110, 300, 1200, 4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600 };
			for (int i = 0; i < baudRates.Length; i++) comboBoxBaudRate.Items.Add(baudRates[i]);
			//��ʼ�����comboBoxDataBits
			int[] dataBits = new int[] { 5, 6, 7, 8 };
			for (int i = 0; i < dataBits.Length; i++) comboBoxDataBits.Items.Add(dataBits[i]);
			//��ʼ�����comboBoxParity
			Parity[] parities = (Parity[])Enum.GetValues(typeof(Parity));
			for (int i = 0; i < parities.Length; i++) comboBoxParity.Items.Add(parities[i]);
			//��ʼ�����comboBoxStopBits
			StopBits[] stopBits = (StopBits[])Enum.GetValues(typeof(StopBits));
			for (int i = 0; i < stopBits.Length; i++) comboBoxStopBits.Items.Add(stopBits[i]);
			//��ʼ�����comboBoxHandshake
			Handshake[] handshakes = (Handshake[])Enum.GetValues(typeof(Handshake));
			for (int i = 0; i < handshakes.Length; i++) comboBoxHandshake.Items.Add(handshakes[i]);
		}

		/// <summary>
		/// ����ͨ������
		/// </summary>
		public GSMModem Modem {
			set {
				modem = value;
				comboBoxPortName.SelectedItem = modem.PortName;
				comboBoxBaudRate.SelectedItem = modem.BaudRate;
				comboBoxDataBits.SelectedItem = modem.DataBits;
				comboBoxParity.SelectedItem = modem.Parity;
				comboBoxStopBits.SelectedItem = modem.StopBits;
				comboBoxHandshake.SelectedItem = modem.Handshake;
			}
		}

		/// <summary>
		/// �޸Ĵ���ͨ������
		/// </summary>
		public void ModifyModem() {
			bool isopen = modem.IsOpen;
			if (isopen) { modem.Close(); }
			modem.PortName = this.comboBoxPortName.Text;
			modem.BaudRate = (int)this.comboBoxBaudRate.SelectedItem;
			modem.DataBits = (int)this.comboBoxDataBits.SelectedItem;
			modem.Parity = (Parity)this.comboBoxParity.SelectedItem;
			modem.StopBits = (StopBits)this.comboBoxStopBits.SelectedItem;
			modem.Handshake = (Handshake)this.comboBoxHandshake.SelectedItem;
			if (isopen) modem.Open();
		}
	}
}
