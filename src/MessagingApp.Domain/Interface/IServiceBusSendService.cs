using Azure.Messaging.ServiceBus;

namespace MessagingApp.Domain.Interface;

public interface IServiceBusSendService
{
    Task SendMessage();
    IServiceBusSendService CreateConnection();
    IServiceBusSendService AddMessage(string message);
    Task Dispose();

}
