using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;


namespace GrpcServiceDemo.Services
{
    public class Service : TestService.TestServiceBase
    {

        public Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hi : " + request.Name
            });
        }

        public override async Task SayHellos(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            for (var i = 0; i < 10; i++)
            {
                await responseStream.WriteAsync(new HelloReply
                {
                    Message = i + " : " + request.Name
                });
                //await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
        }

        public override async Task<HelloReply> SayClientHellos(IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                Console.WriteLine(requestStream.Current.Name);
            }
            return new HelloReply()
            {
                Message = "End."
            };
        }
    }
}
