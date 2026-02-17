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

#### Single Sign-On (SSO)
`Daisi.SDK.Web` includes built-in SSO support via `SsoTicketService`. This enables cross-app authentication where a single login at Manager gives seamless access to all participating apps.

**How it works**: The SSO authority (Manager) creates AES-256-GCM encrypted tickets containing the user's `clientKey` and session info. Relying-party apps (e.g. Drive) decrypt the ticket, validate the `clientKey` with the Orc, and set local cookies.

**Required config keys** (in `appsettings.json` or User Secrets under `Daisi:`):
- `SsoSigningKey` — Base64-encoded 32-byte AES key. Must be the same across all SSO-participating apps.
- `SsoAuthorityUrl` — Base URL of the SSO authority (e.g. `https://manager.daisinet.com`).
- `SsoAppUrl` — This app's own base URL (e.g. `https://drive.daisinet.com`).
- `SsoAllowedOrigins` — Comma-separated origins allowed to request tickets (set on the authority).

**To enable SSO for a new web app**: Add the four `Daisi:Sso*` config keys, call `AddDaisiForWeb()` in DI setup (which registers `SsoTicketService` automatically), and redirect unauthenticated users to `{SsoAuthorityUrl}/sso/authorize?returnUrl={yourCallbackUrl}&origin={yourAppUrl}`. The SDK middleware handles `/sso/callback` automatically.

### Project - Daisi.SDK.Tests
Unit testing project for the SDK. Coverage is very light. We could use some help here as it's not a strength existing on the team at this time.

### Proto - Tools
Proto definitions for the tool delegation system. The `Tools.proto` file defines the `ToolsProto` gRPC service with an `Execute` RPC for direct connect tool execution. The `CommandModels.proto` file contains the `ExecuteToolRequest`, `ToolParam`, and `ExecuteToolResponse` messages used for both ORC-mediated and direct connect tool delegation.

New proto messages:
- `ExecuteToolRequest` — Contains `ToolId`, `Parameters` (list of `ToolParam`), `RequestingHostId`, `SessionId`, and `RequestId`.
- `ToolParam` — Simple key-value pair (`Name`, `Value`) for tool parameters.
- `ExecuteToolResponse` — Contains `Success`, `Output`, `ErrorMessage`, `OutputMessage`, and `OutputFormat`.

New data model fields:
- `Host.ToolsOnly` (field 17) — Boolean flag on the `Host` proto message. Tools-only hosts are excluded from inference routing but available for tool delegation.
- `HostSettings.ToolsOnly` (field 10) — Boolean flag on the `HostSettings` proto message for local settings persistence.

### Client - ToolClient
`ToolClient` (`Clients/V1/Host/ToolClient.cs`) provides a direct connect client for calling `ToolsProto.Execute` on tools-only hosts. Connects to the target host at `http://{ip}:4242` and exposes `ExecuteToolAsync(ExecuteToolRequest)`.

### Proto - SecureTools
Proto definitions for the secure tool discovery system. Defines `SecureToolProto` gRPC service with `GetInstalledSecureTools` RPC. The ORC returns tool definitions including `InstallId` (opaque provider-facing identifier), `EndpointUrl` (provider's base URL), and `BundleInstallId` (shared OAuth identifier for plugin bundles) so consumer hosts and the Manager UI can call providers directly via HTTP. The `Execute` and `Configure` RPCs have been removed — the ORC is no longer in the execution hot path. See the [Secure Tools Provider Guide](https://daisi.ai/learn/marketplace/creating-secure-tools) for the full API contract.

**`SecureToolDefinitionInfo.BundleInstallId`** (field 9) — When a tool belongs to a plugin bundle, all tools share this ID for OAuth token keying. Providers use `BundleInstallId` (when present) instead of `InstallId` to key OAuth tokens, so users only OAuth-connect once per bundle.

**`MarketplacePurchaseInfo.BundleInstallId`** (field 13) — Shared bundle identifier stored on purchase records. Present on both the parent plugin purchase and all child tool purchases created during bundle purchase flow.

### SecureToolClientFactory / SecureToolClient
gRPC client factory for querying the ORC's `SecureToolProto` service for installed tool definitions. Follows the same pattern as `MarketplaceClientFactory`. Registered automatically via `AddDaisiOrcClients()`.

### SecureToolDefinition
SDK model (`Daisi.SDK.Models.Tools.SecureToolDefinition`) representing a secure tool's metadata for consumer-side use. Includes `MarketplaceItemId`, `ToolId`, `Name`, `UseInstructions`, `Parameters`, `ToolGroup`, `InstallId`, `EndpointUrl`, and `BundleInstallId`. Extension method `ToSdkModel()` converts from the proto `SecureToolDefinitionInfo`.

### Proto - Models (AIModel & BackendSettings)
The `SettingsModels.proto` file defines the `AIModel` and `BackendSettings` messages used across the system.

**AIModel fields:**
- `Type` (field 12) — Primary model type (backward compatibility with older hosts).
- `Types` (field 13, repeated) — Multi-type support. A single model can serve multiple modalities (e.g. TextGeneration + ImageGeneration for vision-language models). When `Types` is populated, `Type` is set to `Types[0]` for backward compat.

**BackendSettings fields:**
- `BackendEngine` (field 10, string) — Which inference backend handles this model. Values: `"LlamaSharp"` (GGUF models), `"OnnxRuntimeGenAI"` (ONNX models), or empty for auto-detection (defaults to LlamaSharp).
- `Temperature` (field 11, optional float) — Per-model default temperature override.
- `TopP` (field 12, optional float) — Per-model default top-p override.
- `TopK` (field 13, optional int32) — Per-model default top-k override.
- `RepeatPenalty` (field 14, optional float) — Per-model default repeat penalty override.
- `PresencePenalty` (field 15, optional float) — Per-model default presence penalty override.

These per-model inference defaults sit between the hardcoded defaults and per-request values in the override chain: per-request > per-model > hardcoded.

**ModelModels.proto:**
- `HuggingFaceONNXFile` message — Represents an ONNX model file discovered during HuggingFace lookup, with `FileName`, `SizeBytes`, and `DownloadUrl`.
- `HuggingFaceModelInfo.ONNXFiles` (field 10, repeated) — Lists ONNX files alongside existing GGUF files.

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
