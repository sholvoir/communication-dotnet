namespace CMPP
{
    /// <summary>
    /// 资费类别。
    /// </summary>
    public enum FeeType : byte
    {
        /// <summary>
        /// 对“计费用户号码”免费。
        /// </summary>
        Free = 1,
        /// <summary>
        /// 对“计费用户号码”按条计信息费。
        /// </summary>
        One = 2,
        /// <summary>
        /// 对“计费用户号码”按包月收取信息费。
        /// </summary>
        Month = 3
    }
}
