using Akka.Actor;
using AkkaRemoteArterySample.Protocol;

namespace AkkaRemoteArterySample.Server;

/// <summary>
/// A remote-capable actor that answers <see cref="GreetingRequest"/> messages.
/// Because the ActorSystem is configured with Artery (provider = remote), the
/// client can reach this actor by its canonical Akka address.
/// </summary>
public sealed class GreeterActor : ReceiveActor
{
    public GreeterActor()
    {
        Receive<GreetingRequest>(req =>
        {
            Console.WriteLine($"[server] received {req.Count} greeting(s) for '{req.Name}' (req={req.RequestId})");

            var reply = new GreetingReply(
                Message: $"Hello, {req.Name}! You requested {req.Count} greeting(s).",
                RequestId: req.RequestId,
                SentAt: DateTimeOffset.UtcNow);

            Sender.Tell(reply);
        });
    }
}
