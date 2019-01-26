namespace CMPP
{
    /// <summary>
    /// PROVISION 接口定义的地址信息实体。
    /// </summary>
    public class AddressInfo
    {
        /// <summary>
        /// 设备类型（0：DSMP；100：ISMG；101：WAP SP PROXY；1XX：其他业务网关；200：WAP PORTAL；201：WWW PORTAL；202：VOICE PORTAL；203：PDA PORTAL；2XX：其他门户；300：MMSC；301：KJAVA SERVER；302：LSP；3XX：其它应用平台；400：SP）。
        /// </summary>
        public int DeviceType;
        /// <summary>
        /// 设备编号，设备编号采用各设备的入网编号，例如短信网关使用网关ID、对SP使用其企业代码，该设备编号由MISC分配，并且在同一设备类型中该编号唯一。
        /// </summary>
        public string DeviceID;
    }
}
