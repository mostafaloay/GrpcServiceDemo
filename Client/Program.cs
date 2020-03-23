using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcServiceDemo;

namespace Client
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new TestService.TestServiceClient(channel);
            await ServerStreamingAsync(client);
            Console.ReadKey();
        }

        private static async Task UnaryCall(TestService.TestServiceClient client)
        {
            var response = await client.SayHelloAsync(new HelloRequest { Name = "World" });
            Console.WriteLine("Greeting: " + response.Message);
        }
        private static async Task ServerStreamingAsync(TestService.TestServiceClient client)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(4));
            using var reply = client.SayHellos(new HelloRequest { Name = "mostafa laoy" });
            try
            {
                while (await reply.ResponseStream.MoveNext())
                {
                    Console.WriteLine(reply.ResponseStream.Current.Message);
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("End Server Stream");

            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Stream cancelled.");
            }
        }
    }
}
