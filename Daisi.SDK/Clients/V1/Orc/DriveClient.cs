using Daisi.Protos.V1;
using Daisi.SDK.Interfaces.Authentication;
using Daisi.SDK.Models;
using Daisi.SDK.Providers;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;

namespace Daisi.SDK.Clients.V1.Orc
{
    public class DriveClientFactory(IClientKeyProvider clientKeyProvider)
    {
        public DriveClientFactory() : this(DaisiStaticSettings.DefaultClientKeyProvider) { }

        /// <summary>
        /// Creates a DriveClient pointing at the File Manager Service URL.
        /// Falls back to ORC URL if no separate File Manager URL is configured.
        /// </summary>
        public DriveClient Create(string? fileManagerDomainOrIp = default, int? fileManagerPort = null)
        {
            return new DriveClient(
                fileManagerDomainOrIp ?? DaisiStaticSettings.OrcIpAddressOrDomain,
                fileManagerPort ?? DaisiStaticSettings.OrcPort,
                clientKeyProvider);
        }

        public DriveClient Create(string domainOrIp, int port, bool useSsl)
        {
            return new DriveClient(domainOrIp, port, useSsl, clientKeyProvider);
        }
    }

    public class DriveClient : DrivesProto.DrivesProtoClient
    {
        DriveClient(GrpcChannel channel, IClientKeyProvider clientKeyProvider)
            : base(channel.Intercept((metadata) =>
            {
                if (clientKeyProvider is not null)
                    metadata.Add(DaisiStaticSettings.ClientKeyHeader, clientKeyProvider.GetClientKey());
                else
                    metadata.Add(DaisiStaticSettings.ClientKeyHeader, DaisiStaticSettings.ClientKey);

                if (clientKeyProvider is IDriveIdentityProvider identity)
                {
                    metadata.Add("x-daisi-account-id", identity.GetAccountId());
                    metadata.Add("x-daisi-user-id", identity.GetUserId());
                    metadata.Add("x-daisi-user-name", identity.GetUserName());
                    metadata.Add("x-daisi-user-role", identity.GetUserRole().ToString());
                }

                return metadata;
            }))
        {
        }

        internal DriveClient(string domainOrIp, int port, IClientKeyProvider clientKeyProvider)
            : this(GrpcChannel.ForAddress($"{(DaisiStaticSettings.OrcUseSSL ? "https" : "http")}://{domainOrIp}:{port}"), clientKeyProvider)
        {
        }

        internal DriveClient(string domainOrIp, int port, bool useSsl, IClientKeyProvider clientKeyProvider)
            : this(GrpcChannel.ForAddress($"{(useSsl ? "https" : "http")}://{domainOrIp}:{port}"), clientKeyProvider)
        {
        }

        /// <summary>
        /// Uploads a file to Drive via streaming (backward compatible).
        /// </summary>
        public async Task<UploadResponse> UploadFileAsync(Stream fileStream, string fileName, string path = "/",
            string contentType = "application/octet-stream", bool isSystemFile = false, CancellationToken cancellationToken = default)
        {
            return await UploadFileAsync(fileStream, fileName, null, null, path, contentType, isSystemFile, cancellationToken);
        }

        /// <summary>
        /// Uploads a file to Drive via streaming with repository and folder targeting.
        /// </summary>
        public async Task<UploadResponse> UploadFileAsync(Stream fileStream, string fileName,
            string? repositoryId, string? folderId, string path = "/",
            string contentType = "application/octet-stream", bool isSystemFile = false, CancellationToken cancellationToken = default)
        {
            using var call = Upload(cancellationToken: cancellationToken);
            var buffer = new byte[64 * 1024]; // 64KB chunks
            int bytesRead;
            long offset = 0;
            long totalSize = fileStream.Length;
            bool firstChunk = true;

            while ((bytesRead = await fileStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                var chunk = new FileChunk
                {
                    Data = ByteString.CopyFrom(buffer, 0, bytesRead),
                    Offset = offset,
                    TotalSize = totalSize,
                    IsSystemFile = isSystemFile
                };

                if (firstChunk)
                {
                    chunk.FileName = fileName;
                    chunk.ContentType = contentType;
                    chunk.Path = path;
                    if (!string.IsNullOrEmpty(repositoryId))
                        chunk.RepositoryId = repositoryId;
                    if (!string.IsNullOrEmpty(folderId))
                        chunk.FolderId = folderId;
                    firstChunk = false;
                }

                await call.RequestStream.WriteAsync(chunk, cancellationToken);
                offset += bytesRead;
            }

            await call.RequestStream.CompleteAsync();
            return await call.ResponseAsync;
        }

        /// <summary>
        /// Downloads a file from Drive via streaming.
        /// </summary>
        public async Task<byte[]> DownloadFileAsync(string fileId, CancellationToken cancellationToken = default)
        {
            using var call = Download(new DownloadRequest { FileId = fileId }, cancellationToken: cancellationToken);
            using var memoryStream = new MemoryStream();

            await foreach (var chunk in call.ResponseStream.ReadAllAsync(cancellationToken))
            {
                memoryStream.Write(chunk.Data.Span);
            }

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Searches files by partial filename (for autocomplete).
        /// </summary>
        public async Task<FileSearchResponse> SearchFilesAsync(string query, int maxResults = 10, CancellationToken cancellationToken = default)
        {
            return await SearchFilesAsync(new FileSearchRequest { Query = query, MaxResults = maxResults }, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Searches files by partial filename with repository scope.
        /// </summary>
        public async Task<FileSearchResponse> SearchFilesAsync(string query, int maxResults, IEnumerable<string> repositoryIds, CancellationToken cancellationToken = default)
        {
            var request = new FileSearchRequest { Query = query, MaxResults = maxResults };
            request.RepositoryIds.AddRange(repositoryIds);
            return await SearchFilesAsync(request, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Performs semantic vector search over file contents.
        /// </summary>
        public async Task<VectorSearchResponse> VectorSearchAsync(string query, int topK = 5, bool includeSystemFiles = false, CancellationToken cancellationToken = default)
        {
            return await VectorSearchAsync(new VectorSearchRequest { Query = query, TopK = topK, IncludeSystemFiles = includeSystemFiles }, cancellationToken: cancellationToken);
        }

        // ========== Repository Methods ==========

        /// <summary>
        /// Lists all accessible repositories.
        /// </summary>
        public async Task<ListRepositoriesResponse> ListRepositoriesAsync(bool includeTrash = false, CancellationToken cancellationToken = default)
        {
            return await ListRepositoriesAsync(new ListRepositoriesRequest { IncludeTrash = includeTrash }, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Creates a custom repository.
        /// </summary>
        public async Task<CreateRepositoryResponse> CreateRepositoryAsync(string name, CancellationToken cancellationToken = default)
        {
            return await CreateRepositoryAsync(new CreateRepositoryRequest { Name = name }, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Deletes a custom repository. Moves its files to trash.
        /// </summary>
        public async Task<DeleteRepositoryResponse> DeleteRepositoryAsync(string repositoryId, CancellationToken cancellationToken = default)
        {
            return await DeleteRepositoryAsync(new DeleteRepositoryRequest { RepositoryId = repositoryId }, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Grants a user or app access to a repository.
        /// </summary>
        public async Task<GrantRepositoryAccessResponse> GrantRepositoryAccessAsync(string repositoryId, string userId, string userName, bool isApp = false, CancellationToken cancellationToken = default)
        {
            return await GrantRepositoryAccessAsync(new GrantRepositoryAccessRequest
            {
                RepositoryId = repositoryId,
                UserId = userId,
                UserName = userName,
                IsApp = isApp
            }, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Revokes a user or app's access to a repository.
        /// </summary>
        public async Task<RevokeRepositoryAccessResponse> RevokeRepositoryAccessAsync(string repositoryId, string userId, CancellationToken cancellationToken = default)
        {
            return await RevokeRepositoryAccessAsync(new RevokeRepositoryAccessRequest
            {
                RepositoryId = repositoryId,
                UserId = userId
            }, cancellationToken: cancellationToken);
        }

        // ========== Folder Methods ==========

        /// <summary>
        /// Creates a folder within a repository.
        /// </summary>
        public async Task<CreateFolderResponse> CreateFolderAsync(string repositoryId, string name, string? parentFolderId = null, CancellationToken cancellationToken = default)
        {
            var request = new CreateFolderRequest
            {
                RepositoryId = repositoryId,
                Name = name
            };
            if (!string.IsNullOrEmpty(parentFolderId))
                request.ParentFolderId = parentFolderId;
            return await CreateFolderAsync(request, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Deletes a folder and moves its contents to trash.
        /// </summary>
        public async Task<DeleteFolderResponse> DeleteFolderAsync(string folderId, CancellationToken cancellationToken = default)
        {
            return await DeleteFolderAsync(new DeleteFolderRequest { FolderId = folderId }, cancellationToken: cancellationToken);
        }

        // ========== File Operations ==========

        /// <summary>
        /// Moves a file to a different repository and/or folder.
        /// </summary>
        public async Task<MoveFileResponse> MoveFileAsync(string fileId, string targetRepositoryId, string? targetFolderId = null, CancellationToken cancellationToken = default)
        {
            var request = new MoveFileRequest
            {
                FileId = fileId,
                TargetRepositoryId = targetRepositoryId
            };
            if (!string.IsNullOrEmpty(targetFolderId))
                request.TargetFolderId = targetFolderId;
            return await MoveFileAsync(request, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Lists files in a repository/folder.
        /// </summary>
        public async Task<ListFilesResponse> ListFilesAsync(string? repositoryId = null, string? folderId = null, int pageSize = 50, bool includeSystemFiles = false, CancellationToken cancellationToken = default)
        {
            var request = new ListFilesRequest
            {
                PageSize = pageSize,
                IncludeSystemFiles = includeSystemFiles
            };
            if (!string.IsNullOrEmpty(repositoryId))
                request.RepositoryId = repositoryId;
            if (!string.IsNullOrEmpty(folderId))
                request.FolderId = folderId;
            return await ListAsync(request, cancellationToken: cancellationToken);
        }

        // ========== Trash Methods ==========

        /// <summary>
        /// Lists trashed files.
        /// </summary>
        public async Task<ListTrashResponse> ListTrashAsync(int pageSize = 50, CancellationToken cancellationToken = default)
        {
            return await ListTrashAsync(new ListTrashRequest { PageSize = pageSize }, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Restores a file from trash to its original location.
        /// </summary>
        public async Task<RestoreFromTrashResponse> RestoreFromTrashAsync(string fileId, CancellationToken cancellationToken = default)
        {
            return await RestoreFromTrashAsync(new RestoreFromTrashRequest { FileId = fileId }, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Permanently deletes all trashed files.
        /// </summary>
        public async Task<EmptyTrashResponse> EmptyTrashAsync(CancellationToken cancellationToken = default)
        {
            return await EmptyTrashAsync(new EmptyTrashRequest(), cancellationToken: cancellationToken);
        }

        // ========== Preview ==========

        /// <summary>
        /// Gets a file preview (HTML or raw bytes for rendering).
        /// </summary>
        public async Task<GetFilePreviewResponse> GetFilePreviewAsync(string fileId, CancellationToken cancellationToken = default)
        {
            return await GetFilePreviewAsync(new GetFilePreviewRequest { FileId = fileId }, cancellationToken: cancellationToken);
        }

        // ========== Storage & Settings ==========

        /// <summary>
        /// Gets the current storage limits for the account.
        /// </summary>
        public async Task<StorageLimits> GetStorageLimitsAsync(CancellationToken cancellationToken = default)
        {
            return await GetStorageLimitsAsync(new GetStorageLimitsRequest(), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Sets storage limits for an account.
        /// </summary>
        public async Task<SetStorageLimitsResponse> SetStorageLimitsAsync(string accountId, StorageLimits limits, CancellationToken cancellationToken = default)
        {
            return await SetStorageLimitsAsync(new SetStorageLimitsRequest { AccountId = accountId, Limits = limits }, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets account-level drive settings.
        /// </summary>
        public async Task<GetAccountDriveSettingsResponse> GetAccountDriveSettingsAsync(CancellationToken cancellationToken = default)
        {
            return await GetAccountDriveSettingsAsync(new GetAccountDriveSettingsRequest(), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Sets account-level drive settings.
        /// </summary>
        public async Task<SetAccountDriveSettingsResponse> SetAccountDriveSettingsAsync(bool restrictFolderCreation, CancellationToken cancellationToken = default)
        {
            return await SetAccountDriveSettingsAsync(new SetAccountDriveSettingsRequest { RestrictFolderCreationToManagers = restrictFolderCreation }, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Soft-deletes a file (moves to trash).
        /// </summary>
        public async Task<DeleteFileResponse> DeleteFileAsync(string fileId, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync(new DeleteFileRequest { FileId = fileId }, cancellationToken: cancellationToken);
        }
    }
}
