using System;

namespace EasyExtensions.Extensions
{
    /// <summary>
    /// Numeric fast extensions with basic <see cref="Math"/> methods.
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// Pow specified foundation to exponent.
        /// </summary>
        /// <param name="number"> Foundation. </param>
        /// <param name="exponent"> Exponent of pow. </param>
        /// <returns> Calculation result. </returns>
        /// <exception cref="OverflowException"> Throws when calculation result is too big. </exception>
        public static int Pow(this int number, int exponent)
        {
            checked
            {
                int result = number;
                for (int i = 1; i < exponent; i++)
                {
                    result *= number;
                }
                return result;
            }
        }
    }
}