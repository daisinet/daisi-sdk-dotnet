using Daisi.SDK.Clients.V1.Orc;
using Daisi.SDK.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Testing.Platform.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.Tests
{
    [TestClass]
    public sealed class Setup
    {
        static AuthClientFactory AuthClientFactory { get; set; }

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            AuthClientFactory = new AuthClientFactory();

            var Configuration = new ConfigurationBuilder()
                                .AddUserSecrets<Setup>()
                                .Build();

            DaisiStaticSettings.SecretKey = Configuration["SecretKey"];

            Console.WriteLine(DaisiStaticSettings.SecretKey);

            DaisiStaticSettings.ClientKey = string.Empty;
            DaisiStaticSettings.DefaultClientKeyProvider = new DefaultClientKeyProvider();
            DaisiStaticSettings.NetworkName = "devnet";
            DaisiStaticSettings.OrcIpAddressOrDomain = "localhost";
            DaisiStaticSettings.OrcPort = 5001;
            DaisiStaticSettings.OrcUseSSL = true;

            var authClient = AuthClientFactory.Create();
            var response = authClient.CreateClientKey(new Daisi.Protos.V1.CreateClientKeyRequest() { SecretKey = DaisiStaticSettings.SecretKey });
            DaisiStaticSettings.ClientKey = response.ClientKey;
            Console.WriteLine(DaisiStaticSettings.ClientKey);

        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            // This method is called once for the test assembly, after all tests are run.
            var authClient = AuthClientFactory.Create();
            //authClient.DeleteClientKey(new Protos.V1.DeleteClientKeyRequest() { ClientKey = DaisiStaticSettings.ClientKey, SecretKey = DaisiStaticSettings.SecretKey });
        }
    }
}
