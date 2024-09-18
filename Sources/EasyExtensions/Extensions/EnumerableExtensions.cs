using System;
using System.Linq;
using System.Collections.Generic;

namespace EasyExtensions.Extensions
{
    /// <summary>
    /// <see cref="IEnumerable{T}"/> extensions."/>
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Shuffles the elements of the <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the <see cref="IEnumerable{T}"/>.</typeparam>
        /// <param name="source">The <see cref="IEnumerable{T}"/> to shuffle.</param>
        /// <returns>A shuffled <see cref="IEnumerable{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            var random = new Random();
            T[] elements = source.ToArray();
            for (int i = elements.Length - 1; i >= 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }

        /// <summary>
        /// Gets a random element from the <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the <see cref="IEnumerable{T}"/>.</typeparam>
        /// <param name="enumerable">The <see cref="IEnumerable{T}"/> to get a random element from.</param>
        /// <returns>A random element from the <see cref="IEnumerable{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="enumerable"/> is null.</exception>
        public static T Random<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }
            var random = new Random();
            var list = enumerable as IList<T> ?? enumerable.ToList();
            return list.Count == 0 ? default! : list[random.Next(0, list.Count)];
        }
    }
}
