using System;
using System.Collections.Generic;

namespace Lycoris.RabbitMQ.Extensions.Options
{
    /// <summary>
    /// RabbitMQ 全局配置选项，包含连接参数和队列/交换机的默认参数。
    /// </summary>
    public class RabbitOption : RabbitBaseOption
    {
        /// <summary>
        /// 队列和交换机是否持久化，默认 true。
        /// </summary>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// 队列和交换机是否自动删除，默认 false。
        /// </summary>
        public bool AutoDelete { get; set; } = false;

        /// <summary>
        /// 自定义参数字典，例如 { "x-queue-type", "classic" }。
        /// </summary>
        public Dictionary<string, object> Arguments { get; set; }

        /// <summary>
        /// 消息基础属性，将作为消息 Headers 发送。
        /// </summary>
        public Dictionary<string, object> BasicProps { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal void CheckBaseOptions()
        {
            if (this.Hosts == null || this.Hosts.Length == 0)
                throw new ArgumentNullException(nameof(this.Hosts));

            if (this.Port <= 0)
                throw new ArgumentOutOfRangeException(nameof(this.Port));

            if (string.IsNullOrEmpty(this.UserName))
                throw new ArgumentNullException(nameof(this.UserName));

            if (string.IsNullOrEmpty(this.Password))
                throw new ArgumentNullException(nameof(this.Password));

            if (string.IsNullOrEmpty(this.VirtualHost))
                throw new ArgumentNullException(nameof(this.VirtualHost));
        }
    }
}
