namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRabbitProducerFactory
    {
        /// <summary>
        /// 创建客户端生产者
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IRabbitClientProducer Create(string name);
    }
}
