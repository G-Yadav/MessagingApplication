// See https://aka.ms/new-console-template for more information
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
    sendService.Dispose();
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



Console.WriteLine("Hello, World!");


await host.RunAsync();

