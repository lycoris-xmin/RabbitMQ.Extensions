using System;
using System.Collections.Generic;

namespace Lycoris.RabbitMQ.Extensions.Options
{
    /// <summary>
    /// 交换机与队列绑定配置选项，用于定义交换机类型、路由队列及消息属性。
    /// </summary>
    public class ExchangeQueueOptions
    {
        /// <summary>
        /// 交换机和队列是否持久化，默认 true。
        /// </summary>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// 交换机和队列是否自动删除，默认 false。
        /// </summary>
        public bool AutoDelete { get; set; } = false;

        /// <summary>
        /// 自定义参数字典，例如 { "x-queue-type", "classic" }。
        /// </summary>
        public IDictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// 交换机类型。
        /// </summary>
        public RabbitExchangeType Type { get; set; } = RabbitExchangeType.None;

        /// <summary>
        /// 绑定的路由队列列表。
        /// </summary>
        public RouteQueue[] RouteQueues { get; set; } = Array.Empty<RouteQueue>();

        /// <summary>
        /// 消息基础属性，将作为消息 Headers 发送。
        /// </summary>
        public Dictionary<string, object> BasicProps { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// 死信交换机配置。
        /// </summary>
        public DeadLetterExchangeOption DeadLetterExchange { get; set; }
    }
}
