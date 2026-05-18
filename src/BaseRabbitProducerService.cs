namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 生产者服务基类，自动根据服务类型名称创建对应的客户端生产者实例
    /// </summary>
    public class BaseRabbitProducerService
    {
        /// <summary>
        /// 生产者工厂实例
        /// </summary>
        protected readonly IRabbitProducerFactory RabbitProducerFactory;

        /// <summary>
        /// 与当前服务类型关联的客户端生产者
        /// </summary>
        protected readonly IRabbitClientProducer Producer;

        /// <summary>
        /// 使用生产者工厂初始化，自动创建与服务类型同名的生产者
        /// </summary>
        /// <param name="rabbitProducerFactory">生产者工厂</param>
        public BaseRabbitProducerService(IRabbitProducerFactory rabbitProducerFactory)
        {
            this.RabbitProducerFactory = rabbitProducerFactory;
            this.Producer = rabbitProducerFactory.Create(this.GetType().FullName);
        }
    }
}
