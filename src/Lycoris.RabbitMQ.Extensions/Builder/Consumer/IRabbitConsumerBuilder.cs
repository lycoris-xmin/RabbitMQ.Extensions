using Lycoris.RabbitMQ.Extensions.DataModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Builder.Consumer
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRabbitConsumerBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// 添加一个队列监听
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="onMessageRecieved"></param>
        /// <returns></returns>
        IRabbitConsumerBuilder AddConsumer(string queue, Func<IServiceProvider, RecieveResult, Task> onMessageRecieved);
        /// <summary>
        /// 添加一个交换机监听
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        /// <param name="onMessageRecieved"></param>
        /// <returns></returns>
        IRabbitConsumerBuilder AddConsumer(string exchange, string queue, Func<IServiceProvider, RecieveResult, Task> onMessageRecieved);
    }
}
