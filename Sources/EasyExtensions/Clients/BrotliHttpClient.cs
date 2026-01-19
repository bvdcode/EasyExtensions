// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Handlers;
using System.Net.Http;

namespace EasyExtensions.Clients
{
    /// <summary>
    /// BrotliHttpClient is a HttpClient that supports Brotli compression.
    /// </summary>
    public class BrotliHttpClient : HttpClient
    {
        /// <summary>
        /// Creates a new instance of <see cref="BrotliHttpClient"/>.
        /// </summary>
        public BrotliHttpClient() : base(new BrotliCompressionHandler()) { }
    }
}
