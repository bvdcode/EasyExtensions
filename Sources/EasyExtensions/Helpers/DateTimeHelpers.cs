using System;
using System.Linq;

namespace EasyExtensions.Helpers
{
    /// <summary>
    /// <see cref="DateTime"/> helpers.
    /// </summary>
    public static class DateTimeHelpers
    {
        /// <summary>
        /// Parse DateTime from JSON format.
        /// </summary>
        /// <returns> Parsed datetime with UTC kind. </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public static DateTime ParseDateTime(string datetime)
        {
            return ParseDateTimeOffset(datetime).UtcDateTime;
        }

        /// <summary>
        /// Parse DateTimeOffset from JSON format.
        /// </summary>
        /// <returns> Parsed datetime offset. </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FormatException"></exception>
        public static DateTimeOffset ParseDateTimeOffset(string datetime)
        {
            // 2023-10-19T06:34:22.664719+00:00
            // 2023-10-19T06:34:22.664719 00:00
            // 2023-10-19T06:34:22.664719 00
            // 2023-10-19T06:34:22.664Z

            const string offsetDelimiters = " Zz+-";

            if (string.IsNullOrWhiteSpace(datetime))
            {
                throw new ArgumentException($"'{nameof(datetime)}' cannot be null or whitespace.", nameof(datetime));
            }

            string[] parts = datetime
                .Trim()
                .Replace('t', 'T')
                .Split('T');

            if (parts.Length != 2)
            {
                throw new FormatException($"Specified datetime was incorrect format: {datetime}, must contains 'T' delimiter.");
            }

            string datePart = parts[0];
            string timePart = parts[1];
            DateTime date = ParseDate(datePart);
            DateTime time = ParseTime(timePart);
            TimeSpan offset = timePart.Any(offsetDelimiters.Contains) ? ParseOffset(timePart) : TimeSpan.Zero;
            TimeSpan milliseconds = ParseSmallSeconds(timePart);
            var result = new DateTimeOffset(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, offset);
            result = result.Add(milliseconds);
            return result;
        }

        private static TimeSpan ParseOffset(string timePart)
        {
            if (timePart.Contains('Z') || timePart.Contains('z'))
            {
                return TimeSpan.Zero;
            }
            timePart = timePart.Trim().Replace(' ', '+');
            int index = timePart.IndexOfAny(new char[] { '+', '-' });
            if (index <= 0)
            {
                return TimeSpan.Zero;
            }
            bool plus = timePart.Contains('+');
            string offsetStr = timePart[(index + 1)..];
            if (!offsetStr.Contains(':'))
            {
                bool parsed = int.TryParse(offsetStr, out int offset);
                if (!parsed)
                {
                    throw new FormatException($"Offset was incorrect format: {timePart} ({offsetStr}).");
                }
                return plus ? TimeSpan.FromHours(offset) : TimeSpan.FromHours(-offset);
            }
            else
            {
                string[] parts = offsetStr.Split(':');
                bool parsed = int.TryParse(parts[0], out int offsetHours);
                if (!parsed)
                {
                    throw new FormatException($"Offset was incorrect format: {timePart} ({offsetStr}).");
                }
                parsed = int.TryParse(parts[1], out int offsetMinutes);
                if (!parsed)
                {
                    throw new FormatException($"Offset was incorrect format: {timePart} ({offsetStr}).");
                }
                TimeSpan result = TimeSpan.Zero;
                if (plus)
                {
                    return result
                        .Add(TimeSpan.FromMinutes(offsetMinutes))
                        .Add(TimeSpan.FromHours(offsetHours));
                }
                else
                {
                    return result
                        .Add(TimeSpan.FromMinutes(-offsetMinutes))
                        .Add(TimeSpan.FromHours(-offsetHours));
                }
            }
        }

        private static TimeSpan ParseSmallSeconds(string timePart)
        {
            int index = timePart.IndexOf('.');
            if (index <= 0)
            {
                return TimeSpan.Zero;
            }
            string ms = timePart[(index + 1)..];
            index = ms.IndexOfAny(new char[] { '.', '.', '+', '-', ' ', 'Z', 'z' });
            if (index > 0)
            {
                ms = ms[0..index];
            }
            bool parsed = int.TryParse(ms, out int result);
            if (!parsed || result < 0)
            {
                throw new FormatException($"Milliseconds was incorrect format: {timePart} ({ms}).");
            }
            return TimeSpan.Parse($"00:00:00.{result}");
        }

        private static DateTime ParseTime(string timePart)
        {
            string[] parts = timePart.Split(':');
            bool parsed = int.TryParse(parts[0], out int hour);
            if (!parsed || hour < 0 || hour > 23)
            {
                throw new FormatException($"Hour was incorrect format: {timePart} ({parts[0]}).");
            }
            parsed = int.TryParse(parts[1], out int minute);
            if (!parsed || minute < 0 || minute > 59)
            {
                throw new FormatException($"Minute was incorrect format: {timePart} ({parts[1]}).");
            }
            string secondsStr = parts[2];
            int index = secondsStr.IndexOfAny(new char[] { '.', '.', '+', '-', ' ', 'Z', 'z' });
            if (index > 0)
            {
                secondsStr = secondsStr[0..index];
            }

            parsed = int.TryParse(secondsStr, out int seconds);
            if (!parsed || seconds < 0 || seconds > 59)
            {
                throw new FormatException($"Seconds was incorrect format: {timePart} ({secondsStr}).");
            }
            return new DateTime(1, 1, 1, hour, minute, seconds);
        }

        private static DateTime ParseDate(string datePart)
        {
            string[] parts = datePart.Split('-');
            bool parsed = int.TryParse(parts[0], out int year);
            if (!parsed || year < 0)
            {
                throw new FormatException($"Year was incorrect format: {datePart} ({parts[0]}).");
            }
            parsed = int.TryParse(parts[1], out int month);
            if (!parsed || month <= 0 || month > 12)
            {
                throw new FormatException($"Month was incorrect format: {datePart} ({parts[1]}).");
            }
            parsed = int.TryParse(parts[2], out int day);
            if (!parsed || day <= 0 || day > 31)
            {
                throw new FormatException($"Day was incorrect format: {datePart} ({parts[2]}).");
            }
            return new DateTime(year, month, day, 1, 1, 1);
        }
    }
}