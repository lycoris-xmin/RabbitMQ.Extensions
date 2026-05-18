using Lycoris.RabbitMQ.Extensions.DataModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Builder.Consumer
{
    /// <summary>
    /// 消费者构建器接口，用于添加队列和交换机监听。
    /// 提供流畅 API 来配置消息消费回调，支持队列直连监听和交换机绑定监听两种模式。
    /// </summary>
    public interface IRabbitConsumerBuilder
    {
        /// <summary>
        /// 依赖注入服务集合，用于注册消费者相关的服务和组件。
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// 添加指定队列的消息监听回调。
        /// 注册一个 <see cref="IRabbitConsumerProvider"/> 到 DI 容器，在启动时开始监听指定队列的消息。
        /// </summary>
        /// <param name="queue">队列名称</param>
        /// <param name="onMessageRecieved">消息接收回调，接收 <see cref="IServiceProvider"/> 和 <see cref="RecieveResult"/> 两个参数</param>
        /// <returns>消费者构建器，支持链式调用以添加更多监听</returns>
        IRabbitConsumerBuilder AddConsumer(string queue, Func<IServiceProvider, RecieveResult, Task> onMessageRecieved);

        /// <summary>
        /// 添加指定交换机和队列的消息监听回调。
        /// 注册一个 <see cref="IRabbitConsumerProvider"/> 到 DI 容器，在启动时绑定指定交换机并开始监听队列消息。
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="onMessageRecieved">消息接收回调，接收 <see cref="IServiceProvider"/> 和 <see cref="RecieveResult"/> 两个参数</param>
        /// <returns>消费者构建器，支持链式调用以添加更多监听</returns>
        IRabbitConsumerBuilder AddConsumer(string exchange, string queue, Func<IServiceProvider, RecieveResult, Task> onMessageRecieved);
    }
}
