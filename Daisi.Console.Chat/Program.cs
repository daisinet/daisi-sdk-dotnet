
using Daisi.Console.Chat;
using Daisi.Protos.V1;
using Daisi.SDK.Clients.V1.Host;
using Daisi.SDK.Clients.V1.Orc;
using Daisi.SDK.Extensions;
using Daisi.SDK.Interfaces;
using Daisi.SDK.Models;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("=================================");
Console.WriteLine("Example: Hello, Daisi!");
Console.WriteLine("Written by David Graham on 12/4/2025");
Console.WriteLine("Copyright © 2025. Distributed AI Systems, Inc.");
Console.WriteLine("=================================");

var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(new HostApplicationBuilderSettings()
{
    Args = args,
    EnvironmentName = Environments.Development
});

/*
 * Before you run the app, you need to add your secret key to your local user-secrets,
 * To do so, execute the follow in the DotNet CLI, using your real secret key:
 * 
 * dotnet user-secrets set "Daisi:SecretKey" "<<Your Secret Key>>"
 * 
 * More Info: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0&tabs=windows
 * 
 */

// Add the default Daisi setup to the system, including clients, factories, 
// default Orc and DefaultClientKeyProvider
string secretKey = builder.Configuration["Daisi:SecretKey"];
builder.Services.AddDaisi(secretKey);

// Register the service that will actually run our chats with the hosts.
builder.Services.AddHostedService<DaisiChatBot>();

var host = builder.Build();

// Sets up the app's Client Key for communicating with the Orc.
host.Services.UseDaisi();

await host.RunAsync();

