using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyExtensions.Extensions
{
    /// <summary>
    /// Provides extension methods for working with enumeration (enum) values.
    /// </summary>
    /// <remarks>This static class contains utility methods that extend the functionality of enum types,
    /// enabling more convenient or user-friendly operations when working with enumerations.</remarks>
    public static class EnumExtensions
    {
        /// <summary>
        /// Converts the value of the specified enumeration to a user-friendly string with spaces inserted before
        /// uppercase letters.
        /// </summary>
        /// <remarks>This method is useful for displaying enumeration values in a more readable format,
        /// such as in user interfaces or logs. For example, an enum value named "OrderStatusPendingApproval" would be
        /// converted to "Order Status Pending Approval".</remarks>
        /// <param name="e">The enumeration value to convert to a nicely formatted string.</param>
        /// <returns>A string representation of the enumeration value with spaces inserted before uppercase letters. Returns the
        /// enumeration name as-is if no formatting is needed.</returns>
        public static string ToNiceString(this Enum e)
        {
            string enumString = e.ToString();
            IEnumerable<char> chars = enumString.SelectMany((c, i) =>
            {
                if (i > 0 && char.IsUpper(c))
                {
                    return new char[] { ' ', c };
                }
                return new[] { c };
            });
            return new string(chars.ToArray());
        }
    }
}
