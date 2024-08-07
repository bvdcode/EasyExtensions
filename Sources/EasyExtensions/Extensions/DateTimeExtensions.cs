using System;

namespace EasyExtensions
{
    /// <summary>
    /// <see cref="DateTime"/> extensions.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Convert <see cref="DateTime"/> to Unix timestamp in milliseconds.
        /// </summary>
        /// <returns> Unix timestamp in milliseconds. </returns>
        /// <exception cref="ArgumentException"> DateTime value must be in UTC timezone. </exception>
        public static long ToUnixTimestampMilliseconds(this DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("DateTime value must be in UTC timezone.");
            }
            DateTimeOffset dateTimeOffset = new DateTimeOffset(value);
            return dateTimeOffset.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Convert <see cref="DateTime"/> to Unix timestamp in seconds.
        /// </summary>
        /// <returns> Unix timestamp in seconds. </returns>
        /// <exception cref="ArgumentException"> DateTime value must be in UTC timezone. </exception>
        public static int ToUnixTimestampSeconds(this DateTime value)
        {
            return (int)(value.ToUnixTimestampMilliseconds() / 1000);
        }

        /// <summary>
        /// Remove milliseconds from <see cref="DateTime"/>.
        /// </summary>
        /// <returns> DateTime without milliseconds. </returns>
        public static DateTime DropMilliseconds(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Kind);
        }

        /// <summary>
        /// Remove milliseconds from <see cref="DateTime"/>.
        /// </summary>
        /// <returns> DateTime without milliseconds. </returns>
        public static DateTimeOffset DropMilliseconds(this DateTimeOffset value)
        {
            return new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Offset);
        }

        /// <summary>
        /// Create new datetime with same values but <see cref="DateTimeKind.Utc"/>.
        /// </summary>
        /// <returns> New datetime. </returns>
        public static DateTime ToUniversalTimeWithoutOffset(this DateTime value)
        {
            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        /// <summary>
        /// Convert datetime value to nullable datetime type.
        /// </summary>
        /// <returns> Wrapped datetime value. </returns>
        public static DateTime? ToNullable(this DateTime value)
        {
            DateTime? result = new DateTime?(value);
            return result;
        }
    }
}