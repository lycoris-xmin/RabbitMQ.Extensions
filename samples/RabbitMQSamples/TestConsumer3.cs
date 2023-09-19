using Lycoris.RabbitMQ.Extensions.DataModel;
using Lycoris.RabbitMQ.Extensions.Impl;

namespace RabbitMQSample
{
    public class TestConsumer3 : RabbitConsumerListener
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        protected override Task<ReceivedHandler> ReceivedAsync(string body)
        {
            Console.WriteLine($"TestConsumer3 ==> PushTime:{body} => ReceivedTime:{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            return Task.FromResult(ReceivedHandler.Commit);
        }
    }
}
