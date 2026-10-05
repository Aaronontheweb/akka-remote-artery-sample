using System;
using Akka.Actor;
using Akka.Serialization.V2;
using AkkaRemoteArterySample.Protocol;
using Xunit;

namespace AkkaRemoteArterySample.Tests;

public sealed class GreetingProtocolSerializerTests
{
    [Fact]
    public void GreetingRequest_should_round_trip_through_generated_serializer()
    {
        using var system = ActorSystem.Create("greeting-serializer-test");
        var serializer = new GreetingProtocolSerializer((ExtendedActorSystem)system);

        var message = new GreetingRequest(
            "Akka.NET",
            42,
            Guid.Parse("8f7d35c8-2931-4a48-9b84-2c008ab7f2e4"));

        var bytes = serializer.ToBinary(message);
        var manifest = serializer.Manifest(message);
        var deserialized = serializer.FromBinary(bytes, manifest);

        Assert.IsType<GreetingRequest>(deserialized);
        Assert.Equal(message, deserialized);
        Assert.Equal("greeting-request-v1", manifest);
    }

    [Fact]
    public void GreetingReply_should_round_trip_through_generated_serializer()
    {
        using var system = ActorSystem.Create("greeting-serializer-test");
        var serializer = new GreetingProtocolSerializer((ExtendedActorSystem)system);

        var message = new GreetingReply(
            "Hello, Akka.NET!",
            Guid.Parse("8f7d35c8-2931-4a48-9b84-2c008ab7f2e4"),
            DateTimeOffset.Parse("2026-10-05T18:00:00Z"));

        var bytes = serializer.ToBinary(message);
        var manifest = serializer.Manifest(message);
        var deserialized = serializer.FromBinary(bytes, manifest);

        Assert.IsType<GreetingReply>(deserialized);
        Assert.Equal(message, deserialized);
        Assert.Equal("greeting-reply-v1", manifest);
    }
}
