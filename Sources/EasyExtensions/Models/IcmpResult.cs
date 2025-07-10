using System;
using EasyExtensions.Models.Enums;

namespace EasyExtensions.Models
{
    /// <summary>
    /// Result of an ICMP ping operation.
    /// </summary>
    public class IcmpResult
    {
        /// <summary>
        /// Gets a value indicating whether the ping operation was successful.
        /// </summary>
        public bool IsSuccess => Status == IcmpStatus.Success;

        /// <summary>
        /// Gets the status of the ICMP ping operation.
        /// </summary>
        public IcmpStatus Status { get; }

        /// <summary>
        /// Gets the round-trip time in milliseconds.
        /// </summary>
        public long RoundtripTime { get; }

        /// <summary>
        /// Gets or sets the exception that occurred during the ping operation, if any.
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IcmpResult"/> class.
        /// </summary>
        public IcmpResult(IcmpStatus status, long roundtripTime, Exception? exception = null)
        {
            Status = status;
            Exception = exception;
            RoundtripTime = roundtripTime;
        }
    }
}
