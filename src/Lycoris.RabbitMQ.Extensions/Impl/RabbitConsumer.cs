using Lycoris.RabbitMQ.Extensions.Base;
using Lycoris.RabbitMQ.Extensions.DataModel;
using Lycoris.RabbitMQ.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;

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
        /// <param name="hostAndPorts"></param>
        public RabbitConsumer(params string[] hostAndPorts) : base(hostAndPorts)
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
        private static ListenResult ConsumeInternal(IModel channel, string queue, bool autoAck, ushort? fetchCount, Action<RecieveResult> received)
        {
            if (fetchCount != null)
                channel.BasicQos(0, fetchCount.Value, true);

            var consumer = new EventingBasicConsumer(channel);

            if (received != null)
            {
                consumer.Received += (sender, e) =>
                {
                    var cancellationTokenSource = new CancellationTokenSource();
                    using (var result = new RecieveResult(e, cancellationTokenSource))
                    {

                        if (!autoAck)
                        {
                            cancellationTokenSource.Token.Register(r =>
                            {
                                if (r == null)
                                    throw new ArgumentNullException(nameof(r));

                                if (r is RecieveResult recieveResult)
                                {
                                    if (recieveResult.IsCommit)
                                        channel.BasicAck(e.DeliveryTag, false);
                                    else
                                        channel.BasicNack(e.DeliveryTag, false, recieveResult.Requeue);
                                }
                                else
                                    throw new ArgumentNullException(nameof(recieveResult));

                            }, result);
                        }

                        try
                        {
                            received.Invoke(result);
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

            channel.BasicConsume(queue, autoAck, consumer);
            var listenResult = new ListenResult();
            listenResult.Token.Register(() =>
            {
                try
                {
                    channel.Close();
                    channel.Dispose();
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
        public ListenResult Listen(string queue, ConsumeQueueOptions options = null, Action<RecieveResult> received = null)
        {
            if (options == null)
                options = new ConsumeQueueOptions();

            var channel = GetChannel();
            PrepareQueueChannel(channel, queue, options);

            return ConsumeInternal(channel, queue, options.AutoAck, options.FetchCount, received);
        }

        /// <summary>
        /// 从队列消费消息
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="configure"></param>
        /// <param name="received"></param>
        /// <returns></returns>
        public ListenResult Listen(string queue, Action<ConsumeQueueOptions> configure, Action<RecieveResult> received = null)
        {
            var options = new ConsumeQueueOptions();
            configure?.Invoke(options);
            return Listen(queue, options, received);
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
        public ListenResult Listen(string exchange, string queue, ExchangeConsumeQueueOptions options = null, Action<RecieveResult> received = null)
        {
            if (string.IsNullOrEmpty(exchange))
                throw new ArgumentException("exchange cannot be empty", nameof(exchange));

            if (options == null)
                throw new ArgumentNullException("ExchangeConsumeQueueOptions cannot be empty", nameof(options));

            if (options.Type == RabbitExchangeType.None)
                throw new NotSupportedException($"{nameof(RabbitExchangeType)} must be specified");

            if (options == null)
                options = new ExchangeConsumeQueueOptions();

            var channel = GetChannel();

            PrepareExchangeChannel(channel, exchange, options);

            return ConsumeInternal(channel, queue, options.AutoAck, options.FetchCount, received);
        }
        /// <summary>
        /// 消费消息
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        /// <param name="configure"></param>
        /// <param name="received"></param>
        /// <returns></returns>
        public ListenResult Listen(string exchange, string queue, Action<ExchangeConsumeQueueOptions> configure, Action<RecieveResult> received = null)
        {
            var options = new ExchangeConsumeQueueOptions();
            configure?.Invoke(options);
            return Listen(exchange, queue, options, received);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rabbitBaseOptions"></param>
        /// <returns></returns>
        public static RabbitConsumer Create(RabbitBaseOptions rabbitBaseOptions)
        {
            var consumer = new RabbitConsumer(rabbitBaseOptions.Hosts)
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
