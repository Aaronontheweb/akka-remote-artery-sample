using Akka.Actor;
using Akka.Serialization.V2;

namespace AkkaRemoteArterySample.Protocol;

/// <summary>
/// Marker interface grouping the message types owned by one generated serializer.
/// The source generator dispatches over this closed set at compile time.
/// </summary>
public interface IGreetingProtocol
{
}

/// <summary>
/// A message sent over the wire between client and server.
/// Every field is indexed with <c>[AkkaField(n)]</c> so the generated
/// MessagePack serializer can write/read it with no reflection.
/// </summary>
[AkkaSerializable(Manifest = "greeting-request-v1")]
public sealed record GreetingRequest(
    [property: AkkaField(0)] string Name,
    [property: AkkaField(1)] int Count,
    [property: AkkaField(2)] Guid RequestId) : IGreetingProtocol;

/// <summary>
/// The server's reply, also part of the same protocol so both sides of the
/// wire use the same generated serializer.
/// </summary>
[AkkaSerializable(Manifest = "greeting-reply-v1")]
public sealed record GreetingReply(
    [property: AkkaField(0)] string Message,
    [property: AkkaField(1)] Guid RequestId,
    [property: AkkaField(2)] DateTimeOffset SentAt) : IGreetingProtocol;

/// <summary>
/// The generated serializer. Mark it with <c>[AkkaSerializer{T}]</c> giving a
/// stable name and an id outside Akka's reserved 1-100 range. The generator
/// fills in <c>CreateRegistration()</c>.
/// </summary>
[AkkaSerializer<IGreetingProtocol>("greeting-protocol", 120001)]
public sealed partial class GreetingProtocolSerializer : AkkaSerializer
{
    public static partial SerializerRegistration CreateRegistration();
}
