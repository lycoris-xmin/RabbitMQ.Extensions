using Lycoris.RabbitMQ.Extensions.Base;
using Lycoris.RabbitMQ.Extensions.DataModel;
using Lycoris.RabbitMQ.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RabbitProducer : BaseRabbit
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostAndPorts"></param>
        public RabbitProducer(params string[] hostAndPorts) : base(hostAndPorts)
        {

        }

        #region 普通模式、Work模式
        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="message"></param>
        /// <param name="options"></param>
        public Task PublishAsync(string queue, string message, QueueOption options = null)
            => PublishAsync(queue, new string[] { message }, options);

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="messages"></param>
        /// <param name="options"></param>
        public async Task PublishAsync(string queue, string[] messages, QueueOption options = null)
        {
            if (string.IsNullOrEmpty(queue))
                throw new ArgumentException("queue cannot be empty", nameof(queue));

            if (options == null)
                options = new QueueOption();

            var channel = await GetChannelAsync();

            await PrepareQueueChannelAsync(channel, queue, options);

            foreach (var message in messages)
            {
                var buffer = Encoding.UTF8.GetBytes(message);

                await channel.BasicPublishAsync("", queue, false, new BasicProperties(), buffer);
            }

            await channel.CloseAsync();
        }
        #endregion

        #region 订阅模式、路由模式、Topic模式
        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="routingKey"></param>
        /// <param name="message"></param>
        /// <param name="options"></param>
        public Task PublishAsync(string exchange, string routingKey, string message, ExchangeQueueOptions options = null)
            => PublishAsync(exchange, new RouteMessage() { Message = message, RoutingKey = routingKey }, options);

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="routingKey"></param>
        /// <param name="messages"></param>
        /// <param name="options"></param>
        public Task PublishAsync(string exchange, string routingKey, string[] messages, ExchangeQueueOptions options = null)
            => PublishAsync(exchange, messages.Select(message => new RouteMessage() { Message = message, RoutingKey = routingKey }).ToArray(), options);

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="routeMessage"></param>
        /// <param name="options"></param>
        public Task PublishAsync(string exchange, RouteMessage routeMessage, ExchangeQueueOptions options = null)
            => PublishAsync(exchange, new RouteMessage[] { routeMessage }, options);

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="routeMessages"></param>
        /// <param name="options"></param>
        public async Task PublishAsync(string exchange, RouteMessage[] routeMessages, ExchangeQueueOptions options = null)
        {
            if (string.IsNullOrEmpty(exchange))
                throw new ArgumentException("exchange cannot be empty", nameof(exchange));

            if (options == null)
                options = new ExchangeQueueOptions();

            if (options.Type == RabbitExchangeType.None)
                throw new NotSupportedException($"{nameof(RabbitExchangeType)} must be specified");

            var channel = await GetChannelAsync();

            await PrepareExchangeChannelAsync(channel, exchange, options);

            foreach (var routeMessage in routeMessages)
            {
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

                await channel.BasicPublishAsync(exchange, routeMessage.RoutingKey, true, props, buffer);
            }

            await channel.CloseAsync();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rabbitBaseOptions"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            CloseAsync().GetAwaiter().GetResult();
        }
    }
}
