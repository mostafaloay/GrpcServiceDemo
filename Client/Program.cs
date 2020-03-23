using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcServiceDemo;

namespace Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new TestService.TestServiceClient(channel);
            await BiDirectionalStreaming(client);
            Console.ReadKey();
        }

        private static async Task UnaryCall(TestService.TestServiceClient client)
        {
            var response = await client.SayHelloAsync(new HelloRequest {Name = "World"});
            Console.WriteLine("Greeting: " + response.Message);
        }

        private static async Task ServerStreamingAsync(TestService.TestServiceClient client)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(4));
            using var reply = client.SayHellos(new HelloRequest {Name = "mostafa laoy"});
            try
            {
                while (await reply.ResponseStream.MoveNext()) Console.WriteLine(reply.ResponseStream.Current.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("End Server Stream");
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Stream cancelled.");
            }
        }

        private static async Task ClientStreamingAsync(TestService.TestServiceClient client)
        {
            //var cts = new CancellationTokenSource();
            //cts.CancelAfter(TimeSpan.FromSeconds(4));
            using var call = client.SayClientHellos();
            try
            {
                for (var i = 0; i < 10; i++)
                {
                    Console.WriteLine("Number : " + i);
                    await call.RequestStream.WriteAsync(new HelloRequest()
                    {
                        Name = "Block Stream Number : " + i
                    });
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                }

                await call.RequestStream.CompleteAsync();
                var response = await call;
                // print final response
                Console.WriteLine(response.Message);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Stream cancelled.");
            }
        }

        private static async Task BiDirectionalStreaming(TestService.TestServiceClient client)
        {
            using var call = client.BiDirectionalStreaming();
            Console.WriteLine("Starting background task to receive messages");
            var readTask = Task.Run(async () =>
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                    Console.WriteLine(response.Message);
                // Echo messages sent to the service
            });

            Console.WriteLine("Starting to send messages");
            Console.WriteLine("Type a message to echo then press enter.");
            while (true)
            {
                var result = Console.ReadLine();
                if (string.IsNullOrEmpty(result)) break;
                await call.RequestStream.WriteAsync(new HelloRequest()
                {
                    Name = result
                });
            }

            Console.WriteLine("Disconnecting");
            await call.RequestStream.CompleteAsync();
            await readTask;
        }
    }
}