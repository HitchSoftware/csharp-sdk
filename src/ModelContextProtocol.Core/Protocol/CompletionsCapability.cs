using System.ComponentModel;
using System.Text.Json.Serialization;
using Garden.ModelContextProtocol.Server;

namespace Garden.ModelContextProtocol.Protocol;

/// <summary>
/// Represents the completions capability for providing auto-completion suggestions
/// for prompt arguments and resource references.
/// </summary>
/// <remarks>
/// <para>
/// When enabled, this capability allows a Model Context Protocol server to provide
/// auto-completion suggestions. This capability is advertised to clients during the initialize handshake.
/// </para>
/// <para>
/// The primary function of this capability is to improve the user experience by offering
/// contextual suggestions for argument values or resource identifiers based on partial input.
/// </para>
/// <para>
/// See the <see href="https://github.com/Garden.ModelContextProtocol/specification/blob/main/schema/">schema</see> for details.
/// </para>
/// <para>
/// This class is intentionally empty as the Model Context Protocol specification does not
/// currently define additional properties for completions capabilities. Future versions of the
/// specification might extend this capability with additional configuration options.
/// </para>
/// </remarks>
public sealed class CompletionsCapability
{
}
