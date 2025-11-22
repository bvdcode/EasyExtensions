// SPDX-License-Identifier: AGPL-3.0-only
// Copyright (c) 2025 Vadim Belov

using System.Collections.Generic;

namespace EasyExtensions.Crypto.Internals
{
    internal sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        public static ReferenceEqualityComparer<T> Instance { get; } = new();
        public bool Equals(T? x, T? y) => ReferenceEquals(x, y);
        public int GetHashCode(T obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
