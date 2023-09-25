namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public class BaseRabbitProducerService
    {
        /// <summary>
        /// 
        /// </summary>
        protected readonly IRabbitProducerFactory RabbitProducerFactory;

        /// <summary>
        /// 
        /// </summary>
        protected readonly IRabbitClientProducer Producer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rabbitProducerFactory"></param>
        public BaseRabbitProducerService(IRabbitProducerFactory rabbitProducerFactory)
        {
            this.RabbitProducerFactory = rabbitProducerFactory;
            this.Producer = rabbitProducerFactory.Create(this.GetType().FullName);
        }
    }
}
