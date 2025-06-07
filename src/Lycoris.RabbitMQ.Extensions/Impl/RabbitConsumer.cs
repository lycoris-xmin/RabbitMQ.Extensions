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
    /// 
    /// </summary>
    public sealed class RabbitConsumer : BaseRabbit
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="hostAndPorts"></param>
        public RabbitConsumer(IServiceProvider provider, params string[] hostAndPorts) : base(provider, hostAndPorts)
        {

        }

        /// <summary>
        /// 开始消费
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="queue"></param>
        /// <param name="autoAck"></param>
        /// <param name="fetchCount"></param>
        /// <param name="received"></param>
        /// <returns></returns>
        private static async Task<ListenResult> ConsumeInternalAsync(IChannel channel, string queue, bool autoAck, ushort? fetchCount, Func<RecieveResult, Task> received)
        {
            if (fetchCount != null)
                await channel.BasicQosAsync(0, fetchCount.Value, true);

            var consumer = new AsyncEventingBasicConsumer(channel);

            if (received != null)
            {
                consumer.ReceivedAsync += async (sender, e) =>
                {
                    var cancellationTokenSource = new CancellationTokenSource();
                    using (var result = new RecieveResult(e, cancellationTokenSource))
                    {

                        if (!autoAck)
                        {
                            cancellationTokenSource.Token.Register(async r =>
                            {
                                if (r == null)
                                    throw new ArgumentNullException(nameof(r));

                                if (r is RecieveResult recieveResult)
                                {
                                    if (recieveResult.IsCommit)
                                        await channel.BasicAckAsync(e.DeliveryTag, false);
                                    else
                                        await channel.BasicNackAsync(e.DeliveryTag, false, recieveResult.Requeue);
                                }
                                else
                                    throw new ArgumentNullException(nameof(recieveResult));

                            }, result);
                        }

                        try
                        {
                            await received.Invoke(result);
                        }
                        catch
                        {
                            if (!autoAck)
                            {
                                result.RollBack();
                            }
                        }
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
        /// 从队列消费消息
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="options"></param>
        /// <param name="received"></param>
        /// <returns></returns>
        public async Task<ListenResult> ListenAsync(string queue, ConsumeQueueOption options = null, Func<RecieveResult, Task> received = null)
        {
            if (options == null)
                options = new ConsumeQueueOption();

            var channel = await GetChannelAsync();

            await PrepareQueueChannelAsync(channel, queue, options);

            return await ConsumeInternalAsync(channel, queue, options.AutoAck, options.FetchCount, received);
        }

        /// <summary>
        /// 从队列消费消息
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="configure"></param>
        /// <param name="received"></param>
        /// <returns></returns>
        public Task<ListenResult> ListenAsync(string queue, Action<ConsumeQueueOption> configure, Func<RecieveResult, Task> received = null)
        {
            var options = new ConsumeQueueOption();

            configure?.Invoke(options);

            return ListenAsync(queue, options, received);
        }
        #endregion

        #region 订阅模式、路由模式、Topic模式
        /// <summary>
        /// 消费消息
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        /// <param name="options"></param>
        /// <param name="received"></param>
        /// <returns></returns>
        public async Task<ListenResult> ListenAsync(string exchange, string queue, ExchangeConsumeQueueOption options = null, Func<RecieveResult, Task> received = null)
        {
            if (string.IsNullOrEmpty(exchange))
                throw new ArgumentException("exchange cannot be empty", nameof(exchange));

            if (options == null)
                throw new ArgumentNullException("ExchangeConsumeQueueOptions cannot be empty", nameof(options));

            if (options.Type == RabbitExchangeType.None)
                throw new NotSupportedException($"{nameof(RabbitExchangeType)} must be specified");

            if (options == null)
                options = new ExchangeConsumeQueueOption();

            var channel = await GetChannelAsync();

            await PrepareExchangeChannelAsync(channel, exchange, options);

            return await ConsumeInternalAsync(channel, queue, options.AutoAck, options.FetchCount, received);
        }
        /// <summary>
        /// 消费消息
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        /// <param name="configure"></param>
        /// <param name="received"></param>
        /// <returns></returns>
        public Task<ListenResult> ListenAsync(string exchange, string queue, Action<ExchangeConsumeQueueOption> configure, Func<RecieveResult, Task> received = null)
        {
            var options = new ExchangeConsumeQueueOption();

            configure?.Invoke(options);

            return ListenAsync(exchange, queue, options, received);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="rabbitBaseOptions"></param>
        /// <returns></returns>
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
