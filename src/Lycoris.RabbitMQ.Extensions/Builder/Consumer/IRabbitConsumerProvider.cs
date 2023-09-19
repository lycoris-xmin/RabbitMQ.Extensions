using Lycoris.RabbitMQ.Extensions.Impl;

namespace Lycoris.RabbitMQ.Extensions.Builder.Consumer
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRabbitConsumerProvider : IDisposable
    {
        /// <summary>
        /// 消费者
        /// </summary>
        RabbitConsumer Consumer { get; }

        /// <summary>
        /// 开始监听消费消息
        /// </summary>
        /// <returns></returns>
        Task ListenAsync();
    }
}
