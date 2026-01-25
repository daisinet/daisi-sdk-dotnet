using Daisi.Protos.V1;
using Daisi.SDK.Clients.V1.Host;

namespace Daisi.Tests.Inference;

[TestClass]
public class BasicThinkWithTools
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
    public async Task SendABasicMathQuestionToSeeIfTheMathToolIsCalled()
    {
        var response = client.Send("If a man owns David has four dogs and two cats, what is the percentage of cats in the total number of animals that David owns?", ThinkLevels.BasicWithTools);
        string result = string.Empty;

        CancellationTokenSource cts = new CancellationTokenSource();

        List<SendInferenceResponse> responses = new();

        while (await response.ResponseStream.MoveNext(cts.Token))
        {
            responses.Add(response.ResponseStream.Current);
        }

        

    }
}
