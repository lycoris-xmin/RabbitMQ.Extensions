namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// RabbitMQ 生产者工厂接口，用于创建指定配置名称的生产者实例
    /// </summary>
    public interface IRabbitProducerFactory
    {
        /// <summary>
        /// 根据配置名称创建客户端生产者实例
        /// </summary>
        /// <param name="name">生产者配置名称</param>
        /// <returns>对应配置的客户端生产者实例</returns>
        IRabbitClientProducer Create(string name);
    }
}
