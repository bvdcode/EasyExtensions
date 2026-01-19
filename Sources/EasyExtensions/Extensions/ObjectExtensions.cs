// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions
{
    /// <summary>
    /// Object extensions.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Clone object with MemberwiseClone.
        /// </summary>
        /// <typeparam name="TObj"> Type of object. </typeparam>
        /// <param name="obj"> Object to clone. </param>
        /// <returns> Cloned object. </returns>
        public static TObj MemberwiseClone<TObj>(this TObj obj)
        {
            if (obj == null)
            {
                return default!;
            }
            return (TObj)obj
                .GetType()
                .GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(obj, null)!;
        }
    }
}
