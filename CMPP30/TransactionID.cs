using System;
using System.ComponentModel;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;

namespace CMPP
{
    /// <summary>
    /// SOAP 头验证信息（DSMG 调用该 WEB 服务时需要设置该信息）。
    /// </summary>
    [XmlRoot(Namespace = "http://www.monternet.com/dsmp/schemas/")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TransactionID : SoapHeader
    {
        /// <summary>
        /// 该消息编号。
        /// </summary>
        [XmlText(typeof(string))]
        public string ID;
    }
}
