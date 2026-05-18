using Lycoris.RabbitMQ.Extensions.Base;
using Lycoris.RabbitMQ.Extensions.DataModel;
using Lycoris.RabbitMQ.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// RabbitMQ 消费者，封装消息消费监听逻辑，支持普通队列和交换机两种消费模式
    /// </summary>
    public sealed class RabbitConsumer : BaseRabbit
    {
        /// <summary>
        /// 使用服务提供者和主机列表初始化消费者
        /// </summary>
        /// <param name="provider">依赖注入服务提供者</param>
        /// <param name="hostAndPorts">主机地址列表，格式为 "host:port" 或 "host"</param>
        public RabbitConsumer(IServiceProvider provider, params string[] hostAndPorts) : base(provider, hostAndPorts)
        {

        }

        /// <summary>
        /// 内部消费逻辑：绑定异步消费者到指定队列，处理手动/自动确认和异常回滚
        /// </summary>
        /// <param name="channel">AMQP 信道</param>
        /// <param name="queue">队列名称</param>
        /// <param name="autoAck">是否自动确认</param>
        /// <param name="fetchCount">预取数量</param>
        /// <param name="received">消息接收回调</param>
        /// <returns>监听结果，可用于取消监听</returns>
        private static async Task<ListenResult> ConsumeInternalAsync(IChannel channel, string queue, bool autoAck, ushort? fetchCount, Func<RecieveResult, Task> received)
        {
            if (fetchCount != null)
                await channel.BasicQosAsync(0, fetchCount.Value, true);

            var consumer = new AsyncEventingBasicConsumer(channel);

            if (received != null)
            {
                consumer.ReceivedAsync += async (sender, e) =>
                {
                    var result = new RecieveResult(e);

                    try
                    {
                        await received.Invoke(result);
                    }
                    catch
                    {
                        if (!autoAck)
                            result.RollBack();
                    }

                    if (!autoAck)
                    {
                        if (result.IsCommit)
                            await channel.BasicAckAsync(e.DeliveryTag, false);
                        else
                            await channel.BasicNackAsync(e.DeliveryTag, false, result.Requeue);
                    }
                };
            }

            await channel.BasicConsumeAsync(queue, autoAck, consumer);

            var listenResult = new ListenResult();

            listenResult.Token.Register(async () =>
            {
                try
                {
                    await channel.CloseAsync();
                    await channel.DisposeAsync();
                }
                catch { }
            });

            return listenResult;
        }

        #region 普通模式、Work模式
        /// <summary>
        /// 开始监听指定队列的消息
        /// </summary>
        /// <param name="queue">队列名称</param>
        /// <param name="options">消费队列选项</param>
        /// <param name="received">消息接收回调</param>
        /// <returns>监听结果，可用于取消监听</returns>
        public async Task<ListenResult> ListenAsync(string queue, ConsumeQueueOption options = null, Func<RecieveResult, Task> received = null)
        {
            if (options == null)
                options = new ConsumeQueueOption();

            var channel = await GetChannelAsync();

            await PrepareQueueChannelAsync(channel, queue, options);

            return await ConsumeInternalAsync(channel, queue, options.AutoAck, options.FetchCount, received);
        }

        /// <summary>
        /// 使用配置委托开始监听指定队列的消息
        /// </summary>
        /// <param name="queue">队列名称</param>
        /// <param name="configure">消费选项配置委托</param>
        /// <param name="received">消息接收回调</param>
        /// <returns>监听结果，可用于取消监听</returns>
        public Task<ListenResult> ListenAsync(string queue, Action<ConsumeQueueOption> configure, Func<RecieveResult, Task> received = null)
        {
            var options = new ConsumeQueueOption();

            configure?.Invoke(options);

            return ListenAsync(queue, options, received);
        }
        #endregion

        #region 订阅模式、路由模式、Topic模式
        /// <summary>
        /// 开始监听指定交换机和队列的消息
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="options">交换机消费队列选项</param>
        /// <param name="received">消息接收回调</param>
        /// <returns>监听结果，可用于取消监听</returns>
        /// <exception cref="ArgumentException">交换机名称为空时抛出</exception>
        /// <exception cref="ArgumentNullException">选项为 null 时抛出</exception>
        /// <exception cref="NotSupportedException">未指定交换机类型时抛出</exception>
        public async Task<ListenResult> ListenAsync(string exchange, string queue, ExchangeConsumeQueueOption options = null, Func<RecieveResult, Task> received = null)
        {
            if (string.IsNullOrEmpty(exchange))
                throw new ArgumentException("exchange cannot be empty", nameof(exchange));

            if (options == null)
                throw new ArgumentNullException("ExchangeConsumeQueueOptions cannot be empty", nameof(options));

            if (options.Type == RabbitExchangeType.None)
                throw new NotSupportedException($"{nameof(RabbitExchangeType)} must be specified");

            var channel = await GetChannelAsync();

            await PrepareExchangeChannelAsync(channel, exchange, options);

            return await ConsumeInternalAsync(channel, queue, options.AutoAck, options.FetchCount, received);
        }

        /// <summary>
        /// 使用配置委托开始监听指定交换机和队列的消息
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="configure">交换机消费选项配置委托</param>
        /// <param name="received">消息接收回调</param>
        /// <returns>监听结果，可用于取消监听</returns>
        public Task<ListenResult> ListenAsync(string exchange, string queue, Action<ExchangeConsumeQueueOption> configure, Func<RecieveResult, Task> received = null)
        {
            var options = new ExchangeConsumeQueueOption();

            configure?.Invoke(options);

            return ListenAsync(exchange, queue, options, received);
        }
        #endregion

        /// <summary>
        /// 根据配置创建消费者实例
        /// </summary>
        /// <param name="serviceProvider">依赖注入服务提供者</param>
        /// <param name="rabbitBaseOptions">消费者配置选项</param>
        /// <returns>配置完成的 <see cref="RabbitConsumer"/> 实例</returns>
        public static RabbitConsumer Create(IServiceProvider serviceProvider, RabbitConsumerOption rabbitBaseOptions)
        {
            var consumer = new RabbitConsumer(serviceProvider, rabbitBaseOptions.Hosts)
            {
                Password = rabbitBaseOptions.Password,
                Port = rabbitBaseOptions.Port,
                UserName = rabbitBaseOptions.UserName,
                VirtualHost = rabbitBaseOptions.VirtualHost
            };

            return consumer;
        }
    }
}
