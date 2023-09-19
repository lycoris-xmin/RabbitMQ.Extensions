using Lycoris.RabbitMQ.Extensions.DataModel;
using Lycoris.RabbitMQ.Extensions.Impl;

namespace RabbitMQSample
{
    public class TestConsumer2 : RabbitConsumerListener
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
