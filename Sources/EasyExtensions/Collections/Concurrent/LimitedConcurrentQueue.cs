// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EasyExtensions.Collections.Concurrent
{
    /// <summary>
    /// Provides a thread-safe first in-first out (FIFO) collection with a limit.
    /// </summary>
    /// <typeparam name="T">The type of the elements contained in the queue.</typeparam>
    public class LimitedConcurrentQueue<T> : ConcurrentQueue<T>
    {
        private readonly int _limit;

        /// <summary>
        /// Creates a new instance of the <see cref="LimitedConcurrentQueue{T}"/> class with the specified limit.
        /// </summary>
        /// <param name="limit">Queue limit.</param>
        public LimitedConcurrentQueue(int limit) : base()
        {
            _limit = limit;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LimitedConcurrentQueue{T}"/> class with the specified limit and elements copied from the specified collection.
        /// </summary>
        /// <param name="limit">Queue limit.</param>
        /// <param name="collection">The collection whose elements are copied to the new <see cref="LimitedConcurrentQueue{T}"/>.</param>
        public LimitedConcurrentQueue(int limit, IEnumerable<T> collection) : base(collection)
        {
            _limit = limit;
        }

        /// <summary>
        /// Enqueues an object. If the queue exceeds the limit, the oldest object is dequeued.
        /// </summary>
        /// <param name="item">The object to add to the queue.</param>
        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            while (Count > _limit)
            {
                TryDequeue(out _);
            }
        }
    }
}
