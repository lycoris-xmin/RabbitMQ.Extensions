using Lycoris.RabbitMQ.Extensions.Base;
using Lycoris.RabbitMQ.Extensions.DataModel;
using Lycoris.RabbitMQ.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public void Publish(string queue, string message, QueueOption options = null)
            => Publish(queue, new string[] { message }, options);

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="messages"></param>
        /// <param name="options"></param>
        public void Publish(string queue, string[] messages, QueueOption options = null)
        {
            if (string.IsNullOrEmpty(queue))
                throw new ArgumentException("queue cannot be empty", nameof(queue));

            if (options == null)
                options = new QueueOption();

            var channel = GetChannel();

            PrepareQueueChannel(channel, queue, options);

            foreach (var message in messages)
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish("", queue, null, buffer);
            }
            channel.Close();
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
        public void Publish(string exchange, string routingKey, string message, ExchangeQueueOptions options = null)
            => Publish(exchange, new RouteMessage() { Message = message, RoutingKey = routingKey }, options);

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="routingKey"></param>
        /// <param name="messages"></param>
        /// <param name="options"></param>
        public void Publish(string exchange, string routingKey, string[] messages, ExchangeQueueOptions options = null)
            => Publish(exchange, messages.Select(message => new RouteMessage() { Message = message, RoutingKey = routingKey }).ToArray(), options);

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="routeMessage"></param>
        /// <param name="options"></param>
        public void Publish(string exchange, RouteMessage routeMessage, ExchangeQueueOptions options = null)
            => Publish(exchange, new RouteMessage[] { routeMessage }, options);

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="routeMessages"></param>
        /// <param name="options"></param>
        public void Publish(string exchange, RouteMessage[] routeMessages, ExchangeQueueOptions options = null)
        {
            if (string.IsNullOrEmpty(exchange))
                throw new ArgumentException("exchange cannot be empty", nameof(exchange));

            if (options == null)
                options = new ExchangeQueueOptions();

            if (options.Type == RabbitExchangeType.None)
            {
                throw new NotSupportedException($"{nameof(RabbitExchangeType)} must be specified");
            }

            var channel = GetChannel();

            PrepareExchangeChannel(channel, exchange, options);

            foreach (var routeMessage in routeMessages)
            {
                var buffer = Encoding.UTF8.GetBytes(routeMessage.Message);

                IBasicProperties props = null;
                if (options.BasicProps != null && options.BasicProps.Count > 0)
                {
                    props = channel.CreateBasicProperties();

                    props.Persistent = true;

                    if (props.Headers == null)
                        props.Headers = new Dictionary<string, object>();

                    foreach (var item in options.BasicProps)
                    {
                        props.Headers.Add(item.Key, item.Value);
                    }
                }

                channel.BasicPublish(exchange, routeMessage.RoutingKey, props, buffer);
            }

            channel.Close();
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
            Close();
        }
    }
}
