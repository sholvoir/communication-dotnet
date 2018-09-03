using System;
using System.Collections.Generic;
using System.Text;

namespace Vultrue.Communication {
    /// <summary>
    /// 为Vultrue.Communication.GSMModem.Datatransmitted事件提供参数
    /// </summary>
    public class SerialDataEventArgs : EventArgs {
        private bool isReceived;
        private string serialString;

        /// <summary>
        /// 传输的数据行
        /// </summary>
        public string SerialString {
            get { return serialString; }
        }

        /// <summary>
        /// 指示该数据是否为接受的数据
        /// </summary>
        public bool IsReceived {
            get { return isReceived; }
        }

        /// <summary>
        /// 初始化类Vultrue.Communication.SerialDataEventArgs的新实例
        /// </summary>
        /// <param name="isRecieved">是否为接受的数据</param>
        /// <param name="serialString">传输的数据行</param>
        public SerialDataEventArgs(bool isReceived, string serialString) {
            this.isReceived = isReceived;
            this.serialString = serialString;
        }
    }

    /// <summary>
    /// 为Vultrue.Communication.GSMModem.CommunicationError事件提供参数
    /// </summary>
    public class CommunicationErrorEventArgs : EventArgs {
        private string message;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Message {
            get { return message; }
        }

        /// <summary>
        /// 初始化类Vultrue.Communication.CommunicationErrorEventArgs的新实例
        /// </summary>
        /// <param name="message">错误信息</param>
        public CommunicationErrorEventArgs(string message) {
            this.message = message;
        }
    }

    /// <summary>
    /// 为Vultrue.Communication.GSMModem.NoteletSended事件提供参数
    /// </summary>
    public class NoteletSendedEventArgs : EventArgs {
        private Notelet notelet;

        /// <summary>
        /// 已发送的短信
        /// </summary>
        public Notelet Notelet {
            get { return notelet; }
        }

        /// <summary>
        /// 初始化类Vultrue.Communication.NoteletSendedEventArgs的新实例
        /// </summary>
        /// <param name="notelet">已发送的短信</param>
        public NoteletSendedEventArgs(Notelet notelet) {
            this.notelet = notelet;
        }
    }

    /// <summary>
    /// 为Vultrue.Communication.GSMModem.NoteletReceived事件提供参数
    /// </summary>
    public class NoteletReceivedEventArgs : EventArgs {
        private Notelet[] notelets;

        /// <summary>
        /// 接收到的短信集合
        /// </summary>
        public Notelet[] Notelets {
            get { return notelets; }
        }

        /// <summary>
        /// 初始化类Vultrue.Communication.NoteletReceivedEventArgs的新实例
        /// </summary>
        /// <param name="notelets">接收到的短信集合</param>
        public NoteletReceivedEventArgs(Notelet[] notelets) {
            this.notelets = notelets;
        }
    }

    /// <summary>
    /// 为Vultrue.Communication.GSMModem.PhoneBookReaded事件提供参数
    /// </summary>
    public class PhoneBookReadedEventArgs : EventArgs {
        private int index;
        private string userName;
        private string phoneNum;

        /// <summary>
        /// 存储点索引
        /// </summary>
        public int Index {
            get { return index; }
        }

        /// <summary>
        /// 联系人姓名
        /// </summary>
        public string UserName {
            get { return userName; }
        }

        /// <summary>
        /// 联系人电话
        /// </summary>
        public string PhoneNum {
            get { return phoneNum; }
        }

        /// <summary>
        /// 初始化类Vultrue.Communication.PhoneBookReadedEventArgs的新实例
        /// </summary>
        /// <param name="index">存储卡位置</param>
        /// <param name="userName">联系人姓名</param>
        /// <param name="phoneNum">联系人电话</param>
        public PhoneBookReadedEventArgs(int index, string userName, string phoneNum) {
            this.index = index; this.userName = userName; this.phoneNum = phoneNum;
        }
    }

    /// <summary>
    /// 为Vultrue.Communication.GSMModem.ModemInfoReaded事件提供参数
    /// </summary>
    public class ModemInfoReadedEventArgs : EventArgs {
        private string factoryInfo;
        private string modemInfo;
        private string modemVersion;
        private string smsCenter;

        /// <summary>
        /// 厂商信息
        /// </summary>
        public string FactoryInfo {
            get { return factoryInfo; }
        }

        /// <summary>
        /// Modem信息
        /// </summary>
        public string ModemInfo {
            get { return modemInfo; }
        }

        /// <summary>
        /// Modem版本
        /// </summary>
        public string ModemVersion {
            get { return modemVersion; }
        }

        /// <summary>
        /// 短信中心号码
        /// </summary>
        public string SmsCenter {
            get { return smsCenter; }
        }

        /// <summary>
        /// 初始化类Vultrue.Communication.ModemInfoReadedEventArgs的新实例
        /// </summary>
        /// <param name="factoryInfo">厂商信息</param>
        /// <param name="modemInfo">Modem信息</param>
        /// <param name="modemVersion">Modem版本</param>
        /// <param name="smsCenter">短信中心号码</param>
        public ModemInfoReadedEventArgs(string factoryInfo, string modemInfo, string modemVersion, string smsCenter) {
            this.factoryInfo = factoryInfo;
            this.modemInfo = modemInfo;
            this.modemVersion = modemVersion;
            this.smsCenter = smsCenter;
        }
    }
}
