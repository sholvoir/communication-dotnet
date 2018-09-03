using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Vultrue.Communication {
    /// <summary>串口通信监视器</summary>
    public partial class GSMModemMonitor : UserControl {
        private enum SendMode { Text, PDU };
        private GSMModem modem;
        private SendMode sendmode = SendMode.Text;
        private EventHandler modemOpened;
        private EventHandler modemClosed;
        private EventHandler<SerialDataEventArgs> modemDataTransmitted;

        /// <summary>
        /// 构造串口通信监视器
        /// </summary>
        public GSMModemMonitor() {
            InitializeComponent();
            modemOpened = new EventHandler(modem_OpenClosed);
            modemClosed = new EventHandler(modem_OpenClosed);
            modemDataTransmitted = new EventHandler<SerialDataEventArgs>(modem_DataTransmitted);
        }

        /// <summary>
        /// 启动后执行，界面初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GSMModemMonitor_Load(object sender, EventArgs e) {
            checkBoxAutoClear_CheckedChanged(checkBoxAutoClear, e);
        }

        /// <summary>
        /// 被监视的Modem
        /// </summary>
        public GSMModem Modem {
            get { return modem; }
            set {
                if (modem != null) {
                    modem.Opened -= modemOpened;
                    modem.Closed -= modemClosed;
                    modem.DataTransmitted -= modemDataTransmitted;
                }
                modem = value;
                if (modem != null) {
                    modem.Opened += modemOpened;
                    modem.Closed += modemClosed;
                    modem.DataTransmitted += modemDataTransmitted;
                    modem_OpenClosed(modem, null);
                }
            }
        }

        /// <summary>
        /// Modem打开/关闭事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modem_OpenClosed(object sender, EventArgs e) {
            if (InvokeRequired)
                Invoke(new EventHandler(modem_OpenClosed), new object[] { sender, e });
            else {
                bool isopen = modem.IsOpen;
                buttonOpen.Enabled = !isopen;
                buttonClose.Enabled = isopen;
                buttonManual.Enabled = isopen;
                buttonTest.Enabled = isopen;
                buttonSendDirect.Enabled = isopen;
                buttonReadSMS.Enabled = isopen;
                buttonSendSMS.Enabled = isopen;
            }
        }

        /// <summary>
        /// Modem发送/接收数据处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modem_DataTransmitted(object sender, SerialDataEventArgs e) {
            if (InvokeRequired)
                Invoke(new EventHandler<SerialDataEventArgs>(modem_DataTransmitted), new object[] { sender, e });
            else dataShow.Add(e.IsReceived ? "收" : "发", e.SerialString);
        }

        /// <summary>
        /// 设置或查看Modem属性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonProperty_Click(object sender, EventArgs e) {
            new DlgGSMModemProperty(modem).ShowDialog(this);
        }

        /// <summary>
        /// 打开按钮单击处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOpen_Click(object sender, EventArgs e) {
            modem.Open();
        }

        /// <summary>
        /// 关闭按钮单击处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClose_Click(object sender, EventArgs e) {
            modem.Close();
        }

        /// <summary>
        /// 手工调度单击处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonManual_Click(object sender, EventArgs e) {
            modem.HandSchedule();
        }

        /// <summary>
        /// Modem测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonTest_Click(object sender, EventArgs e) {
            modem.TestModem();
        }

        /// <summary>
        /// 直接发送命令文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendDirect_Click(object sender, EventArgs e) {
            modem.SendDirect(textBoxCmd.Text + "\r");
            textBoxCmd.Text = "";
        }

        /// <summary>
        /// 清除Modem任务列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonTaskClear_Click(object sender, EventArgs e) {
            modem.TaskClear();
        }

        /// <summary>
        /// 是否自动清除切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxAutoClear_CheckedChanged(object sender, EventArgs e) {
            dataShow.AutoClear = checkBoxAutoClear.Checked;
        }

        /// <summary>
        /// 读取短信
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonReadSMS_Click(object sender, EventArgs e) {
            modem.ReadNotelet();
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendSMS_Click(object sender, EventArgs e) {
            if (sendmode == SendMode.PDU) modem.SendNotelet(textBoxMobileNum.Text, textBoxNoteletContent.Text, Notelet.DCS.Data);
            else modem.SendNotelet(textBoxMobileNum.Text, GSMModem.Unicode2Pdustr(textBoxNoteletContent.Text), Notelet.DCS.USC2);
        }

        /// <summary>
        /// 按文本格式发送短信
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemSendSMSText_Click(object sender, EventArgs e) {
            sendmode = SendMode.Text;
            buttonSendSMS.Text = "发送文本短信";
            buttonSendSMS_Click(sender, e);
        }

        /// <summary>
        /// 按PDU格式发送短信
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemSendSMSPDU_Click(object sender, EventArgs e) {
            sendmode = SendMode.PDU;
            buttonSendSMS.Text = "发送PDU短信";
            buttonSendSMS_Click(sender, e);
        }

        /// <summary>
        /// 发送选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendOption_Click(object sender, EventArgs e) {
            buttonSendOption.ContextMenuStrip.Show(buttonSendOption, buttonSendOption.Width / 2, buttonSendOption.Height / 2);
        }

        /// <summary>
        /// 按回车键直接发送到串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxCmd_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\r') buttonSendDirect_Click(buttonSendDirect, e);
        }

        /// <summary>
        /// 按回车键发送短信
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxNoteletContent_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\r') buttonSendSMS_Click(buttonSendSMS, e);
        }
    }
}