using Lycoris.RabbitMQ.Extensions;
using Lycoris.RabbitMQ.Extensions.DataModel;
using System;
using System.Threading.Tasks;

namespace RabbitMQSample
{
    public class TestConsumer3 : BaseRabbitConsumer
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
