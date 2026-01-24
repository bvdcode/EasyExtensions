using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Mediator.Pipeline
{
    /// <summary>
    /// Behavior for executing all <see cref="IRequestPreProcessor{TRequest}"/> instances before handling a request
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class RequestPreProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    {
        private readonly IEnumerable<IRequestPreProcessor<TRequest>> _preProcessors;

        /// <summary>
        /// Initializes a new instance of the RequestPreProcessorBehavior class with the specified collection of request
        /// pre-processors.
        /// </summary>
        /// <param name="preProcessors">The collection of pre-processors to apply to the request. Cannot be null.</param>
        public RequestPreProcessorBehavior(IEnumerable<IRequestPreProcessor<TRequest>> preProcessors)
            => _preProcessors = preProcessors;

        /// <summary>
        /// Handles the specified request by executing all registered pre-processors before invoking the next handler in
        /// the pipeline.
        /// </summary>
        /// <remarks>All pre-processors are executed in order before the main handler is called. If the
        /// operation is canceled via the cancellation token, processing will be aborted.</remarks>
        /// <param name="request">The request message to be processed. Cannot be null.</param>
        /// <param name="next">A delegate representing the next handler to invoke in the processing pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response produced by the
        /// handler pipeline.</returns>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            foreach (var processor in _preProcessors)
            {
                await processor.Process(request, cancellationToken).ConfigureAwait(false);
            }

            return await next(cancellationToken).ConfigureAwait(false);
        }
    }
}