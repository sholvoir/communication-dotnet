using System;
using System.ComponentModel;

namespace CMPP
{
    /// <summary>
    /// 反向订购（取消）PROVISION 接口服务。
    /// </summary>
    public sealed class DSMP
    {

        #region 字段
        private dsmp _dsmp;
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化 <see cref="DSMP"/> 类新实例。
        /// </summary>
        public DSMP(string url)
        {
            _dsmp = new dsmp();
            _dsmp.Url = url;
        }
        #endregion

        #region 公有方法
        /// <summary>
        /// 用户通过 SP 订购数据业务的时候，SP 先进行业务关系订购，再通过该接口向 DSMP 进行用户服务订购的请求。
        /// </summary>
        /// <param name="transID">
        /// 消息编号。
        /// </param>
        /// <param name="sendAddr">
        /// 发送方的地址。
        /// </param>
        /// <param name="destAddr">
        /// 接收方的地址。
        /// </param>
        /// <param name="feeUserID">
        /// 计费用户标识。
        /// </param>
        /// <param name="destUserID">
        /// 使用用户标识（当使用用户和计费用户为同一用户的时候，FeeUser_ID 和 DestUser_ID 的值相同）。
        /// </param>
        /// <param name="spid">
        /// SP 的企业代码。
        /// </param>
        /// <param name="serviceIDType">
        /// </param>
        /// <param name="serviceID">
        /// 业务标识，是数字、字母和符号的组合（长度为 10，SP的业务类型，数字、字母和符号的组合，由SP自定，如图片传情可定为TPCQ，股票查询可定义为11），也叫做计费代码。
        /// </param>
        /// <param name="accessNo">
        /// </param>
        /// <param name="featureStr">
        /// 订购特征参数，订购业务需要携带的参数，可以携带文本/多媒体的相关信息。
        /// </param>
        /// <param name="linkID">
        /// 临时订购关系的匹配码，用来鉴权一次点播请求等事务性的业务。当DSMP生成的订购关系为临时订购关系的时候，返回本字段，否则不填本字段。
        /// </param>
        /// <param name="hResult">
        /// 返回值（0：成功；1：未知错误；2-99：保留；4000：无效的msgtype；4001：无效的action_id；4002：无效的action_reasonid；4003：无效的SP ID；4004：无效的serviceID；4005：无效的pseudocode；4006：无效的accessmode；4007：MISC 同步开通服务,但SP 端已存在订购关系,且状态为开通；4008：MISC 同步开通服务,且SP 端不存在订购关系,但开通服务失败；4009：MISC 同步开通服务,但SP 端已存在订购关系, 且状态为暂停；4010：MISC 同步停止服务, 且SP 端存在订购关系, 但取消服务失败；4011：MISC 同步停止服务, 但SP 端不存在订购关系；4012：MISC 同步暂停服务, 且SP 端存在订购关系, 但暂停服务失败；4013：MISC 同步暂停服务, 但SP 端不存在订购关系；4014：MISC 同步暂停服务, 但SP 端已存在订购关系, 且状态为暂停；4015：MISC 同步激活服务, 但SP 端已存在订购关系, 且状态为开通；4016：MISC 同步激活服务, 但SP 端不存在订购关系；4017：MISC 同步激活服务, 且SP 端存在订购关系, 但激活服务失败；9000：系统磁盘读写错误；9001：网络异常；9002：网络错误；9003：业务网关忙，业务网关缓存；9004：业务网关忙，并且业务网关缓冲区满，MISC 缓存，并暂时不要发送消息，等待一段时间重试；9005：MISC 忙，MISC 缓存；9006：MISC 忙，并且MISC 缓冲区满，业务网关缓存，并暂时不要发送消息，等待一段时间重试；9007：业务网关超过限制的流量；9008：MISC 异常，并不可用；9009：业务网关异常，并不可用；9010：该业务网关没有权限调用该接口消息；9011：MISC 没有权限发送该接口消息给业务网关；9012：版本不支持；9013：消息类型不对，系统不支持；9014：验证错误，无法解析SOAP 和XML 结构、缺少必须存在的字段，或者消息,格式不正确；9015：拒绝消息，服务器无法完成请求的服务）。
        /// </param>
        public void Subscribe(
            string transID,
            AddressInfo sendAddr,
            AddressInfo destAddr,
            UserID feeUserID,
            UserID destUserID,
            string spid,
            string serviceIDType,
            string serviceID,
            string accessNo,
            byte[] featureStr,
            out string linkID,
            out int hResult)
        {
            SubscribeServiceReqType req = new SubscribeServiceReqType();

            req.Dest_Address = new address_info_schema();
            req.Dest_Address.DeviceID = destAddr.DeviceID;
            req.Dest_Address.DeviceType = destAddr.DeviceType.ToString();

            req.DestUser_ID = new user_id_schema();
            req.DestUser_ID.MSISDN = destUserID.MSISDN;
            req.DestUser_ID.PseudoCode = System.Text.Encoding.ASCII.GetBytes(destUserID.PseudoCode);
            req.DestUser_ID.UserIDType = destUserID.UserIDType.ToString();

            req.FeatureStr = featureStr;

            req.FeeUser_ID = new user_id_schema();
            req.FeeUser_ID.MSISDN = feeUserID.MSISDN;
            req.FeeUser_ID.PseudoCode = System.Text.Encoding.ASCII.GetBytes(feeUserID.PseudoCode);
            req.FeeUser_ID.UserIDType = feeUserID.UserIDType.ToString();

            req.MsgType = "SubscribeServiceReq";

            req.Send_Address = new address_info_schema();
            req.Send_Address.DeviceID = sendAddr.DeviceID;
            req.Send_Address.DeviceType = sendAddr.DeviceType.ToString();

            req.Service_ID = new service_id_schema();
            req.Service_ID.AccessNo = accessNo;
            //req.Service_ID.FeatureStr;
            req.Service_ID.ServiceIDType = serviceIDType;
            req.Service_ID.SPID = spid;
            req.Service_ID.SPServiceID = serviceID;

            req.Version = "1.5.0";

            @string transactionID = new @string();
            transactionID.Text = new string[]{ transID };

            SubscribeServiceRespType rpt = _dsmp.SubscribeService(req, ref transactionID);

            linkID = rpt.LinkID;
            hResult = System.Convert.ToInt32(rpt.hRet);
        }
        /// <summary>
        /// 用户通过 SP 取消已订购的数据业务的时候，SP 先通过该接口向 DSMP 进行用户取消服务订购的请求。DSMP 进行取消服务订购成功后，SP 才取消用户对应的业务订购关系。
        /// </summary>
        /// <param name="transID">
        /// 消息编号。
        /// </param>
        /// <param name="sendAddr">
        /// 发送方的地址。
        /// </param>
        /// <param name="destAddr">
        /// 接收方的地址。
        /// </param>
        /// <param name="feeUserID">
        /// 计费用户标识。
        /// </param>
        /// <param name="destUserID">
        /// 使用用户标识（当使用用户和计费用户为同一用户的时候，FeeUser_ID 和 DestUser_ID 的值相同）。
        /// </param>
        /// <param name="spid">
        /// SP 的企业代码。
        /// </param>
        /// <param name="serviceIDType">
        /// </param>
        /// <param name="serviceID">
        /// 业务标识，是数字、字母和符号的组合（长度为 10，SP的业务类型，数字、字母和符号的组合，由SP自定，如图片传情可定为TPCQ，股票查询可定义为11），也叫做计费代码。
        /// </param>
        /// <param name="accessNo">
        /// </param>
        /// <param name="hResult">
        /// 返回值（0：成功；1：未知错误；2-99：保留；4000：无效的msgtype；4001：无效的action_id；4002：无效的action_reasonid；4003：无效的SP ID；4004：无效的serviceID；4005：无效的pseudocode；4006：无效的accessmode；4007：MISC 同步开通服务,但SP 端已存在订购关系,且状态为开通；4008：MISC 同步开通服务,且SP 端不存在订购关系,但开通服务失败；4009：MISC 同步开通服务,但SP 端已存在订购关系, 且状态为暂停；4010：MISC 同步停止服务, 且SP 端存在订购关系, 但取消服务失败；4011：MISC 同步停止服务, 但SP 端不存在订购关系；4012：MISC 同步暂停服务, 且SP 端存在订购关系, 但暂停服务失败；4013：MISC 同步暂停服务, 但SP 端不存在订购关系；4014：MISC 同步暂停服务, 但SP 端已存在订购关系, 且状态为暂停；4015：MISC 同步激活服务, 但SP 端已存在订购关系, 且状态为开通；4016：MISC 同步激活服务, 但SP 端不存在订购关系；4017：MISC 同步激活服务, 且SP 端存在订购关系, 但激活服务失败；9000：系统磁盘读写错误；9001：网络异常；9002：网络错误；9003：业务网关忙，业务网关缓存；9004：业务网关忙，并且业务网关缓冲区满，MISC 缓存，并暂时不要发送消息，等待一段时间重试；9005：MISC 忙，MISC 缓存；9006：MISC 忙，并且MISC 缓冲区满，业务网关缓存，并暂时不要发送消息，等待一段时间重试；9007：业务网关超过限制的流量；9008：MISC 异常，并不可用；9009：业务网关异常，并不可用；9010：该业务网关没有权限调用该接口消息；9011：MISC 没有权限发送该接口消息给业务网关；9012：版本不支持；9013：消息类型不对，系统不支持；9014：验证错误，无法解析SOAP 和XML 结构、缺少必须存在的字段，或者消息,格式不正确；9015：拒绝消息，服务器无法完成请求的服务）。
        /// </param>
        public void UnSubscribe(
            string transID,
            AddressInfo sendAddr,
            AddressInfo destAddr,
            UserID feeUserID,
            UserID destUserID,
            string spid,
            string serviceIDType,
            string serviceID,
            string accessNo,
            out int hResult)
        {
            UnSubscribeServiceReqType req = new UnSubscribeServiceReqType();

            req.Dest_Address = new address_info_schema();
            req.Dest_Address.DeviceID = destAddr.DeviceID;
            req.Dest_Address.DeviceType = destAddr.DeviceType.ToString();

            req.DestUser_ID = new user_id_schema();
            req.DestUser_ID.MSISDN = destUserID.MSISDN;
            req.DestUser_ID.PseudoCode = System.Text.Encoding.ASCII.GetBytes(destUserID.PseudoCode);
            req.DestUser_ID.UserIDType = destUserID.UserIDType.ToString();

            req.FeeUser_ID = new user_id_schema();
            req.FeeUser_ID.MSISDN = feeUserID.MSISDN;
            req.FeeUser_ID.PseudoCode = System.Text.Encoding.ASCII.GetBytes(feeUserID.PseudoCode);
            req.FeeUser_ID.UserIDType = feeUserID.UserIDType.ToString();

            req.MsgType = "UnSubscribeServiceReq";

            req.Send_Address = new address_info_schema();
            req.Send_Address.DeviceID = sendAddr.DeviceID;
            req.Send_Address.DeviceType = sendAddr.DeviceType.ToString();

            req.Service_ID = new service_id_schema();
            req.Service_ID.AccessNo = accessNo;
            //req.Service_ID.FeatureStr;
            req.Service_ID.ServiceIDType = serviceIDType;
            req.Service_ID.SPID = spid;
            req.Service_ID.SPServiceID = serviceID;

            req.Version = "1.5.0";

            @string transactionID = new @string();
            transactionID.Text = new string[] { transID };

            UnSubscribeServiceRespType rpt = _dsmp.UnSubscribeService(req, ref transactionID);
            hResult = System.Convert.ToInt32(rpt.hRet);
        }
        #endregion

        #region WEB 服务代理类
        // 
        // This source code was auto-generated by wsdl, Version=2.0.50727.42.
        // 
        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Web.Services.WebServiceBindingAttribute(Name = "maPortBinding", Namespace = "http://www.monternet.com/dsmp/wsdl/")]
        private partial class dsmp : System.Web.Services.Protocols.SoapHttpClientProtocol
        {

            private @string transactionIDField;

            private System.Threading.SendOrPostCallback SyncOrderRelationOperationCompleted;

            private System.Threading.SendOrPostCallback SubscribeServiceOperationCompleted;

            private System.Threading.SendOrPostCallback UnSubscribeServiceOperationCompleted;

            /// <remarks/>
            public dsmp()
            {
                this.Url = "http://localhost:8080/axis/services/maPort";
            }
            /// <summary>
            /// 
            /// </summary>
            public @string TransactionID
            {
                get
                {
                    return this.transactionIDField;
                }
                set
                {
                    this.transactionIDField = value;
                }
            }

            /// <remarks/>
            public event SyncOrderRelationCompletedEventHandler SyncOrderRelationCompleted;

            /// <remarks/>
            public event SubscribeServiceCompletedEventHandler SubscribeServiceCompleted;

            /// <remarks/>
            public event UnSubscribeServiceCompletedEventHandler UnSubscribeServiceCompleted;

            /// <remarks/>
            [System.Web.Services.Protocols.SoapHeaderAttribute("TransactionID", Direction = System.Web.Services.Protocols.SoapHeaderDirection.InOut)]
            [System.Web.Services.Protocols.SoapDocumentMethodAttribute("sim.SyncOrderRelation", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
            [return: System.Xml.Serialization.XmlElementAttribute("SyncOrderRelationResp", Namespace = "http://www.monternet.com/dsmp/schemas/")]
            public SyncOrderRelationRespType SyncOrderRelation([System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.monternet.com/dsmp/schemas/")] SyncOrderRelationReqType SyncOrderRelationReq, [System.Xml.Serialization.XmlElementAttribute("TransactionID", Namespace = "http://www.monternet.com/dsmp/schemas/")] ref @string TransactionID1)
            {
                object[] results = this.Invoke("SyncOrderRelation", new object[] {
                    SyncOrderRelationReq,
                    TransactionID1});
                TransactionID1 = ((@string)(results[1]));
                return ((SyncOrderRelationRespType)(results[0]));
            }

            /// <remarks/>
            public System.IAsyncResult BeginSyncOrderRelation(SyncOrderRelationReqType SyncOrderRelationReq, @string TransactionID1, System.AsyncCallback callback, object asyncState)
            {
                return this.BeginInvoke("SyncOrderRelation", new object[] {
                    SyncOrderRelationReq,
                    TransactionID1}, callback, asyncState);
            }

            /// <remarks/>
            public SyncOrderRelationRespType EndSyncOrderRelation(System.IAsyncResult asyncResult, out @string TransactionID1)
            {
                object[] results = this.EndInvoke(asyncResult);
                TransactionID1 = ((@string)(results[1]));
                return ((SyncOrderRelationRespType)(results[0]));
            }

            /// <remarks/>
            public void SyncOrderRelationAsync(SyncOrderRelationReqType SyncOrderRelationReq, @string TransactionID1)
            {
                this.SyncOrderRelationAsync(SyncOrderRelationReq, TransactionID1, null);
            }

            /// <remarks/>
            public void SyncOrderRelationAsync(SyncOrderRelationReqType SyncOrderRelationReq, @string TransactionID1, object userState)
            {
                if ((this.SyncOrderRelationOperationCompleted == null))
                {
                    this.SyncOrderRelationOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSyncOrderRelationOperationCompleted);
                }
                this.InvokeAsync("SyncOrderRelation", new object[] {
                    SyncOrderRelationReq,
                    TransactionID1}, this.SyncOrderRelationOperationCompleted, userState);
            }

            private void OnSyncOrderRelationOperationCompleted(object arg)
            {
                if ((this.SyncOrderRelationCompleted != null))
                {
                    System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                    this.SyncOrderRelationCompleted(this, new SyncOrderRelationCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
                }
            }

            /// <remarks/>
            [System.Web.Services.Protocols.SoapHeaderAttribute("TransactionID", Direction = System.Web.Services.Protocols.SoapHeaderDirection.InOut)]
            [System.Web.Services.Protocols.SoapDocumentMethodAttribute("sim.SubscribeService", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
            [return: System.Xml.Serialization.XmlElementAttribute("SubscribeServiceResp", Namespace = "http://www.monternet.com/dsmp/schemas/")]
            public SubscribeServiceRespType SubscribeService([System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.monternet.com/dsmp/schemas/")] SubscribeServiceReqType SubscribeServiceReq, [System.Xml.Serialization.XmlElementAttribute("TransactionID", Namespace = "http://www.monternet.com/dsmp/schemas/")] ref @string TransactionID1)
            {
                object[] results = this.Invoke("SubscribeService", new object[] {
                    SubscribeServiceReq,
                    TransactionID1});
                TransactionID1 = ((@string)(results[1]));
                return ((SubscribeServiceRespType)(results[0]));
            }

            /// <remarks/>
            public System.IAsyncResult BeginSubscribeService(SubscribeServiceReqType SubscribeServiceReq, @string TransactionID1, System.AsyncCallback callback, object asyncState)
            {
                return this.BeginInvoke("SubscribeService", new object[] {
                    SubscribeServiceReq,
                    TransactionID1}, callback, asyncState);
            }

            /// <remarks/>
            public SubscribeServiceRespType EndSubscribeService(System.IAsyncResult asyncResult, out @string TransactionID1)
            {
                object[] results = this.EndInvoke(asyncResult);
                TransactionID1 = ((@string)(results[1]));
                return ((SubscribeServiceRespType)(results[0]));
            }

            /// <remarks/>
            public void SubscribeServiceAsync(SubscribeServiceReqType SubscribeServiceReq, @string TransactionID1)
            {
                this.SubscribeServiceAsync(SubscribeServiceReq, TransactionID1, null);
            }

            /// <remarks/>
            public void SubscribeServiceAsync(SubscribeServiceReqType SubscribeServiceReq, @string TransactionID1, object userState)
            {
                if ((this.SubscribeServiceOperationCompleted == null))
                {
                    this.SubscribeServiceOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSubscribeServiceOperationCompleted);
                }
                this.InvokeAsync("SubscribeService", new object[] {
                    SubscribeServiceReq,
                    TransactionID1}, this.SubscribeServiceOperationCompleted, userState);
            }

            private void OnSubscribeServiceOperationCompleted(object arg)
            {
                if ((this.SubscribeServiceCompleted != null))
                {
                    System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                    this.SubscribeServiceCompleted(this, new SubscribeServiceCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
                }
            }

            /// <remarks/>
            [System.Web.Services.Protocols.SoapHeaderAttribute("TransactionID", Direction = System.Web.Services.Protocols.SoapHeaderDirection.InOut)]
            [System.Web.Services.Protocols.SoapDocumentMethodAttribute("sim.UnSubscribeService", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
            [return: System.Xml.Serialization.XmlElementAttribute("UnSubscribeServiceResp", Namespace = "http://www.monternet.com/dsmp/schemas/")]
            public UnSubscribeServiceRespType UnSubscribeService([System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.monternet.com/dsmp/schemas/")] UnSubscribeServiceReqType UnSubscribeServiceReq, [System.Xml.Serialization.XmlElementAttribute("TransactionID", Namespace = "http://www.monternet.com/dsmp/schemas/")] ref @string TransactionID1)
            {
                object[] results = this.Invoke("UnSubscribeService", new object[] {
                    UnSubscribeServiceReq,
                    TransactionID1});
                TransactionID1 = ((@string)(results[1]));
                return ((UnSubscribeServiceRespType)(results[0]));
            }

            /// <remarks/>
            public System.IAsyncResult BeginUnSubscribeService(UnSubscribeServiceReqType UnSubscribeServiceReq, @string TransactionID1, System.AsyncCallback callback, object asyncState)
            {
                return this.BeginInvoke("UnSubscribeService", new object[] {
                    UnSubscribeServiceReq,
                    TransactionID1}, callback, asyncState);
            }

            /// <remarks/>
            public UnSubscribeServiceRespType EndUnSubscribeService(System.IAsyncResult asyncResult, out @string TransactionID1)
            {
                object[] results = this.EndInvoke(asyncResult);
                TransactionID1 = ((@string)(results[1]));
                return ((UnSubscribeServiceRespType)(results[0]));
            }

            /// <remarks/>
            public void UnSubscribeServiceAsync(UnSubscribeServiceReqType UnSubscribeServiceReq, @string TransactionID1)
            {
                this.UnSubscribeServiceAsync(UnSubscribeServiceReq, TransactionID1, null);
            }

            /// <remarks/>
            public void UnSubscribeServiceAsync(UnSubscribeServiceReqType UnSubscribeServiceReq, @string TransactionID1, object userState)
            {
                if ((this.UnSubscribeServiceOperationCompleted == null))
                {
                    this.UnSubscribeServiceOperationCompleted = new System.Threading.SendOrPostCallback(this.OnUnSubscribeServiceOperationCompleted);
                }
                this.InvokeAsync("UnSubscribeService", new object[] {
                    UnSubscribeServiceReq,
                    TransactionID1}, this.UnSubscribeServiceOperationCompleted, userState);
            }

            private void OnUnSubscribeServiceOperationCompleted(object arg)
            {
                if ((this.UnSubscribeServiceCompleted != null))
                {
                    System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                    this.UnSubscribeServiceCompleted(this, new UnSubscribeServiceCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
                }
            }

            /// <remarks/>
            public new void CancelAsync(object userState)
            {
                base.CancelAsync(userState);
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema")]
        [System.Xml.Serialization.XmlRootAttribute("TransactionID", Namespace = "http://www.monternet.com/dsmp/schemas/", IsNullable = false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public partial class @string : System.Web.Services.Protocols.SoapHeader
        {

            private string[] textField;

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string[] Text
            {
                get
                {
                    return this.textField;
                }
                set
                {
                    this.textField = value;
                }
            }
        }
        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.monternet.com/dsmp/schemas/")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public partial class SyncOrderRelationReqType
        {

            private string msgTypeField;

            private string versionField;

            private address_info_schema send_AddressField;

            private address_info_schema dest_AddressField;

            private user_id_schema feeUser_IDField;

            private user_id_schema destUser_IDField;

            private string linkIDField;

            private string actionIDField;

            private string actionReasonIDField;

            private string sPIDField;

            private string sPServiceIDField;

            private string accessModeField;

            private byte[] featureStrField;

            /// <remarks/>
            public string MsgType
            {
                get
                {
                    return this.msgTypeField;
                }
                set
                {
                    this.msgTypeField = value;
                }
            }

            /// <remarks/>
            public string Version
            {
                get
                {
                    return this.versionField;
                }
                set
                {
                    this.versionField = value;
                }
            }

            /// <remarks/>
            public address_info_schema Send_Address
            {
                get
                {
                    return this.send_AddressField;
                }
                set
                {
                    this.send_AddressField = value;
                }
            }

            /// <remarks/>
            public address_info_schema Dest_Address
            {
                get
                {
                    return this.dest_AddressField;
                }
                set
                {
                    this.dest_AddressField = value;
                }
            }

            /// <remarks/>
            public user_id_schema FeeUser_ID
            {
                get
                {
                    return this.feeUser_IDField;
                }
                set
                {
                    this.feeUser_IDField = value;
                }
            }

            /// <remarks/>
            public user_id_schema DestUser_ID
            {
                get
                {
                    return this.destUser_IDField;
                }
                set
                {
                    this.destUser_IDField = value;
                }
            }

            /// <remarks/>
            public string LinkID
            {
                get
                {
                    return this.linkIDField;
                }
                set
                {
                    this.linkIDField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
            public string ActionID
            {
                get
                {
                    return this.actionIDField;
                }
                set
                {
                    this.actionIDField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
            public string ActionReasonID
            {
                get
                {
                    return this.actionReasonIDField;
                }
                set
                {
                    this.actionReasonIDField = value;
                }
            }

            /// <remarks/>
            public string SPID
            {
                get
                {
                    return this.sPIDField;
                }
                set
                {
                    this.sPIDField = value;
                }
            }

            /// <remarks/>
            public string SPServiceID
            {
                get
                {
                    return this.sPServiceIDField;
                }
                set
                {
                    this.sPServiceIDField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
            public string AccessMode
            {
                get
                {
                    return this.accessModeField;
                }
                set
                {
                    this.accessModeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
            public byte[] FeatureStr
            {
                get
                {
                    return this.featureStrField;
                }
                set
                {
                    this.featureStrField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.monternet.com/dsmp/schemas/")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public partial class address_info_schema
        {

            private string deviceTypeField;

            private string deviceIDField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
            public string DeviceType
            {
                get
                {
                    return this.deviceTypeField;
                }
                set
                {
                    this.deviceTypeField = value;
                }
            }

            /// <remarks/>
            public string DeviceID
            {
                get
                {
                    return this.deviceIDField;
                }
                set
                {
                    this.deviceIDField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.monternet.com/dsmp/schemas/")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public partial class UnSubscribeServiceRespType
        {

            private string versionField;

            private string msgTypeField;

            private string hRetField;

            /// <remarks/>
            public string Version
            {
                get
                {
                    return this.versionField;
                }
                set
                {
                    this.versionField = value;
                }
            }

            /// <remarks/>
            public string MsgType
            {
                get
                {
                    return this.msgTypeField;
                }
                set
                {
                    this.msgTypeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
            public string hRet
            {
                get
                {
                    return this.hRetField;
                }
                set
                {
                    this.hRetField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.monternet.com/dsmp/schemas/")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public partial class UnSubscribeServiceReqType
        {

            private string versionField;

            private string msgTypeField;

            private address_info_schema send_AddressField;

            private address_info_schema dest_AddressField;

            private user_id_schema feeUser_IDField;

            private user_id_schema destUser_IDField;

            private service_id_schema service_IDField;

            /// <remarks/>
            public string Version
            {
                get
                {
                    return this.versionField;
                }
                set
                {
                    this.versionField = value;
                }
            }

            /// <remarks/>
            public string MsgType
            {
                get
                {
                    return this.msgTypeField;
                }
                set
                {
                    this.msgTypeField = value;
                }
            }

            /// <remarks/>
            public address_info_schema Send_Address
            {
                get
                {
                    return this.send_AddressField;
                }
                set
                {
                    this.send_AddressField = value;
                }
            }

            /// <remarks/>
            public address_info_schema Dest_Address
            {
                get
                {
                    return this.dest_AddressField;
                }
                set
                {
                    this.dest_AddressField = value;
                }
            }

            /// <remarks/>
            public user_id_schema FeeUser_ID
            {
                get
                {
                    return this.feeUser_IDField;
                }
                set
                {
                    this.feeUser_IDField = value;
                }
            }

            /// <remarks/>
            public user_id_schema DestUser_ID
            {
                get
                {
                    return this.destUser_IDField;
                }
                set
                {
                    this.destUser_IDField = value;
                }
            }

            /// <remarks/>
            public service_id_schema Service_ID
            {
                get
                {
                    return this.service_IDField;
                }
                set
                {
                    this.service_IDField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.monternet.com/dsmp/schemas/")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public partial class user_id_schema
        {

            private string userIDTypeField;

            private string mSISDNField;

            private byte[] pseudoCodeField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
            public string UserIDType
            {
                get
                {
                    return this.userIDTypeField;
                }
                set
                {
                    this.userIDTypeField = value;
                }
            }

            /// <remarks/>
            public string MSISDN
            {
                get
                {
                    return this.mSISDNField;
                }
                set
                {
                    this.mSISDNField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
            public byte[] PseudoCode
            {
                get
                {
                    return this.pseudoCodeField;
                }
                set
                {
                    this.pseudoCodeField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.monternet.com/dsmp/schemas/")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public partial class service_id_schema
        {

            private string serviceIDTypeField;

            private string sPIDField;

            private string sPServiceIDField;

            private string accessNoField;

            private byte[] featureStrField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
            public string ServiceIDType
            {
                get
                {
                    return this.serviceIDTypeField;
                }
                set
                {
                    this.serviceIDTypeField = value;
                }
            }

            /// <remarks/>
            public string SPID
            {
                get
                {
                    return this.sPIDField;
                }
                set
                {
                    this.sPIDField = value;
                }
            }

            /// <remarks/>
            public string SPServiceID
            {
                get
                {
                    return this.sPServiceIDField;
                }
                set
                {
                    this.sPServiceIDField = value;
                }
            }

            /// <remarks/>
            public string AccessNo
            {
                get
                {
                    return this.accessNoField;
                }
                set
                {
                    this.accessNoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
            public byte[] FeatureStr
            {
                get
                {
                    return this.featureStrField;
                }
                set
                {
                    this.featureStrField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.monternet.com/dsmp/schemas/")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public partial class SubscribeServiceRespType
        {

            private string versionField;

            private string msgTypeField;

            private string hRetField;

            private string linkIDField;

            /// <remarks/>
            public string Version
            {
                get
                {
                    return this.versionField;
                }
                set
                {
                    this.versionField = value;
                }
            }

            /// <remarks/>
            public string MsgType
            {
                get
                {
                    return this.msgTypeField;
                }
                set
                {
                    this.msgTypeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
            public string hRet
            {
                get
                {
                    return this.hRetField;
                }
                set
                {
                    this.hRetField = value;
                }
            }

            /// <remarks/>
            public string LinkID
            {
                get
                {
                    return this.linkIDField;
                }
                set
                {
                    this.linkIDField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.monternet.com/dsmp/schemas/")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public partial class SubscribeServiceReqType
        {

            private string versionField;

            private string msgTypeField;

            private address_info_schema send_AddressField;

            private address_info_schema dest_AddressField;

            private user_id_schema feeUser_IDField;

            private user_id_schema destUser_IDField;

            private service_id_schema service_IDField;

            private byte[] featureStrField;

            /// <remarks/>
            public string Version
            {
                get
                {
                    return this.versionField;
                }
                set
                {
                    this.versionField = value;
                }
            }

            /// <remarks/>
            public string MsgType
            {
                get
                {
                    return this.msgTypeField;
                }
                set
                {
                    this.msgTypeField = value;
                }
            }

            /// <remarks/>
            public address_info_schema Send_Address
            {
                get
                {
                    return this.send_AddressField;
                }
                set
                {
                    this.send_AddressField = value;
                }
            }

            /// <remarks/>
            public address_info_schema Dest_Address
            {
                get
                {
                    return this.dest_AddressField;
                }
                set
                {
                    this.dest_AddressField = value;
                }
            }

            /// <remarks/>
            public user_id_schema FeeUser_ID
            {
                get
                {
                    return this.feeUser_IDField;
                }
                set
                {
                    this.feeUser_IDField = value;
                }
            }

            /// <remarks/>
            public user_id_schema DestUser_ID
            {
                get
                {
                    return this.destUser_IDField;
                }
                set
                {
                    this.destUser_IDField = value;
                }
            }

            /// <remarks/>
            public service_id_schema Service_ID
            {
                get
                {
                    return this.service_IDField;
                }
                set
                {
                    this.service_IDField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
            public byte[] FeatureStr
            {
                get
                {
                    return this.featureStrField;
                }
                set
                {
                    this.featureStrField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.monternet.com/dsmp/schemas/")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public partial class SyncOrderRelationRespType
        {

            private string msgTypeField;

            private string versionField;

            private string hRetField;

            /// <remarks/>
            public string MsgType
            {
                get
                {
                    return this.msgTypeField;
                }
                set
                {
                    this.msgTypeField = value;
                }
            }

            /// <remarks/>
            public string Version
            {
                get
                {
                    return this.versionField;
                }
                set
                {
                    this.versionField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
            public string hRet
            {
                get
                {
                    return this.hRetField;
                }
                set
                {
                    this.hRetField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public delegate void SyncOrderRelationCompletedEventHandler(object sender, SyncOrderRelationCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public partial class SyncOrderRelationCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal SyncOrderRelationCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState)
                :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public SyncOrderRelationRespType Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((SyncOrderRelationRespType)(this.results[0]));
                }
            }

            /// <remarks/>
            public @string TransactionID1
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((@string)(this.results[1]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public delegate void SubscribeServiceCompletedEventHandler(object sender, SubscribeServiceCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public partial class SubscribeServiceCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal SubscribeServiceCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState)
                :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public SubscribeServiceRespType Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((SubscribeServiceRespType)(this.results[0]));
                }
            }

            /// <remarks/>
            public @string TransactionID1
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((@string)(this.results[1]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public delegate void UnSubscribeServiceCompletedEventHandler(object sender, UnSubscribeServiceCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public partial class UnSubscribeServiceCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal UnSubscribeServiceCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState)
                :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public UnSubscribeServiceRespType Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((UnSubscribeServiceRespType)(this.results[0]));
                }
            }

            /// <remarks/>
            public @string TransactionID1
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((@string)(this.results[1]));
                }
            }
        }
        #endregion

    }
}
