using Lycoris.RabbitMQ.Extensions;

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
        public void Test()
        {
            this.Producer.Publish("route.your.routename", "this is push TestConsumer");
            this.Producer.Publish("route.your.routename2", "this is push TestConsumer2");
        }
    }
}
