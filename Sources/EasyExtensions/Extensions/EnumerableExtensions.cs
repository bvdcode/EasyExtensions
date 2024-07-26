using System;
using System.Collections.Generic;

namespace EasyExtensions
{
    /// <summary>
    /// <see cref="IEnumerable{T}"/> extensions."/>
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Randomizes the order of the elements in the collection.
        /// </summary>
        /// <typeparam name="TItem"> Type of the items in the collection. </typeparam>
        /// <param name="enumerable"> Collection to randomize. </param>
        /// <returns> Randomized collection. </returns>
        public static IEnumerable<TItem> Random<TItem>(this IEnumerable<TItem> enumerable)
        {
            Random random = new Random();
            List<TItem> list = new List<TItem>(enumerable);
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                int index = random.Next(count);
                yield return list[index];
                list.RemoveAt(index);
                count--;
            }
        }
    }
}
