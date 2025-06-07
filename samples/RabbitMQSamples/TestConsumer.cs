using Lycoris.RabbitMQ.Extensions;
using Lycoris.RabbitMQ.Extensions.DataModel;
using System;
using System.Threading.Tasks;

namespace RabbitMQSample
{
    public class TestConsumer : BaseRabbitConsumer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        protected override Task<ReceivedHandler> ReceivedAsync(string body)
        {
            Console.WriteLine($"TestConsumer ==> {body}");
            return Task.FromResult(ReceivedHandler.Commit);
        }
    }
}
