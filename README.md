# Daisi's .Net SDK
This is the .Net 10 SDK for interacting with the DAISI network.

Documentation available on [our website](https://daisi.ai/Learn/SDK).

Quickstart Guide is [here](https://daisi.ai/Learn/SDK/dotnet/QuickStart).

Nuget install with Package Manager Console:

```
Install-Package Daisi.SDK
```

### Project - Daisi.SDK
Core functionality across most of the Daisi .Net codebase. Almost every project references this either directly or indirectly. Use this for gaining access to all of the clients that communicate with Orcs and Hosts.

### Project - Daisi.SDK.Razor
A set of Razor components that are useful when building out new apps in the Daisi network. DaisiChat is available here and can allow you to easily add a chat window to any .Net Razor project.

### Project - Daisi.SDK.Web
Middleware and other website related projects that are not components. This is useful for adding DAISI authentication and authorization to your .Net web applications.

### Project - Daisi.SDK.Tests
Unit testing project for the SDK. Coverage is very light. We could use some help here as it's not a strength existing on the team at this time.

### Proto - Releases
Proto definitions for the host release management system. Defines `ReleasesProto` gRPC service with `Create`, `GetReleases`, `GetActiveRelease`, and `Activate` RPCs. The `ReleaseModels.proto` file contains the `HostReleaseInfo` message and all request/response pairs. The `Host` message includes a `ReleaseGroup` field (field 16) for assigning hosts to rollout groups (e.g. beta, production).

# NuGet Releases
The SDK NuGet package is published to nuget.org automatically via the GitHub Actions workflow (`.github/workflows/release-sdk.yml`).

### Automated Release (Tag Push)
Push a tag matching `v*` (e.g. `v1.0.10`) to trigger an automatic build, pack, and publish to nuget.org. A GitHub Release is also created with the `.nupkg` attached.

### Manual Release (Workflow Dispatch)
Use the **workflow_dispatch** trigger in GitHub Actions and provide a `version` input (e.g. `1.0.10`) for ad-hoc releases without creating a tag.

### Versioning
The SDK follows semver (e.g. `1.0.9`, `1.0.10`). The version in `Daisi.SDK.csproj` is overridden at pack time via `/p:Version` and `/p:PackageVersion`, so you do not need to edit the `.csproj` to release.

### Required Secrets
The workflow requires the `NUGET_API_KEY` GitHub secret set to a nuget.org API key with push permissions for the `Daisi.SDK` package.

### One-Click Release Automation
The SDK is automatically checked and published as part of the one-click release pipeline. The orchestrator detects if SDK source changed since the last `v*` tag — if so, it dispatches the `release-sdk.yml` workflow before proceeding with ORC and Host deployment.

See **[ReleaseSetup.md](ReleaseSetup.md)** for the full setup guide.

### Coordinated Release Order (manual fallback)
When proto or SDK changes affect ORC and Host:
1. Tag `daisi-sdk-dotnet` with `v*` — NuGet publishes automatically
2. Deploy ORC (includes SDK via ProjectReference, picks up changes immediately)
3. Tag `daisi-hosts-dotnet` with `beta-*` — phased host rollout begins

# Examples
## Daisi.Console.Chat
The example Console app is meant to show the bare minimum needed to get started. Should give a simple Basic thinking chat in a console window. This was moved into this project from it's own repo to make it easier to keep it up to date with the SDK changes.
You will need a secret key as provided by the [Daisi Manager](https://manager.daisi.ai). 
View our SDK documentation [on our website](https://daisi.ai/Learn/SDK).
