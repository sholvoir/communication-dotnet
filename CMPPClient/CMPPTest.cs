using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vultrue.Communication.CMPPMessages;

namespace Vultrue.Communication
{
    public class CMPPTest
    {
        static void Main()
        {
            CMPPTest a = new CMPPTest();
            CMPPClient c = new CMPPClient();
            c.ResponseReceive += new Vultrue.Communication.CMPP.Client.ResponseEventHandler(a.c_ResponseReceive);
            Console.WriteLine("press 'q' to exit this programe!");
            uint i = 0; //Sequence_Id header
            ulong l = 0; //Msg_Id body
            if (c.Connect("localhost", 7890, "901234", "1234", ++i))
            {
                c.Submit(++l, "1391xxx1138", new string[] { "13911234567" }, "卧鼠藏虫 身披七彩祥云 脚踏金甲圣衣", ++i);
                Console.WriteLine("Request: Sequence_Id: {0},Msg_Id: {1}", i, l);
                c.Query(DateTime.Parse("2005-1-1"), 1, "001", "", ++i);
                Console.WriteLine("Request: Sequence_Id: {0},Msg_Id: {1}", i, l);
                c.Query(DateTime.Parse("2005-1-1"), 1, "001", "", ++i);
                Console.WriteLine("Request: Sequence_Id: {0},Msg_Id: {1}", i, l);
                c.ActiveTest(++i);
                Console.WriteLine("Request: Sequence_Id: {0},Msg_Id: {1}", i, l);
                c.Submit(++l, "1391xxx1138", new string[] { "13911234567" }, "欲穷千里目 粒粒皆辛苦", ++i);
                Console.WriteLine("Request: Sequence_Id: {0},Msg_Id: {1}", i, l);
                //c.StartRun();
            }

            string s;
            while ((s = Console.ReadLine()) != "q")
            {
                if (c.IsConnected)
                {
                    if (s.Length > 0)
                    {
                        c.Submit(++l, "1391xxx1138", new string[] { "13911234567" }, s, ++i);
                        Console.WriteLine("Request: Sequence_Id: {0},Msg_Id: {1}, Content: \"{2}\"", i, l, s);
                    }
                    else
                    {
                        Console.WriteLine("you can submit your SMS at here,or press 'q' to exit this programe!");
                    }
                }

            }
            if (c.IsConnected)
            {
                c.Terminate(++i);
            }

            Console.ReadLine();
        }

        private void c_ResponseReceive(CMPPClient Sender, ResponseEventArgs e)
        {
            CMPP_MESSAGE header = e.ResponseHeader;
            this.PrintHeader(header);
            byte[] bytes = new byte[header.TotalLength];
            e.ResponseHeaderData.CopyTo(bytes, 0);
            e.ResponseBodyData.CopyTo(bytes, CMPP_MESSAGE.HEAD_LENTH);
            string s = "";
            if (header.CommandId == CMPP_COMMAND_ID.CMPP_ACTIVE_TEST_RESP)
            {
                CMPP_ACTIVE_TEST_RESP response = new CMPP_ACTIVE_TEST_RESP(bytes);
                Console.WriteLine(response.Reserved);
            }
            else if (header.CommandId == CMPP_COMMAND_ID.CMPP_CONNECT_RESP)
            {
                CMPP_CONNECT_RESP response = new CMPP_CONNECT_RESP(bytes);
                s = String.Format("CMPP_CONNECT_RESP Status: {0}", response.Status);
            }
            else if (header.CommandId == CMPP_COMMAND_ID.CMPP_DELIVER)
            {
                CMPP_DELIVER response = new CMPP_DELIVER(bytes);
                //s = String.Format("CMPP_DELIVER : {0},{1}",response.Src_terminal_Id,response.Msg_Content);
                if (response.RegisteredDelivery == 0) //普通短信
                {
                    s = String.Format("收到普通短信: \n{0}\n{1}", response.SrcTerminalId, response.MsgContent);
                }
                else
                //该模拟器不能自动生成状态报告再下发!请自行键入下面短信内容后,发送
                //状态报告短信: 00000001DELIVRD031213505003121350501391xxx11381391xxx11381391xx11380001
                {
                    CMPP_MSG_CONTENT x = new CMPP_MSG_CONTENT(Encoding.ASCII.GetBytes(response.MsgContent));
                    s = String.Format("收到状态报告: \n{0}\n{1}", x.Stat, x.DestTerminalId);
                }
            }
            else if (header.CommandId == CMPP_COMMAND_ID.CMPP_QUERY_RESP)
            {
                CMPP_QUERY_RESP response = new CMPP_QUERY_RESP(bytes);
                s = String.Format("CMPP_QUERY_RESP: {0},{1}", response.Time, response.QueryCode);
            }
            else if (header.CommandId == CMPP_COMMAND_ID.CMPP_SUBMIT_RESP)
            {
                CMPP_SUBMIT_RESP response = new CMPP_SUBMIT_RESP(bytes);
                s = String.Format("CMPP_SUBMIT_RESP Msg_Id: {0}, Result: {1}", response.MsgId, response.Result);
            }
            else if (header.CommandId == CMPP_COMMAND_ID.CMPP_TERMINATE_RESP)
            {
                s = String.Format("good bye");
            }

            Console.WriteLine(s + "\n");

        }

        public void PrintHeader(CMPP_MESSAGE Header)
        {
            Console.WriteLine("Response: Sequence_Id: {0}!", Header.SequenceId);
            Console.WriteLine("Total_Length: {0}!", Header.TotalLength);
            Console.WriteLine("Command_Id: {0}!", Header.CommandId);
        }
    }
}
