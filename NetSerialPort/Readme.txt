配置说明:
1. SerialSetting格式: PortName,BaudRate,DataBits,Parity(None=0,Odd=1,Even=2,Mark=3,Space=4),StopBits(None=0,One=1,Two=2,OnePointFive=3),Handshake(None=0,XOnXOff=1,RequestToSend=2,RequestToSendXOnXOff=3)
2. IPPort格式: IP:Port(其中IP可使用域名代替, IP可省略表示本机全部IP, IP省略时, 不要使用分隔符":", Port不能省略)