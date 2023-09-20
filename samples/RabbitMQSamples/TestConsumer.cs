using Lycoris.RabbitMQ.Extensions;
using Lycoris.RabbitMQ.Extensions.DataModel;

namespace RabbitMQSample
{
    public class TestConsumer : RabbitConsumerListener
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
