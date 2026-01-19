// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System;
using System.IO;

namespace EasyExtensions.Crypto.Internals.Pipelines
{
    internal sealed class ReorderBuffer<T>
    {
        private readonly int _windowCap;
        private readonly Func<T, long> _indexSelector;
        private int _window;
        private T[] _ring;
        private bool[] _filled;
        private long[] _slotIndex;
        public long Next { get; private set; }

        public ReorderBuffer(int threads, int windowCap, Func<T, long> indexSelector)
        {
            _windowCap = windowCap;
            _indexSelector = indexSelector ?? throw new ArgumentNullException(nameof(indexSelector));
            const int minWindow = 4;
            _window = Math.Min(Math.Max(minWindow, threads * 4), _windowCap);
            _ring = new T[_window];
            _filled = new bool[_window];
            _slotIndex = new long[_window];
            Next = 0;
        }

        public void EnsureCapacity(long neededIndex)
        {
            if (neededIndex - Next < _window) return;
            int newWindow = Math.Min(_window * 2, _windowCap);
            while (neededIndex - Next >= newWindow && newWindow < _windowCap)
            {
                newWindow = Math.Min(newWindow * 2, _windowCap);
            }
            var newRing = new T[newWindow];
            var newFilled = new bool[newWindow];
            var newSlotIndex = new long[newWindow];
            for (int i = 0; i < _window; i++)
            {
                if (!_filled[i]) continue;
                long idx = _slotIndex[i];
                int newSlot = (int)(idx % newWindow);
                newRing[newSlot] = _ring[i];
                newFilled[newSlot] = true;
                newSlotIndex[newSlot] = idx;
            }
            _ring = newRing; _filled = newFilled; _slotIndex = newSlotIndex; _window = newWindow;
        }

        public void Put(T item)
        {
            long idx = _indexSelector(item);
            if (idx < Next)
            {
                throw new InvalidDataException($"Duplicate chunk index detected. Received {idx}, next expected {Next}.");
            }
            EnsureCapacity(idx);
            int slot = (int)(idx % _window);
            if (_filled[slot])
            {
                throw new InvalidDataException($"Reorder buffer slot collision. Slot {slot} already filled for index {_slotIndex[slot]}, tried to place {idx}.");
            }
            _ring[slot] = item; _slotIndex[slot] = idx; _filled[slot] = true;
        }

        public bool TryPopNext(out T item)
        {
            int slot = (int)(Next % _window);
            if (_filled[slot] && _slotIndex[slot] == Next)
            {
                item = _ring[slot];
                _filled[slot] = false;
                Next++;
                return true;
            }
            item = default!;
            return false;
        }

        public void RecycleLeftovers(Action<T> recycler)
        {
            for (int i = 0; i < _window; i++)
            {
                if (_filled[i])
                {
                    recycler(_ring[i]);
                    _filled[i] = false;
                }
            }
        }
    }
}
