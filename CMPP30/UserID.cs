namespace CMPP
{
    /// <summary>
    /// PROVISION 接口定义的用户标识实体。
    /// </summary>
    public class UserID
    {
        /// <summary>
        /// 用户标识类型（1：用手机号标识；2：用伪码标识；3：两者同时标识）。
        /// </summary>
        public int UserIDType;
        /// <summary>
        /// 用户手机号。
        /// </summary>
        public string MSISDN;
        /// <summary>
        /// 用户伪码。
        /// </summary>
        public string PseudoCode;
    }
}
