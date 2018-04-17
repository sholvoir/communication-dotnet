using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace Vultrue.Communication {
	/// <summary>ͨ�����öԻ���</summary>
	public partial class DlgSerialPortProperty : Form {

		/// <summary>
		/// ����ͨ�����öԻ���
		/// </summary>
		/// <param name="serialSetting">Ҫ�޸ĵĴ���ͨ�����ö���</param>
		public DlgSerialPortProperty(SerialPort serialPort) {
			InitializeComponent();
			ucSerialPort.SerialPort = serialPort;
		}

		/// <summary>
		/// �޸Ĳ��˳�
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonOK_Click(object sender, EventArgs e) {
			ucSerialPort.ModifySerialPort();
			Close();
		}
	}
}