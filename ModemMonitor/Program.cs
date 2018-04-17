using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Vultrue.Communication
{
    class Program
    {
        static AutoResetEvent W;

        static void Main(string[] args)
        {
            W = new AutoResetEvent(false);
            CdmaModem_Huawei modem = new CdmaModem_Huawei();
            modem.SettingInfo = "COM1,115200,8,0,1,2";
            modem.LogStream = System.Console.Out;
            modem.CallOrigin += new EventHandler<CallEventArgs>(modem_CallOrigin);
            modem.CallConn += new EventHandler<CallEventArgs>(modem_CallConn);
            modem.CallCharging += new EventHandler(modem_CallCharging);
            modem.CallEnd += new EventHandler<CallEndEventArgs>(modem_CallEnd);
            modem.MessageSendSuccess += new EventHandler<MessageSendSuccessEventArgs>(modem_MessageSendSuccess);
            modem.MessageSendFailed += new EventHandler<MessageSendFailedEventArgs>(modem_MessageSendFailed);
            modem.Open();
            modem.ExitDataMode();
            modem.SetEcho(0);
            modem.SetAutoReport(0);
            modem.SwichVoicePath(1);
            modem.SendTextMessage("18966921129", "HelloA", Encoding.ASCII);
            W.WaitOne();
            Thread.Sleep(100);
            modem.SendTextMessage("18966921129", "HelloB", Encoding.ASCII);
            W.WaitOne();
            Thread.Sleep(100);
            modem.SendTextMessage("18966921129", "HelloC", Encoding.ASCII);
            W.WaitOne();
            Thread.Sleep(100);
            modem.SendTextMessage("18966921129", "HelloD", Encoding.ASCII);
            W.WaitOne();
            //modem.DialVoice("13379229575");
            System.Console.ReadLine();
            modem.HangupVoice();
            //modem.MTPowerOff();
            modem.Close();
        }

        static void modem_MessageSendFailed(object sender, MessageSendFailedEventArgs e)
        {
            W.Set();
        }

        static void modem_MessageSendSuccess(object sender, MessageSendSuccessEventArgs e)
        {
            W.Set();
        }


        static void modem_CallEnd(object sender, CallEndEventArgs e)
        {
        }

        static void modem_CallCharging(object sender, EventArgs e)
        {
        }

        static void modem_CallConn(object sender, CallEventArgs e)
        {
        }

        static void modem_CallOrigin(object sender, CallEventArgs e)
        {
        }
    }
}
