using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO.Ports;
using System.Globalization;
using System.Timers;

namespace Vultrue.Communication
{
    public partial class SerialPortDebugger : Form
    {
        private enum SendMode { Ascii, Hex };
        private List<byte> receiveLine = new List<byte>();
        private SendMode sendmode = SendMode.Ascii;
        private string autoSendText;

        /// <summary>
        /// 构造一个串口调试器
        /// </summary>
        public SerialPortDebugger()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 串口属性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonProperty_Click(object sender, EventArgs e)
        {
            new DlgSerialPortProperty(serialPort).ShowDialog(this);
        }

        /// <summary>
        /// 打开/关闭串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonStartStop_Click(object sender, EventArgs e)
        {
            bool isopen = serialPort.IsOpen;

            if (isopen)
                serialPort.Close();
            else
            {
                try
                {
                    serialPort.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "串口打开失败");
                    return;
                }
            }

            isopen = serialPort.IsOpen;
            buttonStartStop.Image = isopen ? Properties.Resources.Stop : Properties.Resources.Start;
            buttonSendOption.Enabled = isopen;
            buttonSend.Enabled = isopen;

        }

        /// <summary>
        /// 清除接收区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClearReceived_Click(object sender, EventArgs e)
        {
            dataShowReceived.Clear();
        }

        /// <summary>
        /// 清除发送区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClearSended_Click(object sender, EventArgs e)
        {
            dataShowSended.Clear();
        }

        /// <summary>
        /// 显示接收的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void receivedDataDisplay_CheckedChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataShowReceived.Rows)
            {
                NameValue r = (NameValue)row.DataBoundItem;
                if (radioButtonDisplayHex.Checked)
                    r.Value = ByteString.GetDisplayString((byte[])r.Tag);
                else r.Value = Encoding.Default.GetString((byte[])r.Tag);
            }
        }

        /// <summary>
        /// 显示发送的数据
        /// </summary>
        private void sendedDataDisplay_CheckedChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataShowSended.Rows)
            {
                NameValue r = (NameValue)row.DataBoundItem;
                if (radioButtonDisplayHex.Checked)
                    r.Value = ByteString.GetDisplayString((byte[])r.Tag);
                else r.Value = Encoding.Default.GetString((byte[])r.Tag);
            }
        }

        /// <summary>
        /// 从串口接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (InvokeRequired)
                Invoke(new SerialDataReceivedEventHandler(serialPort_DataReceived), new object[] { sender, e });
            else
            {
                //timerModbus.Stop();
                while (serialPort.BytesToRead > 0) receiveLine.Add((byte)serialPort.ReadByte());
                //timerModbus.Interval = (double)40000 / serialPort.BaudRate;
                //timerModbus.Start();
            }
        }

        /// <summary>
        /// 数据行结束处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerModbus_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (InvokeRequired)
                Invoke(new ElapsedEventHandler(timerModbus_Elapsed), new object[] { sender, e });
            else
            {
                byte[] received = receiveLine.ToArray();
                receiveLine.Clear();
                if (radioButtonDisplayHex.Checked)
                    dataShowReceived.Add(DateTime.Now.ToString(), ByteString.GetDisplayString(received), received);
                else dataShowReceived.Add(DateTime.Now.ToString(),
                    Encoding.Default.GetString(received).Replace("\r", "\\r").Replace("\n", "\\n"), received);
            }
        }

        /// <summary>
        /// 发送数据到串口
        /// </summary>
        /// <param name="text"></param>
        private void send(string text)
        {
            try
            {
                byte[] data = sendmode == SendMode.Hex ? ByteString.GetBytes(text.Replace(" ", "")) :
                    Encoding.ASCII.GetBytes(text.Replace("\\r", "\r").Replace("\\R", "\r").Replace("\\n", "\n").Replace("\\N", "\n"));
                serialPort.Write(data, 0, data.Length);
                if (radioButtonAsHex.Checked)
                    dataShowSended.Add(DateTime.Now.ToString(), ByteString.GetDisplayString(data), data);
                else dataShowSended.Add(DateTime.Now.ToString(), Encoding.ASCII.GetString(data), data);
                textBoxSend.Text = "";
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "发生错误"); }
        }

        /// <summary>
        /// 发送Button单击处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSend_Click(object sender, EventArgs e)
        {
            send(textBoxSend.Text);
        }

        /// <summary>
        /// 自动发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerAutoSend_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (InvokeRequired)
                Invoke(new System.Timers.ElapsedEventHandler(timerAutoSend_Elapsed), new object[] { sender, e });
            else send(autoSendText);
        }

        /// <summary>
        /// 显示发送选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendOption_Click(object sender, EventArgs e)
        {
            buttonSendOption.ContextMenuStrip.Show(buttonSendOption, buttonSendOption.Width / 2, buttonSendOption.Height / 2);
        }

        /// <summary>
        /// 将输入数据作为16进制数发送到串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemSendBinary_Click(object sender, EventArgs e)
        {
            sendmode = SendMode.Hex;
            buttonSend.Text = "发送16进制";
            send(textBoxSend.Text);
        }

        /// <summary>
        /// 将输入数据作为ASCII码数发送到串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemSendAscii_Click(object sender, EventArgs e)
        {
            sendmode = SendMode.Ascii;
            buttonSend.Text = "发送ASCII";
            send(textBoxSend.Text);
        }

        /// <summary>
        /// 周期地将输入数据作为16进制数发送到串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemAutoSendBinary_Click(object sender, EventArgs e)
        {
            try
            {
                timer.Interval = double.Parse(textBoxAutoSendPeriod.Text);
                setSendEnables(false);
                sendmode = SendMode.Hex;
                buttonSend.Text = "发送16进制";
                autoSendText = textBoxSend.Text;
                timer.Start();
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "发生错误"); }
        }

        /// <summary>
        /// 周期地将输入数据作为ASCII码数发送到串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemAutoSendAscii_Click(object sender, EventArgs e)
        {
            try
            {
                timer.Interval = double.Parse(textBoxAutoSendPeriod.Text);
                setSendEnables(false);
                sendmode = SendMode.Ascii;
                buttonSend.Text = "发送ASCII";
                autoSendText = textBoxSend.Text;
                timer.Start();
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "发生错误"); }
        }

        /// <summary>
        /// 停止周期性发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemStop_Click(object sender, EventArgs e)
        {
            timer.Stop();
            setSendEnables(true);
        }

        /// <summary>
        /// 处理不同状态的界面
        /// </summary>
        /// <param name="isManual"></param>
        private void setSendEnables(bool isManual)
        {
            textBoxAutoSendPeriod.Enabled = isManual;
            buttonSend.Enabled = isManual;
            menuItemSendBinary.Enabled = isManual;
            menuItemSendAscii.Enabled = isManual;
            menuItemAutoSendBinary.Enabled = isManual;
            menuItemAutoSendAscii.Enabled = isManual;
            menuItemStop.Enabled = !isManual;
        }

        /// <summary>
        /// 按回车键发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxSend_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' && serialPort.IsOpen) send(textBoxSend.Text + "\r\n");
        }
    }
}