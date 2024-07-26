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
        public static string ToStringWithInner(this Exception exception)
        {
            StringBuilder exceptionMessage = new StringBuilder();
            const int maxDepth = 1000;
            int counter = 0;
            while (true)
            {
                exceptionMessage.Append(exception.GetType().Name);
                exceptionMessage.Append(": ");
                exceptionMessage.Append(exception.Message);
                exceptionMessage.Append(Environment.NewLine);
                if (exception.InnerException != null)
                {
                    exceptionMessage.Append("- ");
                    exception = exception.InnerException;
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