using Lycoris.RabbitMQ.Extensions;
using Lycoris.RabbitMQ.Extensions.DataModel;
using System;
using System.Threading.Tasks;

namespace RabbitMQSample
{
    public class TestConsumer2 : BaseRabbitConsumer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        protected override Task<ReceivedHandler> ReceivedAsync(string body)
        {
            Console.WriteLine("123");
            return Task.FromResult(ReceivedHandler.Commit);
        }
    }
}
