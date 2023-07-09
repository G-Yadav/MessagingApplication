using Azure.Messaging.ServiceBus;

namespace MessagingApp.Domain.Interface;

public interface IServiceBusReceiveService
{
    Task ReceiveMessage();
    IServiceBusReceiveService CreateConnection();
    IServiceBusReceiveService SetHandlers(Handlers handlers, Func<ProcessMessageEventArgs,Task>? messageHandler = null,  Func<ProcessErrorEventArgs,Task>? errorHandler = null);
    Task Dispose();

}

public enum Handlers {
    Default,
    Custom
}
