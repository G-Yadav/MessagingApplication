// See https://aka.ms/new-console-template for more information
using Azure.Messaging.ServiceBus;
using MessagingApp.Domain;
using MessagingApp.Domain.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Configuration.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
builder.Configuration.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"));
using IHost host = builder.Build();

IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
string? connectionString = config.GetValue<string>("connectionString");
string? queueName = config.GetValue<string>("queueName");
string? topicName = config.GetValue<string>("topicName");
string? subscriptionName1 = config.GetValue<string>("BackBencher1");
string? subscriptionName2 = config.GetValue<string>("BackBencher2");

IServiceBusSendService sendService;
if( string.IsNullOrEmpty(connectionString) | string.IsNullOrEmpty(queueName) )
    throw new Exception("Configuration Not found for Connection string or queuename");

sendService = new ServiceBusSendService(connectionString,queueName);

try {
    await sendService
    .CreateConnection()
    .AddMessage("Hi this is first message")
    .AddMessage("Hi This is second message")
    .SendMessage();
}
catch (Exception ex) {
    Console.WriteLine(ex.ToString());
}
finally
{
    await sendService.Dispose();
}


IServiceBusReceiveService receiveService = new ServiceBusReceiveService(connectionString,queueName);

try
{
    await receiveService
        .CreateConnection()
        .SetHandlers(Handlers.Default)
        .ReceiveMessage();
}
catch (Exception ex) 
{
    Console.WriteLine(ex.ToString());
}
finally 
{
    await receiveService.Dispose();
}

Func<ProcessMessageEventArgs, Task> messageHandler = async (ProcessMessageEventArgs args) =>
{
    string body = args.Message.Body.ToString();
    long sender = args.Message.SequenceNumber;
    Console.WriteLine($"Received From subscription:{sender}) {body}");

    await args.CompleteMessageAsync(args.Message);
};

Func<ProcessErrorEventArgs,Task> errorHandler = (ProcessErrorEventArgs args) =>
{
    Console.WriteLine(args.Exception.ToString());

    return Task.CompletedTask;
};


if(string.IsNullOrEmpty(topicName))
    throw new Exception("Configuration Not found for Connection string or queuename");

sendService = new ServiceBusSendService(connectionString,topicName);

try {
    await sendService
    .CreateConnection()
    .AddMessage("Hi this is first message in Topic")
    .AddMessage("Hi This is second message in Topic")
    .SendMessage();

    Console.WriteLine("Added Message to Topic");
}
catch (Exception ex) {
    Console.WriteLine(ex.ToString());
}
finally
{
    await sendService.Dispose();
}

if(string.IsNullOrEmpty(subscriptionName1))
    throw new Exception("Configuration Not found for Connection string or queuename");

receiveService = new ServiceBusReceiveService(connectionString, topicName, subscriptionName1);

try
{
    await receiveService
        .CreateConnection()
        .SetHandlers(Handlers.Custom, messageHandler, errorHandler)
        .ReceiveMessage();
}
catch (Exception ex) 
{
    Console.WriteLine(ex.ToString());
}
finally 
{
    await receiveService.Dispose();
}

if(string.IsNullOrEmpty(subscriptionName2))
    throw new Exception("Configuration Not found for Connection string or queuename");

receiveService = new ServiceBusReceiveService(connectionString, topicName, subscriptionName2);

try
{
    await receiveService
        .CreateConnection()
        .SetHandlers(Handlers.Custom, messageHandler, errorHandler)
        .ReceiveMessage();
}
catch (Exception ex) 
{
    Console.WriteLine(ex.ToString());
}
finally 
{
    await receiveService.Dispose();
}



Console.WriteLine("Hello, World!");


await host.RunAsync();

