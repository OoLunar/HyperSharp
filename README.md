# HyperSharp Overview
HyperSharp is a C# implementation of the [HTTP 1.1 protocol](https://www.rfc-editor.org/rfc/rfc9110). It's designed with emphasis on speed, lightweight nature, and most importantly: user-friendliness.

To get started, you can install the [NuGet package](https://www.nuget.org/packages/HyperSharp/) and follow the [setup](#setup) instructions.

To request support, you may open up a new GitHub issue, discussion or join the [Discord](https://discord.gg/HvMgJkzs6J).

To report bugs or request new features, please use GitHub issues.

Lastly, all API documentation and tutorials are available on the [website](https://oolunar.github.io/HyperSharp/), which is generated from the latest commit at the [docs](https://github.com/OoLunar/HyperSharp/tree/master/docs) folder.

# Table of Contents
 - [Core Concept: Responders](#core-concept-responders)
 - [Response Handling](#response-handling)
 - [Setup](#setup)
 - [Example: Hello World](#example-hello-world)

# Core Concept: Responders

 - The foundation of HyperSharp relies on the concept of responders.
 - A responder consists of a list of delegates.
 - Each delegate is a function that takes a request and produces a response.
 - Both request and response are generic types, allowing customization.
 - All responders and responder dependencies are executed sequentially.
 - If any delegate returns an error or value, all subsequent delegates are skipped and the response is returned.

# Response Handling

 - Responders use results and errors for handling responses.
 - Synchronous (`void`) and asynchronous (`Task`/`ValueTask`) execution modes are supported, with no additional setup required from the user.

# Setup

There are two ways to setup HyperSharp, depending on your needs. The recommended way is to use dependency injection, through `IServiceCollection`:

```csharp
serviceCollection.AddHyperSharp((serviceProvider, hyperConfiguration) =>
{
    IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
    string? host = configuration.GetValue("server:address", "localhost")?.Trim();
    if (!IPAddress.TryParse(host, out IPAddress? address))
    {
        IPAddress[] addresses = Dns.GetHostAddresses(host);
        address = addresses.Length != 0 ? addresses[0] : throw new InvalidOperationException("The server address could not be resolved to an IP address.");
    }

    hyperConfiguration.ListeningEndpoint = new IPEndPoint(address, configuration.GetValue("server:port", 8080));
    hyperConfiguration.AddResponders(new[] { typeof(HelloWorldResponder) });
});

IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
HyperServer hyperServer = serviceProvider.GetRequiredService<HyperServer>();
hyperServer.Start();
```

We recommend using the service collection method because the responders use dependency injection to resolve their dependencies. This allows for a more modular and testable design.

However, we understand that not everyone wants to use dependency injection. If you don't want to use dependency injection, you can use the `HyperServer` class directly:

```csharp
HyperServer hyperServer = new(new HyperConfiguration(new HyperConfigurationBuilder()
{
    ListeningEndpoint = new IPEndPoint(IPAddress.Loopback, 8080),
    Responders = new[] { typeof(HelloWorldResponder) }
}));

hyperServer.Start();
```

# Example: Hello World

```csharp
using System;
using System.Threading;
using HyperSharp.Protocol;
using HyperSharp.Responders;
using HyperSharp.Results;

public class HelloWorldResponder : IResponder<HyperContext, HyperStatus>
{
    // Specifies any required types for this responder (empty in this case)
    public static Type[] Needs => Type.EmptyTypes;

    // Generates a response indicating success with a message "Hello World!"
    public Result<HyperStatus> Respond(HyperContext context, CancellationToken cancellationToken = default) => Result.Success(HyperStatus.OK("Hello World!"));
}
```

This example demonstrates the structure of a responder using HyperSharp to create a "Hello World!" response.