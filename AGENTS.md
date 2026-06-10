# AGENTS.md

This document captures important context for anyone (human or agent) working on the Garden fork of the Model Context Protocol C# SDK.

## Build & Release

- Cloud Build is triggered by pushing tags matching `^11\..+`
- The solution file is `ModelContextProtocol.slnx`
- All three main packages are published to GitHub Packages under the `HitchSoftware` organization:
  - `Garden.ModelContextProtocol`
  - `Garden.ModelContextProtocol.Core`
  - `Garden.ModelContextProtocol.AspNetCore`

## Critical Cloud Build Setting

**Do not remove or modify the following without re-testing the full publish flow:**

In `cloudbuild.yaml`, the `push-packages` step includes:

```yaml
env:
  - 'DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0'
```

### Why this setting exists

During the initial release of the Garden fork (tags 11.2.0 – 11.2.10), the `Garden.ModelContextProtocol.Core` package consistently failed to upload to GitHub Packages during Cloud Build, while smaller packages succeeded.

Extensive debugging showed:
- The package itself was fine (~2.2 MB) and pushed successfully from developer machines.
- Changing push order, adding `--no-symbols`, `--timeout`, and even attempting MTU clamping did not resolve the issue.
- The failure was a transport-level stall ("Operation canceled", "Pushing took too long") that occurred after several minutes.

The root cause was traced to the modern `SocketsHttpHandler` in .NET 10 inside the Cloud Build environment when talking to GitHub Packages. Setting `DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0` forces the legacy `HttpClientHandler`, which resolved the stall.

This setting was added after 11 failed build attempts and should be considered **required** for reliable publishes until the underlying incompatibility is understood or resolved upstream.

## Current Build Configuration (11.2.26+)

- Only targets `net10.0` (netstandard2.0, net8.0, and net9.0 removed)
- `src/Directory.Build.props` has been deleted
- Root `nuget.config` has been deleted
- `cloudbuild.yaml` only restores/builds/packs the three main projects under `src/`
- Push logic is the simple vanilla loop (no Core-first ordering, no custom env vars)

## When Pulling Changes from Upstream

When merging changes from the original `modelcontextprotocol/csharp-sdk`:

1. **Do not** restore `src/Directory.Build.props` or the root `nuget.config` without review.
2. **Do not** re-add `netstandard2.0` / `net8.0` / `net9.0` target frameworks without testing the full publish flow.
3. **Do not** re-introduce the old `push-packages` logic (Core-first, custom env vars, etc.).
4. After any upstream merge, run a test tag (e.g. `11.2.99`) and verify that all three Garden packages build and push successfully.

## When Modifying cloudbuild.yaml

If you need to change the `push-packages` step, please:
1. Re-run a full publish cycle with a test tag (e.g. `11.2.99`).
2. Confirm that **all three** packages publish successfully.
3. Update this document with any new requirements.
