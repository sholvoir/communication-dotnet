using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Vultrue.Communication
{
    /// <summary>
    /// HUAWEI CDMA2000 1X Modem Special AT Command Set ^HFEEPO:1,0,0,0
    /// </summary>
    public class CdmaModem_Huawei : CdmaModem
    {
        #region Static Var
        private static Regex regexSystemStart = new Regex("^SYSSTART");
        private static Regex regexMode = new Regex("^\\^MODE:\\s*(?<sys_mode>\\d+)");
        private static Regex regexRssiLevel = new Regex("^\\^RSSILVL:\\s*(?<rssi>\\d+)");
        private static Regex regexOrigin = new Regex("^\\^ORIG:\\s*(?<call_x>\\d+),(?<call_type>\\d+)");
        private static Regex regexConn = new Regex("^\\^CONN:\\s*(?<call_x>\\d+),(?<call_type>\\d+)");
        private static Regex regexCharging = new Regex("^\\^HFEEPO:\\s*(?<f1>\\d+),(?<f2>\\d+),(?<f3>\\d+),(?<f4>\\d+)");
        private static Regex regexCallEnd = new Regex("^\\^CEND:\\s*(?<call_x>\\d+),(?<duration>\\d+),(?<end_status>\\d+)");
        private static Regex regexMessageSendSuccess = new Regex("^\\^HCMGSS:(?<mr>\\d+)");
        private static Regex regexMessageSendFailed = new Regex("^\\^HCMGSF:(?<err>\\d+)");
        #endregion

        #region EventHandler

        /// <summary>
        /// MT Start Prompt
        /// </summary>
        public event EventHandler SystemStarted;

        /// <summary>
        /// 表示将处理 Vultrue.Communication.Modem 对象的来电事件的方法。
        /// </summary>
        public event EventHandler<ModeChangedEventArgs> ModeChanged;

        /// <summary>
        /// 表示将处理 Vultrue.Communication.Modem 对象的RSSI Over Threshold的 方法。
        /// </summary>
        public event EventHandler<RssiChangedEventArgs> RssiChanged;

        /// <summary>
        /// 电话发起事件
        /// </summary>
        public event EventHandler<CallEventArgs> CallOrigin;

        /// <summary>
        /// 电话连接事件
        /// </summary>
        public event EventHandler<CallEventArgs> CallConn;

        /// <summary>
        /// 电话计费事件
        /// </summary>
        public event EventHandler CallCharging;

        /// <summary>
        /// 电话挂断事件
        /// </summary>
        public event EventHandler<CallEndEventArgs> CallEnd;

        /// <summary>
        /// Message Send Success
        /// </summary>
        public event EventHandler<MessageSendSuccessEventArgs> MessageSendSuccess;

        /// <summary>
        /// Message Send Failed
        /// </summary>
        public event EventHandler<MessageSendFailedEventArgs> MessageSendFailed;

        #endregion

        #region Generic

        /// <summary>
        /// Reset MT after (delay) s
        /// </summary>
        /// <param name="delay"></param>
        public void MTReset(int delay)
        {
            lock (mt) ExecTask(string.Format("AT^RESET={0}\r", delay), "");
        }

        /// <summary>
        /// 关闭Modem
        /// </summary>
        public void MTPowerOff()
        {
            lock (mt) ExecTask("AT^MSO\r", "");
        }

        /// <summary>
        /// 主动上报控制
        /// </summary>
        /// <returns>0:关闭主动上报; 1:开启主动上报</returns>
        public int GetAutoReport()
        {
            lock (mt) return int.Parse(ExecTask("AT^CURC?\r", "^\\^CURC?:\\s*(?<ans>\\d+)")[0].Result("${ans}"));
        }

        /// <summary>
        /// 主动上报控制
        /// </summary>
        /// <param name="mode">0:关闭主动上报; 1:开启主动上报</param>
        public void SetAutoReport(int mode)
        {
            lock (mt) ExecTask(string.Format("AT^CURC={0}\r", mode), "");
        }

        #endregion

        #region Call Control Command

        /// <summary>
        /// 发起语音呼叫
        /// </summary>
        /// <param name="phoneNumber">对端电话号码</param>
        public void DialVoice(string phoneNumber)
        {
            lock (mt) ExecTask(string.Format("AT+CDV{0}\r", phoneNumber), "");
        }

        /// <summary>
        /// 挂机
        /// </summary>
        public void HangupVoice()
        {
            lock (mt) ExecTask("AT+CHV\r", "");
        }

        /// <summary>
        /// 接电话
        /// </summary>
        public void AnswerCall()
        {
            lock (mt) ExecTask("ATA\r", "");
        }

        /// <summary>
        /// 切换语音通道
        /// </summary>
        /// <param name="n">通道号(0/1)</param>
        public void SwichVoicePath(int n)
        {
            lock (mt) ExecTask(string.Format("AT^SWSPATH={0}\r", n), "");
        }

        #endregion

        #region NETWORK SERVICE COMMANDS

        /// <summary>
        /// RSSI Report
        /// </summary>
        /// <returns>0:No Report或1:Auto Report</returns>
        public int GetRssiReport()
        {
            lock (mt) return int.Parse(ExecTask("AT^RSSIREP?\r", "^\\^RSSIREP:\\s*(?<ans>\\d+)")[0].Result("${ans}"));
        }

        /// <summary>
        /// Set RSSI Auto Report
        /// </summary>
        /// <param name="rep">0:No Report或1:Auto Report</param>
        public void SetRssiReport(int rep)
        {
            lock (mt) ExecTask(string.Format("AT^RSSIREP={0}\r", rep), "");
        }

        #endregion

        #region Small Message Command

        /// <summary>
        /// 读取短信
        /// </summary>
        /// <param name="index">短信所在存储空间的索引</param>
        /// <returns>读取的短信</returns>
        public Message ReadTextMessage(int index)
        {
            lock (mt)
            {
                free = 0;
                ClearBuffer();
                string instruction = string.Format("AT^HCMGR={0}\r", index);
                SendCmd(instruction);
                Regex regex = new Regex("^\\^HCMGR:\\s*(?<callerID>\\d+),(?<year>\\d+),(?<month>\\d+),(?<day>\\d+)," +
                    "(?<hour>\\d+),(?<minute>\\d+),(?<second>\\d+),(?<lang>\\d+),(?<format>\\d+),(?<length>\\d+)," +
                    "(?<prt>\\d+),(?<prv>\\d+),(?<type>\\d+),(?<stat>\\d+)");
                Match match;
                string line;
                for (; ; )
                {
                    line = ReadLine();
                    if (RegexError1.Match(line).Success)
                        throw new ModemErrorException(instruction + " 执行错误");
                    if ((match = RegexError2.Match(line)).Success)
                        throw new ModemErrorException(match.Result("${errinfo}"), instruction + " 执行错误");
                    if ((match = regex.Match(line)).Success) break;
                    else UnsolicitedDeal(line);
                }
                if ((MessageFormat)int.Parse(match.Result("${format}")) == MessageFormat.UNICODE)
                {
                    SerialPort.Encoding = Encoding.BigEndianUnicode;
                    line = SerialPort.ReadTo("\u001A");
                    SerialPort.Encoding = Encoding.ASCII;
                }
                else
                    line = SerialPort.ReadTo("\u001A");
                Log("收\t" + line);
                for (; ; )
                {
                    string str = ReadLine();
                    if (RegexOK.Match(str).Success) break;
                    else UnsolicitedDeal(str);
                }
                return new Message(index,
                    (MessageState)int.Parse(match.Result("${stat}")),
                    match.Result("${callerID}"),
                    string.Format("{0},{1},{2},{3},{4},{5}",
                        match.Result("${year}"),
                        match.Result("${month}"),
                        match.Result("${day}"),
                        match.Result("${hour}"),
                        match.Result("${minute}"),
                        match.Result("${second}")),
                    line);
            }
        }

        /// <summary>
        /// 列出短信
        /// </summary>
        /// <param name="state">筛选短信的状态</param>
        /// <returns>短信索引列表</returns>
        public Tuple<int, MessageState>[] ListMessage(MessageState state)
        {
            lock (mt)
            {
                List<Match> matchs = ExecTask(string.Format("AT^HCMGL={0}\r", (int)state), "^\\^HCMGL:\\s*(?<index>\\d+),(?<stat>\\d+)");
                Tuple<int, MessageState>[] messageIndexes = new Tuple<int, MessageState>[matchs.Count];
                for (int i = 0; i < matchs.Count; i++)
                    messageIndexes[i] = new Tuple<int, MessageState>(int.Parse(matchs[i].Result("${index}")), (MessageState)int.Parse(matchs[i].Result("${stat}")));
                return messageIndexes;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mp"></param>
        public void SetMessageParameter(MessageParameter mp)
        {
            lock (mt) ExecTask(string.Format("AT^HSMSSS={0},{1},{2},{3}\r",
                 (int)mp.ACK, (int)mp.Priority, (int)mp.Format, (int)mp.Preserve), "");
        }

        /// <summary>
        /// 以文本模式发送发送短信
        /// </summary>
        /// <param name="mobileNum">发送的短信目标号码</param>
        /// <param name="msg">短信内容文本</param>
        /// <param name="encoding">短信编码方式</param>
        /// <returns></returns>
        public void SendTextMessage(string mobileNum, string msg, Encoding encoding)
        {
            lock (mt)
            {
                free = 0;
                ClearBuffer();
                string instruction = string.Format("AT^HCMGS=\"{0}\"\r", mobileNum);
                SendCmd(instruction);
                Thread.Sleep(200);
                string line = SerialPort.ReadExisting();
                UnsolicitedDeal(line);
                if (RegexPrompt.Match(line).Success)
                {
                    Log("收\t" + line.Replace("\r\n", ""));
                    msg += "\u001A";
                    byte[] data = encoding.GetBytes(msg);
                    SerialPort.Write(data, 0, data.Length);
                    Log("发\t" + msg);
                    for (int i = 0; i < 20; i++)
                    {
                        string str = ReadLine();
                        if (RegexOK.Match(str).Success) break;
                        else UnsolicitedDeal(str);
                    }
                }
                else
                {
                    Log("收\t" + line);
                    SerialPort.Write("\u001B");
                    if (RegexOK.Match(line).Success || regexMessageSendFailed.Match(line).Success || RegexError1.Match(line).Success)
                        throw new ModemErrorException(instruction + " 执行错误");
                    Match match = RegexError2.Match(line);
                    if (match.Success)
                        throw new ModemErrorException(match.Result("${errinfo}"), instruction + " 执行错误");
                    return;
                }
            }
        }

        #endregion

        #region Unslicited

        /// <summary>
        /// 主动上报数据处理
        /// </summary>
        /// <param name="line"></param>
        /// <returns>匹配正确返回True，不正确返回False</returns>
        protected override bool UnsolicitedDeal(string line)
        {
            if (base.UnsolicitedDeal(line)) return true;
            Match match;
            if ((match = regexSystemStart.Match(line)).Success)
            {
                if (SystemStarted != null)
                    new Thread(() => { SystemStarted(this, EventArgs.Empty); }).Start();
                return true;
            }
            if ((match = regexMode.Match(line)).Success)
            {
                if (ModeChanged != null)
                    new Thread(() => { ModeChanged(this, new ModeChangedEventArgs(int.Parse(match.Result("${sys_mode}")))); }).Start();
                return true;
            }
            if ((match = regexRssiLevel.Match(line)).Success)
            {
                if (RssiChanged != null)
                    new Thread(() => { RssiChanged(this, new RssiChangedEventArgs(int.Parse(match.Result("${ans}")))); }).Start();
                return true;
            }
            if ((match = regexMessageSendSuccess.Match(line)).Success)
            {
                if (MessageSendSuccess != null)
                    new Thread(() => { MessageSendSuccess(this, new MessageSendSuccessEventArgs(int.Parse(match.Result("${mr}")))); }).Start();
                return true;
            }
            if ((match = regexMessageSendFailed.Match(line)).Success)
            {
                if (MessageSendFailed != null)
                    new Thread(() => { MessageSendFailed(this, new MessageSendFailedEventArgs(int.Parse(match.Result("${err}")))); }).Start();
                return true;
            }
            if ((match = regexOrigin.Match(line)).Success)
            {
                if (CallOrigin != null)
                    new Thread(() => { CallOrigin(this, new CallEventArgs(int.Parse(match.Result("${call_x}")), int.Parse(match.Result("${call_type}")))); }).Start();
                return true;
            }
            if ((match = regexConn.Match(line)).Success)
            {
                if (CallConn != null)
                    new Thread(() => { CallConn(this, new CallEventArgs(int.Parse(match.Result("${call_x}")), int.Parse(match.Result("${call_type}")))); }).Start();
                return true;
            }
            if ((match = regexCharging.Match(line)).Success)
            {
                if (CallCharging != null)
                    new Thread(() => { CallCharging(this, EventArgs.Empty); }).Start();
                return true;
            }
            if ((match = regexCallEnd.Match(line)).Success)
            {
                if (CallEnd != null)
                    new Thread(() => { CallEnd(this, new CallEndEventArgs(int.Parse(match.Result("${call_x}")), int.Parse(match.Result("${duration}")), int.Parse(match.Result("${end_status}")))); }).Start();
                return true;
            }
            return false;
        }

        #endregion
    }

    #region EventArgs

    /// <summary>
    /// 
    /// </summary>
    public class ModeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public int Mode { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        public ModeChangedEventArgs(int mode) { Mode = mode; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RssiChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public int Rssi { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rssi"></param>
        public RssiChangedEventArgs(int rssi) { Rssi = rssi; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CallEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public int CallId { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public int CallType { get; private set;}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="callType"></param>
        public CallEventArgs(int callId, int callType) { CallId = callId; CallType = callType; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CallEndEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public int CallId { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public int Duration { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public int EndStatus { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="duration"></param>
        /// <param name="endStatus"></param>
        public CallEndEventArgs(int callId, int duration, int endStatus)
        {
            CallId = callId;
            Duration = duration;
            EndStatus = endStatus;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MessageSendSuccessEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public int MessageID { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageID"></param>
        public MessageSendSuccessEventArgs(int messageID) { MessageID = messageID; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MessageSendFailedEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public int ErrorCode { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        public MessageSendFailedEventArgs(int errorCode) { ErrorCode = errorCode; }
    }

    #endregion
}
