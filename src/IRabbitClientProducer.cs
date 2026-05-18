using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// RabbitMQ 客户端生产者接口，提供向队列或交换机发送消息的能力
    /// </summary>
    public interface IRabbitClientProducer : IDisposable
    {
        /// <summary>
        /// 向普通队列发送单条消息
        /// </summary>
        /// <param name="messages">消息内容</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task PublishAsync(string messages, CancellationToken cancellationToken = default);

        /// <summary>
        /// 向普通队列发送多条消息
        /// </summary>
        /// <param name="messages">消息内容数组</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task PublishAsync(string[] messages, CancellationToken cancellationToken = default);

        /// <summary>
        /// 通过交换机发送单条消息
        /// </summary>
        /// <param name="routingKey">路由键</param>
        /// <param name="messages">消息内容</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task PublishAsync(string routingKey, string messages, CancellationToken cancellationToken = default);

        /// <summary>
        /// 通过交换机发送多条消息
        /// </summary>
        /// <param name="routingKey">路由键</param>
        /// <param name="messages">消息内容数组</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task PublishAsync(string routingKey, string[] messages, CancellationToken cancellationToken = default);
    }
}
