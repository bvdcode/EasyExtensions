using System;

namespace EasyExtensions
{
    /// <summary>
    /// <see cref="DateTime"/> extensions.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Remove microseconds from <see cref="DateTime"/>.
        /// </summary>
        /// <returns> DateTime without microseconds. </returns>
        public static DateTime DropMicroseconds(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Kind);
        }

        /// <summary>
        /// Remove microseconds from <see cref="DateTime"/>.
        /// </summary>
        /// <returns> DateTime without microseconds. </returns>
        public static DateTimeOffset DropMicroseconds(this DateTimeOffset value)
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