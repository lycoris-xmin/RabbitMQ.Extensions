using Lycoris.RabbitMQ.Extensions.Impl;
using System;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Builder.Consumer
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRabbitConsumerProvider : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        string Exchange { get; }

        /// <summary>
        /// 
        /// </summary>
        string Queue { get; }

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
