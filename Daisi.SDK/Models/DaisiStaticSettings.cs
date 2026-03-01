using Daisi.SDK.Interfaces.Authentication;
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
            if (configuration.TryGetValue("Daisi:SsoSigningKey", out var ssoSigningKey))
            {
                SsoSigningKey = ssoSigningKey;
            }
            if (configuration.TryGetValue("Daisi:SsoAuthorityUrl", out var ssoAuthorityUrl))
            {
                SsoAuthorityUrl = ssoAuthorityUrl;
            }
            if (configuration.TryGetValue("Daisi:SsoAppUrl", out var ssoAppUrl))
            {
                SsoAppUrl = ssoAppUrl;
            }
            if (configuration.TryGetValue("Daisi:SsoAllowedOrigins", out var ssoAllowedOrigins))
            {
                SsoAllowedOrigins = ssoAllowedOrigins?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }
        }

        public static void AutoswapOrc()
        {
            string? aspNetCoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            OrcIpAddressOrDomain = "orc.daisinet.com";
            OrcPort = 443;
            OrcUseSSL = true;

            if (string.IsNullOrWhiteSpace(aspNetCoreEnvironment) || aspNetCoreEnvironment.ToLower() == "development")
            {
                NetworkName = "devnet";
            } 
            else
            {
                NetworkName = "mainnet";
            }


        }

        public static string OrcUrl => $"{OrcProtocol}://{OrcIpAddressOrDomain}:{OrcPort}";

        /// <summary>
        /// Allows consumers to set the network that they want to use. Default is "devnet".
        /// </summary>
        public static string NetworkName { get; set; } = "devnet";

        /// <summary>
        /// Determines whether the client should use SSL to connect to Hosts and Orcs.
        /// Default is True.
        /// </summary>
        public static bool OrcUseSSL { get; set; } = true;

        public static string OrcProtocol => OrcUseSSL ? "https" : "http";

        /// <summary>
        /// Gets or sets the IP address or domain name used to connect to the default Orc service.
        /// </summary>
        /// <remarks>Example: orc.daisi.ai or 192.168.0.1</remarks>   
        public static string OrcIpAddressOrDomain { get; set; } = "orc.daisinet.com";

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
        public static IClientKeyProvider? DefaultClientKeyProvider { get; set; }

        /// <summary>
        /// Applies user-configured Orc connection settings at runtime.
        /// </summary>
        public static void ApplyUserSettings(string domain, int port, bool useSsl)
        {
            if (!string.IsNullOrWhiteSpace(domain))
                OrcIpAddressOrDomain = domain;
            if (port > 0)
                OrcPort = port;
            OrcUseSSL = useSsl;
        }

        /// <summary>
        /// Base64-encoded AES-256 key shared across SSO-participating apps.
        /// Used to encrypt and decrypt SSO tickets during cross-app authentication.
        /// Configure via <c>Daisi:SsoSigningKey</c>.
        /// </summary>
        public static string? SsoSigningKey { get; set; }

        /// <summary>
        /// Base URL of the SSO authority (Identity Provider), e.g. "https://manager.daisinet.com".
        /// Unauthenticated users on relying-party apps are redirected here to log in.
        /// Configure via <c>Daisi:SsoAuthorityUrl</c>.
        /// </summary>
        public static string? SsoAuthorityUrl { get; set; }

        /// <summary>
        /// This app's own base URL, used to build the SSO callback URL.
        /// Configure via <c>Daisi:SsoAppUrl</c>.
        /// </summary>
        public static string? SsoAppUrl { get; set; }

        /// <summary>
        /// Origins allowed to request SSO tickets from this app's <c>/sso/authorize</c> endpoint.
        /// Requests with an <c>origin</c> not in this list are rejected with HTTP 400.
        /// Configure via <c>Daisi:SsoAllowedOrigins</c> (comma-separated).
        /// </summary>
        public static string[]? SsoAllowedOrigins { get; set; }

        public const string ClientKeyHeader = "x-daisi-client-key";

        /// <summary>
        /// Gets and sets the service provider to use throughout the tooling system.
        /// </summary>
        public static IServiceProvider Services { get; set; }

    }
}
