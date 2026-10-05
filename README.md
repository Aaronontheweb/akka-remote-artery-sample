# Akka Remote Artery Sample

A small Akka.NET v1.6 beta sample that pulls together two of the newest pieces of the stack:

- **Source-generated serialization** (`Akka.Serialization.V2`) — compile-time, reflection-free
  MessagePack serializers for your own message types.
- **Artery** remoting over `tcp` — the experimental Akka.Streams-based transport that runs beside
  classic DotNetty remoting.

Both are configured through **Akka.Hosting**.

## What's here

Three projects:

| Project | Role |
|---------|------|
| `AkkaRemoteArterySample.Protocol` | The shared protocol: messages (`GreetingRequest` / `GreetingReply`) and the generated serializer. |
| `AkkaRemoteArterySample.Server` | A host that spins up a remote `GreeterActor` and answers requests over Artery. |
| `AkkaRemoteArterySample.Client` | A host that sends a `GreetingRequest` to the server over Artery and prints the reply. |

## Requirements

- .NET 10 SDK (`global.json` pins it here)
- Akka.NET `1.6.0-beta1` (resolved from NuGet)

## How it works

### Source-generated serialization

`Akka.Serialization.V2` ships a Roslyn source generator inside the package. You declare a schema
with attributes and the generator writes the MessagePack
`Serialize`/`Deserialize`/`Manifest`/`SizeHint` code for you. See
`src/AkkaRemoteArterySample.Protocol/GreetingProtocol.cs`:

```csharp
[AkkaSerializable(Manifest = "greeting-request-v1")]
public sealed record GreetingRequest(
    [property: AkkaField(0)] string Name,
    [property: AkkaField(1)] int Count,
    [property: AkkaField(2)] Guid RequestId) : IGreetingProtocol;

[AkkaSerializer<IGreetingProtocol>("greeting-protocol", 120001)]
public sealed partial class GreetingProtocolSerializer : AkkaSerializer
{
    public static partial SerializerRegistration CreateRegistration();
}
```

The generator fills in `CreateRegistration()`. Register it through Akka.Hosting:

```csharp
akkaBuilder.AddSetup(GreetingProtocolSerializer.CreateRegistration().CreateSetup());
```

### Artery over TCP

Artery is experimental and off by default; you turn it on through HOCON. There is no
Akka.Hosting helper for it yet, so this sample adds a `application.conf` file via `AddHoconFile`:

```hocon
akka {
  actor {
    provider = remote
  }
  remote {
    artery {
      enabled = on
      canonical {
        hostname = "localhost"
        port = 25520
      }
    }
  }
}
```

`tcp` is the only transport implemented so far (and the default once Artery is enabled). Note the
address scheme change: classic remoting uses `akka.tcp://`, while Artery uses `akka://` and a
different default port (`25520`).

The client talks to the server's greeter at:

```
akka://artery-sample-server@localhost:25520/user/greeter
```

## Running it

Open two terminals. First start the server:

```bash
dotnet run --project src/AkkaRemoteArterySample.Server
```

Then, in a second terminal, run the client:

```bash
dotnet run --project src/AkkaRemoteArterySample.Client
```

You should see the server log the request and the client print a matching reply, with the
request/response ids matching.

## Notes

- Artery is **experimental**. Akka.NET logs its own "don't use in production" warning at startup.
- One `ActorSystem` uses one transport. Classic (`akka.tcp://`) and Artery (`akka://`) are not
  wire-compatible, so every node in a connection must run the same one.
- Serializer ids `1`–`100` are reserved for Akka's built-in serializers; the sample uses `120001`.
