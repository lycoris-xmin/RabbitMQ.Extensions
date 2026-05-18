using Lycoris.RabbitMQ.Extensions.Impl;
using System;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Builder.Consumer
{
    /// <summary>
    /// 消费者提供者接口，封装单个消费者的监听生命周期。
    /// 每个实现代表一个独立的 RabbitMQ 消费者实例，负责连接的建立、消息监听、资源释放等完整生命周期。
    /// </summary>
    public interface IRabbitConsumerProvider : IDisposable
    {
        /// <summary>
        /// 交换机名称。若为空字符串则表示仅监听队列，不绑定交换机。
        /// </summary>
        string Exchange { get; }

        /// <summary>
        /// 队列名称。
        /// </summary>
        string Queue { get; }

        /// <summary>
        /// RabbitMQ 消费者实例，封装底层的连接、通道和消息消费逻辑。
        /// </summary>
        RabbitConsumer Consumer { get; }

        /// <summary>
        /// 开始监听消息。
        /// 内部会建立 RabbitMQ 连接和通道，绑定队列并开始消费消息，若已启动则跳过。
        /// </summary>
        /// <returns>异步任务</returns>
        Task ListenAsync();
    }
}
