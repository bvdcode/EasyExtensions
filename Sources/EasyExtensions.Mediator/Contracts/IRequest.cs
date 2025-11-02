namespace EasyExtensions.Mediator.Contracts
{
    /// <summary>
    /// Represents a request message in the API infrastructure.
    /// </summary>
    /// <remarks>This interface serves as a marker for request types that are processed by the API pipeline.
    /// Implementing this interface allows a type to be recognized as a request and handled accordingly by request handlers
    /// or middleware components. It extends the <see cref="IBaseRequest"/> interface to provide additional context or
    /// categorization for request objects.</remarks>
    public interface IRequest : IBaseRequest { }

    /// <summary>
    /// Defines a request that returns a response of the specified type when handled.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response returned by the request.</typeparam>
    public interface IRequest<out TResponse> : IBaseRequest { }
}
