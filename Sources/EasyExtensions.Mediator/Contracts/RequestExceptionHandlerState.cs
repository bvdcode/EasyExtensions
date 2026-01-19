// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Mediator.Contracts
{
    /// <summary>
    /// Represents the state of an exception handler for a request, including whether the exception has been handled and
    /// an optional response value.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response associated with the handled request. Must be a reference type.</typeparam>
    public class RequestExceptionHandlerState<TResponse>
    {
        /// <summary>
        /// Gets a value indicating whether the event has been marked as handled.
        /// </summary>
        public bool Handled { get; private set; }

        /// <summary>
        /// Gets the response returned by the operation, if available.
        /// </summary>
        public TResponse Response { get; private set; } = default!;

        /// <summary>
        /// Marks the request as handled and sets the response to the specified value.
        /// </summary>
        /// <param name="response">The response value to associate with the handled request.</param>
        public void SetHandled(TResponse response)
        {
            Handled = true;
            Response = response;
        }
    }
}
