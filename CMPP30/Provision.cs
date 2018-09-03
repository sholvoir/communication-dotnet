using System;
using System.ComponentModel;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace CMPP
{
    /// <summary>
    /// 正向（反向）同步 PROVISION 接口服务。
    /// </summary>
    [WebService(Namespace = "http://www.monternet.com/dsmp/schemas/")]
    [SoapDocumentService(RoutingStyle = SoapServiceRoutingStyle.RequestElement)]
    public abstract class Provision : System.Web.Services.WebService
    {

        #region 属性
        /// <summary>
        /// SOAP 消息编号（DSMG 调用该 WEB 服务时需要设置该信息）。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TransactionID TransactionID;
        #endregion

        #region 保护方法
        /// <summary>
        /// MISC 因为某种情况（如：用户通过手机短信的方式执行了某操作）更新了用户订购关系（包括：订购、取消、暂停、激活）的时候，通过此函数发起和 SP 的更新订购关系的交互。
        /// </summary>
        /// <param name="id">
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
        /// <param name="linkID">
        /// 临时订购关系的事务 ID。
        /// </param>
        /// <param name="actionID">
        /// 服务状态管理动作代码（1：开通服务；2：停止服务；3：激活服务；4：暂停服务）。
        /// </param>
        /// <param name="actionReasonID">
        /// 产生服务状态管理动作原因的代码（1：用户发起行为；2：Admin&amp;1860发起行为；3：Boss停机；4：Boss开机；5：Boss过户；6：Boss销户；7：Boss改号；8：扣费失败导致的服务取消；9：其他）。
        /// </param>
        /// <param name="spid">
        /// SP 的企业代码。
        /// </param>
        /// <param name="serviceID">
        /// 业务标识，是数字、字母和符号的组合（长度为 10，SP的业务类型，数字、字母和符号的组合，由SP自定，如图片传情可定为TPCQ，股票查询可定义为11），也叫做计费代码。
        /// </param>
        /// <param name="accessMode">
        /// 服务的访问方式（1：WEB；2：WAP；3：SMS）。
        /// </param>
        /// <param name="featureStr">
        /// 服务订购参数（base64加密），内容是长号码＋空格＋用户发送内容。
        /// </param>
        /// <param name="hResult">
        /// 返回值（0：成功；1：未知错误；2-99：保留；4000：无效的msgtype；4001：无效的action_id；4002：无效的action_reasonid；4003：无效的SP ID；4004：无效的serviceID；4005：无效的pseudocode；4006：无效的accessmode；4007：MISC 同步开通服务,但SP 端已存在订购关系,且状态为开通；4008：MISC 同步开通服务,且SP 端不存在订购关系,但开通服务失败；4009：MISC 同步开通服务,但SP 端已存在订购关系, 且状态为暂停；4010：MISC 同步停止服务, 且SP 端存在订购关系, 但取消服务失败；4011：MISC 同步停止服务, 但SP 端不存在订购关系；4012：MISC 同步暂停服务, 且SP 端存在订购关系, 但暂停服务失败；4013：MISC 同步暂停服务, 但SP 端不存在订购关系；4014：MISC 同步暂停服务, 但SP 端已存在订购关系, 且状态为暂停；4015：MISC 同步激活服务, 但SP 端已存在订购关系, 且状态为开通；4016：MISC 同步激活服务, 但SP 端不存在订购关系；4017：MISC 同步激活服务, 且SP 端存在订购关系, 但激活服务失败；9000：系统磁盘读写错误；9001：网络异常；9002：网络错误；9003：业务网关忙，业务网关缓存；9004：业务网关忙，并且业务网关缓冲区满，MISC 缓存，并暂时不要发送消息，等待一段时间重试；9005：MISC 忙，MISC 缓存；9006：MISC 忙，并且MISC 缓冲区满，业务网关缓存，并暂时不要发送消息，等待一段时间重试；9007：业务网关超过限制的流量；9008：MISC 异常，并不可用；9009：业务网关异常，并不可用；9010：该业务网关没有权限调用该接口消息；9011：MISC 没有权限发送该接口消息给业务网关；9012：版本不支持；9013：消息类型不对，系统不支持；9014：验证错误，无法解析SOAP 和XML 结构、缺少必须存在的字段，或者消息,格式不正确；9015：拒绝消息，服务器无法完成请求的服务）。
        /// </param>
        protected abstract void OnSyncOrderRelationReq(
            string id,
            AddressInfo sendAddr, 
            AddressInfo destAddr, 
            UserID feeUserID, 
            UserID destUserID, 
            string linkID, 
            int actionID, 
            int actionReasonID, 
            string spid, 
            string serviceID, 
            int accessMode, 
            byte[] featureStr, 
            out int hResult);
        #endregion

        #region 公有方法
        /// <summary>
        /// MISC 因为某种情况（如：用户通过手机短信的方式执行了某操作）更新了用户订购关系（包括：订购、取消、暂停、激活）的时候，通过此函数发起和 SP 的更新订购关系的交互。
        /// </summary>
        /// <param name="Version">
        /// 接口消息的版本号，目前接口消息的版本都为“1.5.0”
        /// </param>
        /// <param name="MsgType">
        /// 消息类型。
        /// </param>
        /// <param name="Send_Address">
        /// 发送方的地址。
        /// </param>
        /// <param name="Dest_Address">
        /// 接收方的地址。
        /// </param>
        /// <param name="FeeUser_ID">
        /// 计费用户标识。
        /// </param>
        /// <param name="DestUser_ID">
        /// 使用用户标识（当使用用户和计费用户为同一用户的时候，FeeUser_ID 和 DestUser_ID 的值相同）。
        /// </param>
        /// <param name="LinkID">
        /// 临时订购关系的事务 ID。
        /// </param>
        /// <param name="ActionID">
        /// 服务状态管理动作代码（1：开通服务；2：停止服务；3：激活服务；4：暂停服务）。
        /// </param>
        /// <param name="ActionReasonID">
        /// 产生服务状态管理动作原因的代码（1：用户发起行为；2：Admin&amp;1860发起行为；3：Boss停机；4：Boss开机；5：Boss过户；6：Boss销户；7：Boss改号；8：扣费失败导致的服务取消；9：其他）。
        /// </param>
        /// <param name="SPID">
        /// SP 的企业代码。
        /// </param>
        /// <param name="SPServiceID">
        /// 业务标识，是数字、字母和符号的组合（长度为 10，SP的业务类型，数字、字母和符号的组合，由SP自定，如图片传情可定为TPCQ，股票查询可定义为11），也叫做计费代码。
        /// </param>
        /// <param name="AccessMode">
        /// 服务的访问方式（1：WEB；2：WAP；3：SMS）。
        /// </param>
        /// <param name="FeatureStr">
        /// 服务订购参数（base64加密），内容是长号码＋空格＋用户发送内容。
        /// </param>
        /// <param name="hRet">
        /// 返回值（0：成功；1：未知错误；2-99：保留；4000：无效的msgtype；4001：无效的action_id；4002：无效的action_reasonid；4003：无效的SP ID；4004：无效的serviceID；4005：无效的pseudocode；4006：无效的accessmode；4007：MISC 同步开通服务,但SP 端已存在订购关系,且状态为开通；4008：MISC 同步开通服务,且SP 端不存在订购关系,但开通服务失败；4009：MISC 同步开通服务,但SP 端已存在订购关系, 且状态为暂停；4010：MISC 同步停止服务, 且SP 端存在订购关系, 但取消服务失败；4011：MISC 同步停止服务, 但SP 端不存在订购关系；4012：MISC 同步暂停服务, 且SP 端存在订购关系, 但暂停服务失败；4013：MISC 同步暂停服务, 但SP 端不存在订购关系；4014：MISC 同步暂停服务, 但SP 端已存在订购关系, 且状态为暂停；4015：MISC 同步激活服务, 但SP 端已存在订购关系, 且状态为开通；4016：MISC 同步激活服务, 但SP 端不存在订购关系；4017：MISC 同步激活服务, 且SP 端存在订购关系, 但激活服务失败；9000：系统磁盘读写错误；9001：网络异常；9002：网络错误；9003：业务网关忙，业务网关缓存；9004：业务网关忙，并且业务网关缓冲区满，MISC 缓存，并暂时不要发送消息，等待一段时间重试；9005：MISC 忙，MISC 缓存；9006：MISC 忙，并且MISC 缓冲区满，业务网关缓存，并暂时不要发送消息，等待一段时间重试；9007：业务网关超过限制的流量；9008：MISC 异常，并不可用；9009：业务网关异常，并不可用；9010：该业务网关没有权限调用该接口消息；9011：MISC 没有权限发送该接口消息给业务网关；9012：版本不支持；9013：消息类型不对，系统不支持；9014：验证错误，无法解析SOAP 和XML 结构、缺少必须存在的字段，或者消息,格式不正确；9015：拒绝消息，服务器无法完成请求的服务）。
        /// </param>
        [WebMethod]
        [SoapHeader("TransactionID", Direction = SoapHeaderDirection.InOut)]
        [SoapDocumentMethod("sim.SyncOrderRelation", RequestElementName = "SyncOrderRelationReq", ResponseElementName = "SyncOrderRelationResp")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SyncOrderRelationReq(
            ref string Version,
            ref string MsgType,
            AddressInfo Send_Address,
            AddressInfo Dest_Address,
            UserID FeeUser_ID,
            UserID DestUser_ID,
            string LinkID,
            int ActionID,
            int ActionReasonID,
            string SPID,
            string SPServiceID,
            int AccessMode,
            byte[] FeatureStr,
            out int hRet)
        {

            Version = "1.5.0";
            MsgType = "SyncOrderRelationResp";

            OnSyncOrderRelationReq(
                TransactionID == null ? null : TransactionID.ID,
                Send_Address,
                Dest_Address,
                FeeUser_ID,
                DestUser_ID,
                LinkID,
                ActionID,
                ActionReasonID,
                SPID, SPServiceID,
                AccessMode,
                FeatureStr,
                out hRet);
        }
        #endregion

    }
}
