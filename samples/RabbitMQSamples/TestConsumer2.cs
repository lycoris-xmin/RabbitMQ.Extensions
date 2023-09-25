using Lycoris.RabbitMQ.Extensions;
using Lycoris.RabbitMQ.Extensions.DataModel;

namespace RabbitMQSample
{
    public class TestConsumer2 : BaseRabbitConsumerListener
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
