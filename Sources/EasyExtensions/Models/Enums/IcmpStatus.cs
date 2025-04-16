namespace EasyExtensions.Models.Enums
{
    /// <summary>
    /// Status of an ICMP ping operation.
    /// </summary>
    public enum IcmpStatus
    {
        /// <summary>
        /// The ping was successful.
        /// </summary>
        Success,

        /// <summary>
        /// The ping timed out.
        /// </summary>
        Timeout,

        /// <summary>
        /// An error occurred during the ping operation.
        /// </summary>
        Failed
    }
}
