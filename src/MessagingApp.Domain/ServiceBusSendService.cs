using Azure.Messaging.ServiceBus;
using MessagingApp.Domain.Interface;

namespace MessagingApp.Domain;
public class ServiceBusSendService : IServiceBusSendService
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly IList<string> _messages;

    public ServiceBusSendService(string connectionString, string queuename)
    {
        ServiceBusClientOptions options = new(){
            TransportType = ServiceBusTransportType.AmqpWebSockets
        };
        _client = new ServiceBusClient(connectionString, options);
        _sender = _client.CreateSender(queuename);
        _messages = new List<string>();         
    }

    public IServiceBusSendService AddMessage(string message)
    {
        if(message.ToCharArray().Length > 2048)
            throw new ArgumentOutOfRangeException("Message", "Message size is too long");
        _messages.Add(message);
        return this;
    }

    public IServiceBusSendService CreateConnection()
    {
        return this;
    }

    public async Task Dispose()
    {
        await _sender.CloseAsync();
        await _sender.DisposeAsync();
    }

    public async Task SendMessage()
    {
        ServiceBusMessageBatch messageBatch = await _sender.CreateMessageBatchAsync();
        foreach (var message in _messages)
        {
            if(!messageBatch.TryAddMessage(new ServiceBusMessage(message.ToString())))
            {
                throw new Exception("Message too long to fit in batch" + message.ToString());
            }
        }

        await _sender.SendMessagesAsync(messageBatch);  
    }
}
