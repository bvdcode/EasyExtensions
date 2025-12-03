namespace EasyExtensions.Mediator.Contracts
{
    /// <summary>
    /// Defines a mediator that coordinates sending requests and publishing notifications between components.
    /// </summary>
    /// <remarks>Implementations of this interface typically provide a central point for handling commands,
    /// queries, and events, decoupling the sender from the receiver. IMediator combines the capabilities of ISender and
    /// IPublisher, allowing both request/response and publish/subscribe patterns. Thread safety and lifetime management
    /// depend on the specific implementation.</remarks>
    public interface IMediator : ISender, IPublisher { }
}
