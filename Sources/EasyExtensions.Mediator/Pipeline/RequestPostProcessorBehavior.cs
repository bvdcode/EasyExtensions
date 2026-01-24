using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Mediator.Pipeline
{
    /// <summary>
    /// Behavior for executing all <see cref="IRequestPostProcessor{TRequest,TResponse}"/> instances after handling the request
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public class RequestPostProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    {
        private readonly IEnumerable<IRequestPostProcessor<TRequest, TResponse>> _postProcessors;

        /// <summary>
        /// Initializes a new instance of the RequestPostProcessorBehavior class with the specified post-processors.
        /// </summary>
        /// <param name="postProcessors">The collection of post-processors to apply to the request and response. Cannot be null.</param>
        public RequestPostProcessorBehavior(IEnumerable<IRequestPostProcessor<TRequest, TResponse>> postProcessors)
            => _postProcessors = postProcessors;

        /// <summary>
        /// Handles the specified request by invoking the next handler in the pipeline and executing all configured
        /// post-processors.
        /// </summary>
        /// <remarks>All registered post-processors are executed after the main handler completes. If the
        /// operation is canceled via the cancellation token, post-processors may not run.</remarks>
        /// <param name="request">The request message to be handled. Cannot be null.</param>
        /// <param name="next">A delegate representing the next handler in the pipeline. Invoked to obtain the response for the request.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response produced by the
        /// pipeline after all post-processors have executed.</returns>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var response = await next(cancellationToken).ConfigureAwait(false);

            foreach (var processor in _postProcessors)
            {
                await processor.Process(request, response, cancellationToken).ConfigureAwait(false);
            }

            return response;
        }
    }
}