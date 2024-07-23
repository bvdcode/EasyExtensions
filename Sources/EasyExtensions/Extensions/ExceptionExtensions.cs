using System;
using System.Text;

namespace EasyExtensions
{
    /// <summary>
    /// <see cref="Exception"/> extensions.
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Create string with error message from all inner exceptions if exists.
        /// </summary>
        /// <returns> Error message. </returns>
        public static string ToStringWithInner(this Exception ex)
        {
            StringBuilder exceptionMessage = new StringBuilder();
            const int maxDepth = 1000;
            int counter = 0;
            while (true)
            {
                exceptionMessage.Append(ex.GetType().Name);
                exceptionMessage.Append(": ");
                exceptionMessage.Append(ex.Message);
                exceptionMessage.Append(Environment.NewLine);
                if (ex.InnerException != null)
                {
                    exceptionMessage.Append("- ");
                    ex = ex.InnerException;
                }
                else
                {
                    break;
                }
                counter++;
                if (counter > maxDepth)
                {
                    exceptionMessage.Append("... too much inner exceptions: " + counter);
                    break;
                }
            }
            return exceptionMessage.ToString();
        }
    }
}