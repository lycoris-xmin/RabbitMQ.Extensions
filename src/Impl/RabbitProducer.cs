using Lycoris.RabbitMQ.Extensions.Base;
using Lycoris.RabbitMQ.Extensions.DataModel;
using Lycoris.RabbitMQ.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// RabbitMQ 生产者，封装消息发布逻辑，支持普通队列模式和交换机模式，内部缓存 Channel 以提高性能
    /// </summary>
    public sealed class RabbitProducer : BaseRabbit
    {
        private IChannel _cachedChannel;

        /// <summary>
        /// 使用指定主机列表初始化生产者
        /// </summary>
        /// <param name="hostAndPorts">主机地址列表，格式为 "host:port" 或 "host"</param>
        public RabbitProducer(params string[] hostAndPorts) : base(hostAndPorts)
        {

        }

        /// <summary>
        /// 获取或创建复用的 Channel，已关闭的 Channel 会自动重建
        /// </summary>
        private async Task<IChannel> GetOrCreateChannelAsync()
        {
            if (_cachedChannel == null || !_cachedChannel.IsOpen)
            {
                _cachedChannel?.Dispose();
                _cachedChannel = await GetChannelAsync();
            }
            return _cachedChannel;
        }

        #region 普通模式、Work模式
        /// <summary>
        /// 向普通队列发送单条消息
        /// </summary>
        /// <param name="queue">队列名称</param>
        /// <param name="message">消息内容</param>
        /// <param name="options">队列选项</param>
        /// <param name="basicProperties">消息属性（Headers、CorrelationId、ReplyTo等）</param>
        /// <param name="cancellationToken">取消令牌</param>
        public Task PublishAsync(string queue, string message, QueueOption options = null, BasicProperties basicProperties = null, CancellationToken cancellationToken = default)
            => PublishAsync(queue, new string[] { message }, options, basicProperties, cancellationToken);

        /// <summary>
        /// 向普通队列发送多条消息
        /// </summary>
        /// <param name="queue">队列名称</param>
        /// <param name="messages">消息内容数组</param>
        /// <param name="options">队列选项</param>
        /// <param name="basicProperties">消息属性（Headers、CorrelationId、ReplyTo等）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <exception cref="ArgumentException">队列名称为空时抛出</exception>
        public async Task PublishAsync(string queue, string[] messages, QueueOption options = null, BasicProperties basicProperties = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(queue))
                throw new ArgumentException("queue cannot be empty", nameof(queue));

            if (options == null)
                options = new QueueOption();

            var channel = await GetOrCreateChannelAsync();

            await PrepareQueueChannelAsync(channel, queue, options);

            foreach (var message in messages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var buffer = Encoding.UTF8.GetBytes(message);

                var props = basicProperties ?? new BasicProperties();

                await channel.BasicPublishAsync("", queue, false, props, buffer, cancellationToken);
            }
        }
        #endregion

        #region 订阅模式、路由模式、Topic模式
        /// <summary>
        /// 通过交换机发送单条消息
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="routingKey">路由键</param>
        /// <param name="message">消息内容</param>
        /// <param name="options">交换机队列选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        public Task PublishAsync(string exchange, string routingKey, string message, ExchangeQueueOptions options = null, CancellationToken cancellationToken = default)
            => PublishAsync(exchange, new RouteMessage() { Message = message, RoutingKey = routingKey }, options, cancellationToken);

        /// <summary>
        /// 通过交换机发送多条消息（共享同一路由键）
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="routingKey">路由键</param>
        /// <param name="messages">消息内容数组</param>
        /// <param name="options">交换机队列选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        public Task PublishAsync(string exchange, string routingKey, string[] messages, ExchangeQueueOptions options = null, CancellationToken cancellationToken = default)
            => PublishAsync(exchange, messages.Select(message => new RouteMessage() { Message = message, RoutingKey = routingKey }).ToArray(), options, cancellationToken);

        /// <summary>
        /// 通过交换机发送单条路由消息
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="routeMessage">路由消息，包含消息体和路由键</param>
        /// <param name="options">交换机队列选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        public Task PublishAsync(string exchange, RouteMessage routeMessage, ExchangeQueueOptions options = null, CancellationToken cancellationToken = default)
            => PublishAsync(exchange, new RouteMessage[] { routeMessage }, options, cancellationToken);

        /// <summary>
        /// 通过交换机发送多条路由消息
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="routeMessages">路由消息数组，每条消息可具有独立的路由键</param>
        /// <param name="options">交换机队列选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <exception cref="ArgumentException">交换机名称为空时抛出</exception>
        /// <exception cref="NotSupportedException">未指定交换机类型时抛出</exception>
        public async Task PublishAsync(string exchange, RouteMessage[] routeMessages, ExchangeQueueOptions options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(exchange))
                throw new ArgumentException("exchange cannot be empty", nameof(exchange));

            if (options == null)
                options = new ExchangeQueueOptions();

            if (options.Type == RabbitExchangeType.None)
                throw new NotSupportedException($"{nameof(RabbitExchangeType)} must be specified");

            var channel = await GetOrCreateChannelAsync();

            await PrepareExchangeChannelAsync(channel, exchange, options);

            foreach (var routeMessage in routeMessages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var buffer = Encoding.UTF8.GetBytes(routeMessage.Message);

                var props = new BasicProperties();
                if (options.BasicProps != null && options.BasicProps.Count > 0)
                {
                    if (props.Headers == null)
                        props.Headers = new Dictionary<string, object>();

                    foreach (var item in options.BasicProps)
                    {
                        props.Headers.Add(item.Key, item.Value);
                    }
                }

                props.MessageId = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

                await channel.BasicPublishAsync(exchange, routeMessage.RoutingKey, true, props, buffer, cancellationToken);
            }
        }
        #endregion

        /// <summary>
        /// 根据配置创建生产者实例
        /// </summary>
        /// <param name="rabbitBaseOptions">生产者配置选项</param>
        /// <returns>配置完成的 <see cref="RabbitProducer"/> 实例</returns>
        public static RabbitProducer Create(RabbitProducerOption rabbitBaseOptions)
        {
            var producer = new RabbitProducer(rabbitBaseOptions.Hosts)
            {
                Password = rabbitBaseOptions.Password,
                Port = rabbitBaseOptions.Port,
                UserName = rabbitBaseOptions.UserName,
                VirtualHost = rabbitBaseOptions.VirtualHost
            };

            return producer;
        }

        /// <summary>
        /// 释放缓存的 Channel 和连接资源（同步）
        /// </summary>
        public override void Dispose()
        {
            _cachedChannel?.CloseAsync().GetAwaiter().GetResult();
            _cachedChannel?.Dispose();
            _cachedChannel = null;
            base.Dispose();
        }

        /// <summary>
        /// 异步释放缓存的 Channel 和连接资源
        /// </summary>
        public override async ValueTask DisposeAsync()
        {
            if (_cachedChannel != null)
            {
                await _cachedChannel.CloseAsync();
                _cachedChannel.Dispose();
                _cachedChannel = null;
            }
            await base.DisposeAsync();
        }
    }
}
