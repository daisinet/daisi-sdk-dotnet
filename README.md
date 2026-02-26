# Daisi's .Net SDK
This is the .Net 10 SDK for interacting with the DAISI network.

Documentation available on [our website](https://daisi.ai/Learn/SDK).

Quickstart Guide is [here](https://daisi.ai/Learn/SDK/dotnet/QuickStart).

Nuget install with Package Manager Console:

```
Install-Package Daisi.SDK
```


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

### Command Proto Messages

The SDK defines command messages for ORC-to-host communication in `Protos/V1/Models/CommandModels.proto`. Notable model management commands:

- `DownloadModelRequest` / `DownloadModelResponse` — Sent by the ORC during heartbeat when a host is missing a required model. The host downloads the file and loads it without restart.
- `RemoveModelRequest` / `RemoveModelResponse` — Broadcast by the ORC when a model is deleted. Hosts unload, remove from settings, and delete the file.
- `IndexFileRequest` — Sent by the ORC to a host to index a Drive file into the vector database for RAG search. Includes accountId, fileId, repositoryId, and createdByUserId.
- `McpSyncRequest` / `McpSyncResponse` — Sent by the ORC to a host to sync resources from an MCP server. The host connects to the MCP server, reads resources, and uploads them to Drive.

### MCP Client

The SDK includes an MCP (Model Context Protocol) client for managing external data source integrations via `McpClientFactory` and `McpClient`.

**Factory pattern:**
```csharp
var mcpClientFactory = new McpClientFactory(clientKeyProvider);
var mcpClient = mcpClientFactory.Create();
```

**Available methods:**
- `RegisterServerAsync(name, serverUrl, authType, authSecret, targetRepoId, syncInterval)` — Register a new MCP server
- `UpdateServerAsync(request)` — Update server configuration
- `RemoveServerAsync(serverId)` — Remove a registered server
- `ListServersAsync()` — List all MCP servers for the account
- `GetServerAsync(serverId)` — Get a specific server's details
- `DiscoverResourcesAsync(serverId)` — Discover available resources on a server
- `ToggleResourceSyncAsync(serverId, resourceUri, enabled)` — Enable/disable sync for a resource
- `TriggerSyncAsync(serverId)` — Trigger an immediate sync
- `GetSyncStatusAsync(serverId)` — Get the current sync status

**MCP proto definitions** are in `Protos/V1/Mcp.proto` with model types in `Protos/V1/Models/CommandModels.proto`.

### Embedding Interface

The SDK defines `IEmbeddingBackend` in `Daisi.Inference/Interfaces/` for text embedding generation. Host implementations use this interface to generate embeddings for RAG vector search.

```csharp
public interface IEmbeddingBackend : IAsyncDisposable
{
    string BackendName { get; }
    int Dimensions { get; }
    Task<float[]> EmbedAsync(string text, CancellationToken ct = default);
    Task<float[][]> EmbedBatchAsync(string[] texts, CancellationToken ct = default);
}
```

### Drive Client (Vector Search)

`DriveClient` now supports access-controlled vector search with repository and file filtering:

```csharp
var response = await driveClient.VectorSearchAsync(
    query: "search text",
    topK: 10,
    repositoryIds: new[] { "repo-1", "repo-2" },  // optional - filter to specific repos
    fileIds: null,                                   // optional - filter to specific files
    includeSystemFiles: false
);
```

Results are scoped to repositories the caller has access to. Each `VectorSearchResult` includes a `RepositoryId` field for client-side grouping.

# Examples
## Daisi.Console.Chat
The example Console app is meant to show the bare minimum needed to get started. Should give a simple Basic thinking chat in a console window. This was moved into this project from it's own repo to make it easier to keep it up to date with the SDK changes.
You will need a secret key as provided by the [Daisi Manager](https://manager.daisi.ai). 
View our SDK documentation [on our website](https://daisi.ai/Learn/SDK).
