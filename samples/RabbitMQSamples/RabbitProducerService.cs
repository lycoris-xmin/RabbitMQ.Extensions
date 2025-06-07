using Lycoris.RabbitMQ.Extensions;
using System.Threading.Tasks;

namespace RabbitMQSample
{
    public class RabbitProducerService : BaseRabbitProducerService, IRabbitProducerService
    {
        public RabbitProducerService(IRabbitProducerFactory rabbitProducerFactory) : base(rabbitProducerFactory)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public async Task Test()
        {
            await this.Producer.PublishAsync("route.your.routename", "this is push TestConsumer");
            await this.Producer.PublishAsync("route.your.routename2", "this is push TestConsumer2");
        }
    }
}
