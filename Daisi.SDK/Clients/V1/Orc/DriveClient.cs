using Daisi.Protos.V1;
using Daisi.SDK.Interfaces.Authentication;
using Daisi.SDK.Models;
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
                return metadata;
            }))
        {
        }

        internal DriveClient(string domainOrIp, int port, IClientKeyProvider clientKeyProvider)
            : this(GrpcChannel.ForAddress($"{(DaisiStaticSettings.OrcUseSSL ? "https" : "http")}://{domainOrIp}:{port}"), clientKeyProvider)
        {
        }

        /// <summary>
        /// Uploads a file to Drive via streaming.
        /// </summary>
        public async Task<UploadResponse> UploadFileAsync(Stream fileStream, string fileName, string path = "/",
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
        /// Searches files by partial filename (for #autocomplete).
        /// </summary>
        public async Task<FileSearchResponse> SearchFilesAsync(string query, int maxResults = 10, CancellationToken cancellationToken = default)
        {
            return await SearchFilesAsync(new FileSearchRequest { Query = query, MaxResults = maxResults }, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Performs semantic vector search over file contents.
        /// </summary>
        public async Task<VectorSearchResponse> VectorSearchAsync(string query, int topK = 5, bool includeSystemFiles = false, CancellationToken cancellationToken = default)
        {
            return await VectorSearchAsync(new VectorSearchRequest { Query = query, TopK = topK, IncludeSystemFiles = includeSystemFiles }, cancellationToken: cancellationToken);
        }
    }
}
