# MCP C# SDK

[![NuGet version](https://img.shields.io/nuget/v/Garden.ModelContextProtocol.svg)](https://www.nuget.org/packages/Garden.ModelContextProtocol)

The official C# SDK for the [Model Context Protocol](https://modelcontextprotocol.io/), enabling .NET applications, services, and libraries to implement and interact with MCP clients and servers. Please visit the [API documentation](https://csharp.sdk.modelcontextprotocol.io/api/Garden.ModelContextProtocol.html) for more details on available functionality.

## Packages

This SDK consists of three main packages:

- **[Garden.ModelContextProtocol.Core](https://www.nuget.org/packages/Garden.ModelContextProtocol.Core)** [![NuGet version](https://img.shields.io/nuget/v/Garden.ModelContextProtocol.Core.svg)](https://www.nuget.org/packages/Garden.ModelContextProtocol.Core) - For projects that only need to use the client or low-level server APIs and want the minimum number of dependencies.

- **[Garden.ModelContextProtocol](https://www.nuget.org/packages/Garden.ModelContextProtocol)** [![NuGet version](https://img.shields.io/nuget/v/Garden.ModelContextProtocol.svg)](https://www.nuget.org/packages/Garden.ModelContextProtocol) - The main package with hosting and dependency injection extensions. References `Garden.ModelContextProtocol.Core`. This is the right fit for most projects that don't need HTTP server capabilities.

- **[Garden.ModelContextProtocol.AspNetCore](https://www.nuget.org/packages/Garden.ModelContextProtocol.AspNetCore)** [![NuGet version](https://img.shields.io/nuget/v/Garden.ModelContextProtocol.AspNetCore.svg)](https://www.nuget.org/packages/Garden.ModelContextProtocol.AspNetCore) - The library for HTTP-based MCP servers. References `Garden.ModelContextProtocol`.

## Getting Started

To get started, see the [Getting Started](https://csharp.sdk.modelcontextprotocol.io/concepts/getting-started.html) guide in the conceptual documentation for installation instructions, package-selection guidance, and complete examples for both clients and servers.

You can also browse the [samples](samples) directory and the [API documentation](https://csharp.sdk.modelcontextprotocol.io/api/Garden.ModelContextProtocol.html) for more details on available functionality.

## About MCP

The Model Context Protocol (MCP) is an open protocol that standardizes how applications provide context to Large Language Models (LLMs). It enables secure integration between LLMs and various data sources and tools.

For more information about MCP:

- [Official MCP Documentation](https://modelcontextprotocol.io/)
- [MCP C# SDK Documentation](https://csharp.sdk.modelcontextprotocol.io/)
- [Protocol Specification](https://modelcontextprotocol.io/specification/)
- [GitHub Organization](https://github.com/modelcontextprotocol)

## Cross-Application Access (Identity Assertion Authorization Grant flow)

The SDK provides support for the [Identity Assertion Authorization Grant flow](https://github.com/modelcontextprotocol/ext-auth/blob/main/specification/draft/enterprise-managed-authorization.mdx)
via `IdentityAssertionGrantProvider`. See the [Cross-Application Access](docs/concepts/transports/transports.md#cross-application-access) section in the transport docs for full usage details.

## License

This project is licensed under the [Apache License 2.0](LICENSE).
