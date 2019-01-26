using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Vultrue.Communication
{
    internal enum SendMode { Text, PDU };

    /// <summary>
    /// 串口通信监视器
    /// </summary>
    public partial class UCGSMModemMonitor : UserControl
    {
        #region 变量
        private GSMModem modem;
        private SendMode sendmode = SendMode.Text;
        private EventHandler modemOpened;
        private EventHandler modemClosed;
        private EventHandler<SerialDataEventArgs> modemDataTransmitted;
        #endregion

        #region 构造

        /// <summary>
        /// 构造串口通信监视器
        /// </summary>
        public UCGSMModemMonitor()
        {
            InitializeComponent();
            modemOpened = new EventHandler(modem_OpenClosed);
            modemClosed = new EventHandler(modem_OpenClosed);
            modemDataTransmitted = new EventHandler<SerialDataEventArgs>(modem_DataTransmitted);
        }

        #endregion

        #region 属性

        /// <summary>
        /// 被监视的Modem
        /// </summary>
        public GSMModem Modem
        {
            get { return modem; }
            set
            {
                if (modem != null)
                {
                    modem.Opened -= modemOpened;
                    modem.Closed -= modemClosed;
                    modem.DataTransmitted -= modemDataTransmitted;
                }
                if ((modem = value) != null)
                {
                    modem.Opened += modemOpened;
                    modem.Closed += modemClosed;
                    modem.DataTransmitted += modemDataTransmitted;
                    modem_OpenClosed(modem, null);
                }
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 启动后执行，界面初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GSMModemMonitor_Load(object sender, EventArgs e)
        {
            checkBoxAutoClear_CheckedChanged(checkBoxAutoClear, e);
        }

        #endregion

        #region Modem事件处理

        /// <summary>
        /// Modem打开/关闭事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modem_OpenClosed(object sender, EventArgs e)
        {
            if (InvokeRequired)
                Invoke(new EventHandler(modem_OpenClosed), new object[] { sender, e });
            else
            {
                bool isopen = modem.IsOpen;
                buttonOpen.Enabled = !isopen;
                buttonClose.Enabled = isopen;
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
        private void modem_DataTransmitted(object sender, SerialDataEventArgs e)
        {
            if (InvokeRequired)
                Invoke(new EventHandler<SerialDataEventArgs>(modem_DataTransmitted), new object[] { sender, e });
            else dataShow.Add(e.IsReceived ? "收" : "发", e.SerialString);
        }

        #endregion

        #region 回车键处理

        /// <summary>
        /// 按回车键直接发送到串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxCmd_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r') buttonSendDirect_Click(buttonSendDirect, e);
        }

        /// <summary>
        /// 按回车键发送短信
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxNoteletContent_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r') buttonSendSMS_Click(buttonSendSMS, e);
        }

        #endregion

        #region 界面处理

        /// <summary>
        /// 是否自动清除切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxAutoClear_CheckedChanged(object sender, EventArgs e)
        {
            dataShow.AutoClear = checkBoxAutoClear.Checked;
        }

        /// <summary>
        /// 发送选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendOption_Click(object sender, EventArgs e)
        {
            buttonSendOption.ContextMenuStrip.Show(buttonSendOption,
                buttonSendOption.Width / 2, buttonSendOption.Height / 2);
        }

        /// <summary>
        /// 设置或查看Modem属性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonProperty_Click(object sender, EventArgs e)
        {
            new DlgGSMModemProperty(modem).ShowDialog(this);
        }

        /// <summary>
        /// 打开按钮单击处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOpen_Click(object sender, EventArgs e)
        {
            modem.Open();
        }

        /// <summary>
        /// 关闭按钮单击处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClose_Click(object sender, EventArgs e)
        {
            modem.Close();
        }

        /// <summary>
        /// 直接发送命令文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendDirect_Click(object sender, EventArgs e)
        {
            modem.SendDirect(textBoxCmd.Text.Replace("\\r", "\r").Replace("\\n", "\n"));
            textBoxCmd.Text = "";
        }

        #endregion

        #region 短信处理

        /// <summary>
        /// 读取短信
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonReadSMS_Click(object sender, EventArgs e)
        {
            try { modem.ListMessage(MessageState.ALL); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendSMS_Click(object sender, EventArgs e)
        {
            try
            {
                if (sendmode == SendMode.PDU)
                    modem.SendMessage(textBoxMobileNum.Text, textBoxNoteletContent.Text, DataCodingScheme.Data);
                else
                    modem.SendMessage(textBoxMobileNum.Text, PduString.GetPdustr(textBoxNoteletContent.Text),
                        DataCodingScheme.USC2);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        /// <summary>
        /// 按文本格式发送短信
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemSendSMSText_Click(object sender, EventArgs e)
        {
            sendmode = SendMode.Text;
            buttonSendSMS.Text = "发送文本短信";
            buttonSendSMS_Click(sender, e);
        }

        /// <summary>
        /// 按PDU格式发送短信
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemSendSMSPDU_Click(object sender, EventArgs e)
        {
            sendmode = SendMode.PDU;
            buttonSendSMS.Text = "发送PDU短信";
            buttonSendSMS_Click(sender, e);
        }

        #endregion
    }
}