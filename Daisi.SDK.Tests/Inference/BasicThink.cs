using Daisi.Protos.V1;
using Daisi.SDK.Clients.V1.Host;
using Daisi.SDK.Clients.V1.Orc;
using Daisi.SDK.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Daisi.Tests.Inference
{
    [TestClass]
    public sealed class BasicThink
    {
        static InferenceClientFactory Factory { get; set; }

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            // This method is called once for the test class, before any tests of the class are run.
            Factory = new InferenceClientFactory();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // This method is called once for the test class, after all tests of the class are run.
        }

        InferenceClient client;

        [TestInitialize]
        public void TestInit()
        {
            // This method is called before each test method.
            client = Factory.Create();
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            // This method is called after each test method.
            await client.CloseAsync(true);

        }

        [TestMethod]
        public async Task TestForThinkingAndResponseTags()
        {         
            var response = client.Send("Tell me a joke.", ThinkLevels.Basic);
            string result = string.Empty;

            CancellationTokenSource cts = new CancellationTokenSource();

            while(await response.ResponseStream.MoveNext(cts.Token))
            {
                var infResponse = response.ResponseStream.Current;

                Assert.AreEqual(Protos.V1.InferenceResponseTypes.Text, infResponse.Type, $"Type should be Text, but is {infResponse.Type}");
                Assert.AreEqual(Protos.V1.InferenceOutputFormats.PlainText, infResponse.Format, $"Format should be PlainText, but is {infResponse.Format}");
                Assert.AreEqual("Assistant", infResponse.AuthorRole, $"AuthRole should be Assistant, but is {infResponse.AuthorRole}");
                result += infResponse.Content;                
            }

            Assert.Contains("<think>", result, "Result does not contain the <think> opening tag.");
            Assert.Contains("</think>", result, "Result does not contain the </think> closing tag.");
            Assert.Contains("<response>", result, "Result does not contain the <response> opening tag.");
            Assert.Contains("</response>", result, "Result does not contain the </response> closing tag.");
        }
    }
}
