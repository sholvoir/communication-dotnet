namespace CMPP
{
    /// <summary>
    /// 计费用户。
    /// </summary>
    public enum FeeUserType : byte
    {
        /// <summary>
        /// 对源终端计费。
        /// </summary>
        From = 1,
        /// <summary>
        /// 对目的终端计费。
        /// </summary>
        Termini = 0,
        /// <summary>
        /// 对 SP 计费。
        /// </summary>
        SP = 2,
        /// <summary>
        /// 对指定用户计费（由 feeUser 指定）。
        /// </summary>
        FeeUser = 3
    }
}
