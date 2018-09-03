using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace Vultrue.Communication {
	/// <summary>通信设置对话框</summary>
	public partial class DlgGSMModemProperty : Form {

		/// <summary>
		/// 构造通信设置对话框
		/// </summary>
		/// <param name="serialSetting">要修改的串行通信设置对象</param>
		public DlgGSMModemProperty(GSMModem modem) {
			InitializeComponent();
			gsmModemProperty.Modem = modem;
		}

		/// <summary>
		/// 修改并退出
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonOK_Click(object sender, EventArgs e) {
			gsmModemProperty.ModifyModem();
			Close();
		}
	}
}