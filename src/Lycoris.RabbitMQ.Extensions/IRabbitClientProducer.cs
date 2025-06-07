using System;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRabbitClientProducer : IDisposable
    {
        /// <summary>
        /// 普通的往队列发送消息
        /// </summary>
        /// <param name="messages"></param>
        Task PublishAsync(string messages);

        /// <summary>
        /// 普通的往队列发送消息
        /// </summary>
        /// <param name="messages"></param>
        Task PublishAsync(string[] messages);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routingKey"></param>
        /// <param name="messages"></param>
        Task PublishAsync(string routingKey, string messages);

        /// <summary>
        /// 使用交换机发送消息
        /// </summary>
        /// <param name="routingKey"></param>
        /// <param name="messages"></param>
        Task PublishAsync(string routingKey, string[] messages);
    }
}
