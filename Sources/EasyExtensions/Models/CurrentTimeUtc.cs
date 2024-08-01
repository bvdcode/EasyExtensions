﻿using System;

namespace EasyExtensions.Models
{
    /// <summary>
    /// Wrapper for current UTC time.
    /// </summary>
    public class CurrentTimeUtc
    {
        /// <summary>
        /// Current UTC time.
        /// </summary>
        public DateTime CurrentUtc { get; set; } = DateTime.UtcNow;
    }
}
