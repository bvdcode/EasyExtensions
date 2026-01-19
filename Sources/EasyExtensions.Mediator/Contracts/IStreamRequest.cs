// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Mediator.Contracts
{
    /// <summary>
    /// Represents a request that expects a stream of responses of the specified type.
    /// </summary>
    /// <remarks>This interface is typically used to define requests in systems that support streaming
    /// responses, such as event-driven or real-time data processing scenarios. Implementations may use this interface
    /// to indicate that a request should be handled in a way that produces multiple results over time rather than a
    /// single response.</remarks>
    /// <typeparam name="TResponse">The type of the response elements returned by the stream.</typeparam>
    public interface IStreamRequest<out TResponse> { }
}
