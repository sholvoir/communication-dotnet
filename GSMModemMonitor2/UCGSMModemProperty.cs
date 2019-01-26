using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using Microsoft.Win32;

namespace Vultrue.Communication
{
    public partial class UCGSMModemProperty : UserControl
    {
        /// <summary>
        /// 构造串口通信设置界面
        /// </summary>
        public UCGSMModemProperty()
        {
            InitializeComponent();
            //初始化组件comboBoxPortName
            RegistryKey hardware = Registry.LocalMachine.OpenSubKey("hardware");
            RegistryKey deviceMap = hardware.OpenSubKey("deviceMap");
            RegistryKey serialComm = deviceMap.OpenSubKey("serialComm");
            string[] names = serialComm.GetValueNames();
            string[] ports = new string[names.Length];
            for (int i = 0; i < names.Length; i++) ports[i] = (string)serialComm.GetValue(names[i]);
            comboBoxPortName.DataSource = ports;
            serialComm.Close();
            deviceMap.Close();
            hardware.Close();
            comboBoxBaudRate.DataSource = new int[] { 110, 300, 1200, 4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600 };
            comboBoxDataBits.DataSource = new int[] { 5, 6, 7, 8 };
            comboBoxParity.DataSource = (Parity[])Enum.GetValues(typeof(Parity));
            comboBoxStopBits.DataSource = (StopBits[])Enum.GetValues(typeof(StopBits));
            comboBoxHandshake.DataSource = (Handshake[])Enum.GetValues(typeof(Handshake));
        }

        /// <summary>
        /// 串口通信设置
        /// </summary>
        public string CommSetting
        {
            get
            {
                return string.Format("{0},{1},{2},{3},{4},{5}",
                    comboBoxPortName.SelectedItem,
                    comboBoxBaudRate.SelectedItem,
                    comboBoxDataBits.SelectedItem,
                    (int)(Parity)comboBoxParity.SelectedItem,
                    (int)(StopBits)comboBoxStopBits.SelectedItem,
                    (int)(Handshake)comboBoxHandshake.SelectedItem);
            }
            set
            {
                string[] setting = value.Split(',');
                if (setting.Length < 6) throw new ArgumentException();
                try { comboBoxPortName.SelectedItem = setting[0]; }
                catch { }
                comboBoxBaudRate.SelectedItem = int.Parse(setting[1]);
                comboBoxDataBits.SelectedItem = int.Parse(setting[2]);
                comboBoxParity.SelectedItem = (Parity)int.Parse(setting[3]);
                comboBoxStopBits.SelectedItem = (StopBits)int.Parse(setting[4]);
                comboBoxHandshake.SelectedItem = (Handshake)int.Parse(setting[5]);
            }
        }
    }
}
