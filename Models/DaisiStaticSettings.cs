using Daisi.SDK.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

//[assembly: System.Reflection.AssemblyVersionAttribute("1.0.1")]
//[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.1")]

//[assembly: System.Reflection.AssemblyCompanyAttribute("Distributed AI Systems, Inc")]
//[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
//[assembly: System.Reflection.AssemblyCopyrightAttribute("Copyright 2025. Distributed AI Systems, Inc.")]
//[assembly: System.Reflection.AssemblyDescriptionAttribute("SDK for interfacing with the DAISI network.")]
//[assembly: System.Reflection.AssemblyProductAttribute("Daisi.SDK")]
//[assembly: System.Reflection.AssemblyTitleAttribute("Daisi.SDK")]
//[assembly: System.Reflection.AssemblyMetadataAttribute("RepositoryUrl", "https://github.com/daisinet/daisi-sdk-dotnet")]

namespace Daisi.SDK.Models
{
    public static class DaisiStaticSettings
    {
        public static void LoadFromConfiguration(IDictionary<string, string> configuration)
        {
            if (configuration.TryGetValue("Daisi:OrcIpAddressOrDomain", out var orcAddress))
            {
                OrcIpAddressOrDomain = orcAddress;
            }
            if (configuration.TryGetValue("Daisi:OrcPort", out var orcPortString) && int.TryParse(orcPortString, out var orcPort))
            {
                OrcPort = orcPort;
            }
            if (configuration.TryGetValue("Daisi:OrcUseSSL", out var useSSLString) && bool.TryParse(useSSLString, out var useSSL))
            {
                OrcUseSSL = useSSL;
            }
            if (configuration.TryGetValue("Daisi:ClientKey", out var clientKey))
            {
                ClientKey = clientKey;
            }
            if (configuration.TryGetValue("Daisi:SecretKey", out var secretKey))
            {
                SecretKey = secretKey;
            }
        }

        public static void AutoswapOrc()
        {
            string? aspNetCoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.IsNullOrWhiteSpace(aspNetCoreEnvironment) || aspNetCoreEnvironment.ToLower() == "development")
            {
                OrcIpAddressOrDomain = "orc-dev.daisinet.com";
                OrcPort = 443;
            }
            else
            {
                OrcIpAddressOrDomain = "orc-live.daisinet.com";
                OrcPort = 443;
            }

            OrcUseSSL = true;

        }

        public static string OrcUrl => $"{OrcProtocol}://{OrcIpAddressOrDomain}:{OrcPort}";

        /// <summary>
        /// Allows consumers to set the network that they want to use. Default is "devnet".
        /// </summary>
        public static string NetworkName { get; set; } = "devnet";

        /// <summary>
        /// Determines whether the client should use SSL to connect to hosts and orcs.
        /// </summary>
        public static bool OrcUseSSL { get; set; } = true;

        public static string OrcProtocol => OrcUseSSL ? "https" : "http";

        /// <summary>
        /// Gets or sets the IP address or domain name used to connect to the default Orc service.
        /// </summary>
        /// <remarks>Example: orc.daisi.ai or 192.168.0.1</remarks>   
        public static string OrcIpAddressOrDomain { get; set; } = "orc-live.daisi.ai";

        /// <summary>
        /// Gets or sets the port number used for the default ORC service connections.
        /// </summary>
        public static int OrcPort { get; set; } = 443;

        /// <summary>
        /// This could be the consumer or the host key needed to validate transactions.
        /// </summary>
        /// <remarks>Should be included in all headers such as "x-client-key: BASIC {ClientKey}".</remarks>        
        public static string? ClientKey { get; set; }

        /// <summary>
        /// Gets or sets the secret key used for authenticating the application with external services.
        /// </summary>
        /// <remarks>
        /// The secret key is required when integrating with the DAISI network. 
        /// Ensure that this value is stored securely and not exposed in client-side code or
        /// public repositories.
        /// </remarks>
        public static string? SecretKey { get; set; }
        public static IClientKeyProvider? DefaultClientKeyProvider { get; internal set; }

        public const string ClientKeyHeader = "x-daisi-client-key";


    }
}
