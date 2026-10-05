using Akka.Actor;
using Akka.Hosting;
using AkkaRemoteArterySample.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAkka("artery-sample-client", (akkaBuilder, _) =>
{
    // Same source-generated serializer registration as the server, so both
    // sides produce and consume identical wire messages.
    akkaBuilder.AddSetup(GreetingProtocolSerializer.CreateRegistration().CreateSetup());
    akkaBuilder.AddHoconFile("application.conf", HoconAddMode.Prepend);
});

var host = builder.Build();
await host.StartAsync();

var system = host.Services.GetRequiredService<ActorSystem>();
// Artery uses the "akka://" scheme, unlike classic remoting's "akka.tcp://".
var greeter = system.ActorSelection("akka://artery-sample-server@localhost:25520/user/greeter");

Console.WriteLine($"[client] sending requests to {greeter.Path}");

var requestId = Guid.NewGuid();
var reply = await greeter.Ask<GreetingReply>(
    new GreetingRequest("Akka.NET", 3, requestId),
    TimeSpan.FromSeconds(15));

Console.WriteLine($"[client] reply for {reply.RequestId}: {reply.Message} (at {reply.SentAt:O})");
Console.WriteLine($"[client] request/response ids match: {reply.RequestId == requestId}");

await host.StopAsync();
