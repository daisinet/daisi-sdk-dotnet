# Release Automation Setup — daisi-sdk-dotnet

The SDK publishes to NuGet automatically when triggered. In the one-click release system, the orchestrator checks if SDK source has changed since the last tag — if so, it dispatches this repo's `release-sdk.yml` workflow.

## Prerequisites

- A nuget.org account with push permissions for the `Daisi.SDK` package
- A nuget.org API key

---

## Step 1: Create a NuGet API Key

1. Go to [nuget.org > API keys](https://www.nuget.org/account/apikeys)
2. Click **Create**
3. Set:
   - **Key name**: `daisi-sdk-publish`
   - **Expiration**: 365 days (max)
   - **Glob pattern**: `Daisi.SDK`
   - **Scopes**: Push
4. Click **Create** and copy the key

---

## Step 2: Configure GitHub Repository Secrets

Go to **daisi-sdk-dotnet** repo: **Settings > Secrets and variables > Actions > New repository secret**

| Secret Name | Value | Purpose |
|---|---|---|
| `NUGET_API_KEY` | The NuGet API key from Step 1 | Push packages to nuget.org |

That's the only secret needed. The orchestrator dispatches this workflow using its own `RELEASE_PAT` (configured in the [daisi-orc-dotnet setup](../daisi-orc-dotnet/ReleaseSetup.md)).

---

## Step 3: Verify the Setup

### Manual test

1. Go to the **daisi-sdk-dotnet** repo > **Actions** tab
2. Select the **Release SDK NuGet** workflow
3. Click **Run workflow**, enter the current version (e.g. `1.0.9`)
4. Verify the package appears on [nuget.org/packages/Daisi.SDK](https://www.nuget.org/packages/Daisi.SDK)

> **Note**: NuGet packages can take a few minutes to index after upload.

---

## How the Orchestrator Triggers SDK Releases

The `orchestrate-release.yml` workflow (in `daisi-orc-dotnet`) does the following:

1. Checks out `daisi-sdk-dotnet` with `fetch-depth: 0` (full history)
2. Finds the latest `v*` tag
3. Runs `git diff` against the tag, looking at the `Daisi.SDK/` directory
4. If files changed: dispatches `release-sdk.yml` with the version from the `.csproj`
5. If unchanged: skips the SDK publish step entirely

This means the SDK is only published when its source actually changes — not on every DAISI release.

---

## Versioning

The SDK uses independent semver (e.g. `1.0.9`, `1.0.10`), separate from the Host/ORC timestamp versions (e.g. `2026.02.11.1430`).

To bump the SDK version:
1. Edit `Daisi.SDK/Daisi.SDK.csproj` — update the `<Version>`, `<AssemblyVersion>`, and `<FileVersion>` fields
2. Commit and push
3. On the next one-click release, the orchestrator will detect the source change and publish the new version

Alternatively, push a `v*` tag directly (e.g. `v1.0.10`) to trigger an immediate publish.

---

## Workflow Reference

| Workflow | File | Trigger | What it does |
|---|---|---|---|
| **Release SDK NuGet** | `release-sdk.yml` | Tag push (`v*`) or `workflow_dispatch` | Packs and publishes Daisi.SDK to nuget.org |
