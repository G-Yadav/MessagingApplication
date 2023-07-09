using Azure.Messaging.ServiceBus;
using MessagingApp.Domain.Interface;

namespace MessagingApp.Domain;

public class ServiceBusReceiveService : IServiceBusReceiveService
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusProcessor _processor;

    public ServiceBusReceiveService(string connectionString, string queueName)
    {
        ServiceBusClientOptions options = new () {
            TransportType = ServiceBusTransportType.AmqpWebSockets
        };
        _client = new ServiceBusClient(connectionString,options);
        _processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions());
        
    }

    public IServiceBusReceiveService CreateConnection()
    {
        return this;
    }

    public async Task Dispose()
    {
        await _processor.DisposeAsync();
        await _client.DisposeAsync();
    }

    public async Task ReceiveMessage()
    {
        await _processor.StartProcessingAsync();

        Console.Read();

        Console.WriteLine("Stopping the receiver ...");
        await _processor.StopProcessingAsync();
        Console.WriteLine("Stopped the receiver!");
    }

    public IServiceBusReceiveService SetHandlers(Handlers handlers, Func<ProcessMessageEventArgs, Task>? messageHandler = null, Func<ProcessErrorEventArgs, Task>? errorHandler = null)
    {
        if(handlers == Handlers.Custom) {
            _processor.ProcessMessageAsync += messageHandler;
            _processor.ProcessErrorAsync += errorHandler;
        }
        else {
            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;
        }

        return this;
    }

    private async Task MessageHandler(ProcessMessageEventArgs args) {
        string body = args.Message.Body.ToString();
        Console.WriteLine($"Received: {body}");

        await args.CompleteMessageAsync(args.Message);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args) {
        Console.WriteLine(args.Exception.ToString());

        return Task.CompletedTask;
    }
}
