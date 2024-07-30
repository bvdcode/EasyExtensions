using System;
using System.Collections.Generic;

namespace EasyExtensions.Collections.Concurrent
{
    /// <summary>
    /// Provides a first in-first out (FIFO) collection with a limit.
    /// </summary>
    /// <typeparam name="T">The type of the elements contained in the queue.</typeparam>
    public class LimitedQueue<T> : Queue<T>
    {
        private readonly int _limit;

        /// <summary>
        /// Creates a new instance of the <see cref="LimitedQueue{T}"/> class with the specified limit.
        /// </summary>
        /// <param name="limit">Queue limit.</param>
        public LimitedQueue(int limit) : base()
        {
            _limit = limit;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LimitedQueue{T}"/> class with the specified limit and elements copied from the specified collection.
        /// </summary>
        /// <param name="limit">Queue limit.</param>
        /// <param name="collection">The collection whose elements are copied to the new <see cref="LimitedQueue{T}"/>.</param>
        public LimitedQueue(int limit, IEnumerable<T> collection) : base(collection)
        {
            _limit = limit;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LimitedQueue{T}"/> class with the specified limit and capacity.
        /// </summary>
        /// <param name="limit">Queue limit.</param>
        /// <param name="capacity">The initial number of elements that the <see cref="LimitedQueue{T}"/> can contain.</param>
        public LimitedQueue(int limit, int capacity) : base(capacity)
        {
            _limit = limit;
            if (capacity > limit)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity cannot be greater than the limit.");
            }
        }

        /// <summary>
        /// Enqueues an object. If the queue exceeds the limit, the oldest object is dequeued.
        /// </summary>
        /// <param name="item">The object to add to the queue.</param>
        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            while (Count > _limit && TryDequeue(out _)) ;
        }
    }
}