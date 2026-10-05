using Akka.Actor;
using Akka.Hosting;
using AkkaRemoteArterySample.Protocol;
using AkkaRemoteArterySample.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAkka("artery-sample-server", (akkaBuilder, _) =>
{
    // Register the source-generated serializer. Because we configure Artery
    // via HOCON (no Akka.Hosting helper exists for it yet), we add the HOCON
    // file here on top of the serializer setup.
    akkaBuilder.AddSetup(GreetingProtocolSerializer.CreateRegistration().CreateSetup());
    akkaBuilder.AddHoconFile("application.conf", HoconAddMode.Prepend);

    akkaBuilder.WithActors((system, registry) =>
    {
        var greeter = system.ActorOf(Props.Create(() => new GreeterActor()), "greeter");
        registry.Register<GreeterActor>(greeter);
        Console.WriteLine($"[server] greeter started at {greeter.Path}");
    });
});

var host = builder.Build();
await host.StartAsync();

var system = host.Services.GetRequiredService<ActorSystem>();
await system.WhenTerminated;
await host.StopAsync();
