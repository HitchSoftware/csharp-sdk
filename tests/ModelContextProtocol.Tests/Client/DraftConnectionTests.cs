using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;

namespace ModelContextProtocol.Tests.Client;

/// <summary>
/// Tests for the draft protocol revision (SEP-2575 + SEP-2567) connection flow on
/// <see cref="McpClient"/> — the client should call <c>server/discover</c> instead of
/// <c>initialize</c> when <see cref="McpClientOptions.ExperimentalProtocolVersion"/> is set and
/// the server supports the requested version, and it should fall back to the legacy
/// <c>initialize</c> handshake otherwise.
/// </summary>
#pragma warning disable MCPEXP002 // ExperimentalProtocolVersion
public class DraftConnectionTests : ClientServerTestBase
{
    private const string DraftVersion = "2026-06-XX";
    private const string LatestStableVersion = "2025-11-25";

    private bool _serverHasExperimental;

    public DraftConnectionTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper, startServer: false)
    {
    }

    protected override void ConfigureServices(ServiceCollection services, IMcpServerBuilder mcpServerBuilder)
    {
        services.Configure<McpServerOptions>(options =>
        {
            options.ServerInfo = new Implementation { Name = nameof(DraftConnectionTests), Version = "1.0" };
            if (_serverHasExperimental)
            {
                options.ExperimentalProtocolVersion = DraftVersion;
            }
        });
    }

    [Fact]
    public async Task DraftClient_ConnectingToDraftServer_NegotiatesExperimentalVersion()
    {
        _serverHasExperimental = true;
        StartServer();

        var options = new McpClientOptions { ExperimentalProtocolVersion = DraftVersion };
        await using var client = await CreateMcpClientForServer(options);

        Assert.Equal(DraftVersion, client.NegotiatedProtocolVersion);
        Assert.NotNull(client.ServerCapabilities);
        Assert.Equal(nameof(DraftConnectionTests), client.ServerInfo.Name);
    }

    [Fact]
    public async Task DraftClient_ConnectingToLegacyServer_FallsBackToLegacyInitialize()
    {
        _serverHasExperimental = false;
        StartServer();

        var options = new McpClientOptions { ExperimentalProtocolVersion = DraftVersion };
        await using var client = await CreateMcpClientForServer(options);

        Assert.NotEqual(DraftVersion, client.NegotiatedProtocolVersion);
        Assert.Equal(LatestStableVersion, client.NegotiatedProtocolVersion);
    }

    [Fact]
    public async Task LegacyClient_ConnectingToDraftServer_NegotiatesLegacyVersion()
    {
        _serverHasExperimental = true;
        StartServer();

        await using var client = await CreateMcpClientForServer();

        Assert.NotEqual(DraftVersion, client.NegotiatedProtocolVersion);
    }

    [Fact]
    public async Task LegacyClient_CanCallServerDiscover_EvenWithoutDraftConfigured()
    {
        // server/discover is registered unconditionally, so a legacy client can probe it
        // (e.g., to learn capabilities without doing a second initialize).
        _serverHasExperimental = false;
        StartServer();

        await using var client = await CreateMcpClientForServer();

        var response = await client.SendRequestAsync(
            new JsonRpcRequest { Method = RequestMethods.ServerDiscover },
            TestContext.Current.CancellationToken);

        var discoverResult = JsonSerializer.Deserialize<DiscoverResult>(response.Result, McpJsonUtilities.DefaultOptions);
        Assert.NotNull(discoverResult);
        Assert.NotEmpty(discoverResult.SupportedVersions);
        Assert.Contains(LatestStableVersion, discoverResult.SupportedVersions);
        Assert.Equal(nameof(DraftConnectionTests), discoverResult.ServerInfo.Name);
    }

    [Fact]
    public async Task DraftServer_DiscoverIncludesExperimentalVersion()
    {
        _serverHasExperimental = true;
        StartServer();

        await using var client = await CreateMcpClientForServer();

        var response = await client.SendRequestAsync(
            new JsonRpcRequest { Method = RequestMethods.ServerDiscover },
            TestContext.Current.CancellationToken);

        var discoverResult = JsonSerializer.Deserialize<DiscoverResult>(response.Result, McpJsonUtilities.DefaultOptions);
        Assert.NotNull(discoverResult);
        Assert.Contains(DraftVersion, discoverResult.SupportedVersions);
    }
}
