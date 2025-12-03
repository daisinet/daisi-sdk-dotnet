using Daisi.Protos.V1;
using Daisi.SDK.Interfaces;
using Daisi.SDK.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Daisi.SDK.Clients.V1.Orc
{
    public class CommandClientFactory(IClientKeyProvider clientKeyProvider)
    {
        public HostCommandClient Create(string? orcDomainOrIp = default, int? orcPort = null)
        {
            return new HostCommandClient(orcDomainOrIp ?? DaisiStaticSettings.OrcIpAddressOrDomain, orcPort ?? DaisiStaticSettings.OrcPort, clientKeyProvider);
        }

    }
    public class HostCommandClient : HostCommandsProto.HostCommandsProtoClient
    {
        private HostCommandClient(GrpcChannel orcChannel, IClientKeyProvider clientKeyProvider)
          : base(orcChannel.Intercept((metadata) =>
          {
              if (clientKeyProvider is not null)
                  metadata.Add(DaisiStaticSettings.ClientKeyHeader, clientKeyProvider.GetClientKey());
              else
                  metadata.Add(DaisiStaticSettings.ClientKeyHeader, DaisiStaticSettings.ClientKey);

              return metadata;
          }))
        {
            
        }
        internal HostCommandClient(string orcDomainOrIp, int orcPort, IClientKeyProvider clientKeyProvider)
            : this(GrpcChannel.ForAddress($"{(DaisiStaticSettings.OrcUseSSL ? "https" : "http")}://{orcDomainOrIp}:{orcPort}"), clientKeyProvider)
        {

        }

        public override AsyncDuplexStreamingCall<Command, Command> Open(CallOptions options)
        {
            var result = base.Open(options);


            try
            {
                EnvironmentRequest environmentRequest = new()
                {
                    AppVersion = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString(),
                    OperatingSystemVersion = Environment.OSVersion.VersionString
                };

                if (OperatingSystem.IsWindows())
                {
                    environmentRequest.OperatingSystem = "Windows";
                }
                if (OperatingSystem.IsLinux())
                {
                    environmentRequest.OperatingSystem = "Linux";
                }
                if (OperatingSystem.IsAndroid())
                {
                    environmentRequest.OperatingSystem = "Android";
                }
                if (OperatingSystem.IsIOS())
                {
                    environmentRequest.OperatingSystem = "IOS";
                }
                if (OperatingSystem.IsMacCatalyst())
                {
                    environmentRequest.OperatingSystem = "MacCatalyst";
                }
                if (OperatingSystem.IsMacOS())
                {
                    environmentRequest.OperatingSystem = "MacOS";
                }

                Command environmentCommand = new Command()
                {
                    Name = nameof(EnvironmentRequest),
                    Payload = Any.Pack(environmentRequest)
                };

                result.RequestStream.WriteAsync(environmentCommand);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Building Environment Command: {ex.GetBaseException().Message}");
            }



            return result;

        }
    }
}
