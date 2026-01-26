using Daisi.Protos.V1;
using Daisi.SDK.Clients.V1.Host;
using Daisi.SDK.Extensions;
using Daisi.SDK.Models;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using Console = System.Console;

namespace Daisi.Console.Chat
{
    internal class DaisiChatBot(InferenceClientFactory inferenceClientFactory) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            System.Console.WriteLine($"Loading Chat Session with {DaisiStaticSettings.OrcUrl}");


            // Create the client that we will use to connect to the
            // Daisi network and communicate with the host
            var inferenceClient = inferenceClientFactory.Create();


            // OPTIONAL: Create the inference session with an initialization prompt.
            // This isn't strictly necessary and the client will do it if
            // you don't call it manually. Uncomment to use this option.

            //var inferenceSession = await inferenceClient.CreateAsync(new CreateInferenceRequest()
            //{
            //    InitializationPrompt = "You are a friendly Daisi chatbot helping a new user understand the world.",
            //    ThinkLevel = ThinkLevels.ThinkBasic                
            //});

            // Give the user some general instructions
            System.Console.WriteLine($"Welcome to DaisiBot! Ask a question or type \"exit\" to stop or \"new\" to start a new chat.\n\n");

            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.Write("User: ");
            System.Console.ForegroundColor = ConsoleColor.White;

            // Keep the chat going until the user wants to stop or the application stops
            while (!stoppingToken.IsCancellationRequested) 
            {

                //Take the user's input              
                string inputFromUser = System.Console.ReadLine();

                //Make sure the user doesn't want to quit.
                if (inputFromUser != "exit" 
                    && inputFromUser != "new"
                    && !string.IsNullOrWhiteSpace(inputFromUser))
                {
                    // Send the user's input to DAISI. In this case, we are
                    // letting the Orc (Orchestrator) and the client figure everything out.
                    var response = inferenceClient.Send(inputFromUser, ThinkLevels.BasicWithTools);

                    System.Console.ForegroundColor = ConsoleColor.Green;
                    System.Console.Write($"\nAssistant: ");
                    System.Console.ForegroundColor = ConsoleColor.White;

                    // Cycle through each part of the response until there are no 
                    // more parts in the response and write each part to the console.
                    while (await response.ResponseStream.MoveNext(stoppingToken))
                    {
                        var resp = response.ResponseStream.Current;
                        if(resp.Type == InferenceResponseTypes.Error) //Error message will be sent in complete chunks
                            System.Console.WriteLine($"\nERROR: {resp.Content.CleanupAssistantResponse()}");
                        else if (resp.Type == InferenceResponseTypes.Tooling) //Tooling message will be sent in complete chunks
                            System.Console.WriteLine($"\nTOOL: {resp.Content.CleanupAssistantResponse()}");
                        else if (resp.Type == InferenceResponseTypes.Thinking || resp.Type == InferenceResponseTypes.Text)
                            System.Console.Write(resp.Content.CleanupAssistantResponse());
                    }

                    System.Console.WriteLine();
                    // Ask the user for more input
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.Write($"\nUser: ");
                    System.Console.ForegroundColor = ConsoleColor.White;

                }
                else if(inputFromUser == "new")
                {
                    await inferenceClient.CloseAsync();
                    inferenceClient = inferenceClientFactory.Create();
                }
                else
                    break;
            }

            // Closes the inference and logs everything appropriately.
            // The inference session will timeout if you don't close it like this.
            // Each timeout is a small negative on reputation, but they add up,
            // so also be sure to close out your client requests.
            await inferenceClient.CloseAsync();
        }
    }
}
