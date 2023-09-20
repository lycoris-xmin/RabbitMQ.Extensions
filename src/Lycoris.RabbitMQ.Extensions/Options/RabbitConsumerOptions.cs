using Lycoris.RabbitMQ.Extensions.Builder;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lycoris.RabbitMQ.Extensions.Options
{
    /// <summary>
    /// 
    /// </summary>
    public class RabbitConsumerOptions : RabbitOptions
    {
        /// <summary>
        /// 是否自动提交
        /// </summary>
        public bool AutoAck { get; set; } = false;

        /// <summary>
        /// 每次发送消息条数
        /// </summary>
        public ushort? FetchCount { get; set; } = 2;

        /// <summary>
        /// 
        /// </summary>
        private RabbitExchangeType _Type = RabbitExchangeType.None;
        /// <summary>
        /// 交换机类型
        /// </summary>
        public RabbitExchangeType Type
        {
            get => _Type;
            set
            {
                _Type = value;
                if (_Type == RabbitExchangeType.Delayed)
                    this.Arguments = RabbitMQArguments.Delayed;
                else if (this.Arguments == null || !this.Arguments.Any())
                    this.Arguments = RabbitMQArguments.Default;
            }
        }

        /// <summary>
        /// 队列及路由值
        /// </summary>
        public RouteQueue[] RouteQueues { get; set; } = Array.Empty<RouteQueue>();

        /// <summary>
        /// 使用基础配置
        /// </summary>
        /// <param name="configureName"></param>
        public virtual void UseRabbitOption(string configureName)
        {
            var rabbitOption = RabbitMQOptionsStore.GetRabbitMQOption(configureName);
            if (rabbitOption != null)
            {
                this.Hosts = rabbitOption.Hosts;
                this.Port = rabbitOption.Port;
                this.UserName = rabbitOption.UserName;
                this.Password = rabbitOption.Password;
                this.VirtualHost = rabbitOption.VirtualHost;
                this.Durable = rabbitOption.Durable;
                this.AutoDelete = rabbitOption.AutoDelete;
                this.Arguments = rabbitOption.Arguments ?? new Dictionary<string, object>();
                this.BasicProps = rabbitOption.BasicProps ?? new Dictionary<string, object>();
            }
        }
    }
}
