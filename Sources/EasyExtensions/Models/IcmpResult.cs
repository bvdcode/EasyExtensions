using EasyExtensions.Models.Enums;

namespace EasyExtensions.Models
{
    /// <summary>
    /// Result of an ICMP ping operation.
    /// </summary>
    public class IcmpResult
    {
        /// <summary>
        /// Gets the status of the ICMP ping operation.
        /// </summary>
        public IcmpStatus Status { get; }

        /// <summary>
        /// Gets the round-trip time in milliseconds.
        /// </summary>
        public long RoundtripTime { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IcmpResult"/> class.
        /// </summary>
        public IcmpResult(IcmpStatus status, long roundtripTime)
        {
            Status = status;
            RoundtripTime = roundtripTime;
        }
    }
}
