using System.Collections.Generic;

namespace Lycoris.RabbitMQ.Extensions.Options
{
    /// <summary>
    /// 队列声明选项，用于配置队列的持久化、自动删除及死信交换机等参数。
    /// </summary>
    public class QueueOption
    {
        /// <summary>
        /// 队列是否持久化，默认 true。
        /// </summary>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// 队列是否自动删除，默认 true。
        /// </summary>
        public bool AutoDelete { get; set; } = true;

        /// <summary>
        /// 自定义参数字典，例如 { "x-queue-type", "classic" }。
        /// </summary>
        public IDictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// 死信交换机配置。
        /// </summary>
        public DeadLetterExchangeOption DeadLetterExchange { get; set; }
    }
}
