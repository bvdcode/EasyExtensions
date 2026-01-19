// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Mediator.Contracts
{
    /// <summary>
    /// Defines a contract for processing a request asynchronously before it is handled.
    /// </summary>
    /// <remarks>Implement this interface to perform pre-processing logic on requests, such as validation,
    /// logging, or modification, before the main request handler executes. Pre-processors can be used to enforce
    /// cross-cutting concerns or prepare requests for further handling.</remarks>
    /// <typeparam name="TRequest">The type of the request to be processed. Must not be null.</typeparam>
    public interface IRequestPreProcessor<in TRequest> where TRequest : notnull
    {
        /// <summary>
        /// Processes the specified request asynchronously.
        /// </summary>
        /// <param name="request">The request to be processed. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous processing operation.</returns>
        Task Process(TRequest request, CancellationToken cancellationToken = default);
    }
}
