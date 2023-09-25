using Lycoris.RabbitMQ.Extensions.Builder;
using System;
using System.Collections.Generic;

namespace Lycoris.RabbitMQ.Extensions.Options
{
    /// <summary>
    /// 
    /// </summary>
    public class RabbitProducerOption
    {
        /// <summary>
        /// 保留发布者数
        /// </summary>
        public int InitializeCount { get; set; } = 5;

        /// <summary>
        /// 队列
        /// </summary>
        public string[] Queues { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 交换机
        /// </summary>
        public string Exchange { get; set; } = string.Empty;

        /// <summary>
        /// 交换机类型
        /// </summary>
        public RabbitExchangeType Type { get; set; }

        /// <summary>
        /// 延迟队列的延迟时间(单位:秒，默认:1秒)
        /// </summary>
        public int DelayTime { get; set; } = 1;

        /// <summary>
        /// 队列及路由值
        /// </summary>
        public RouteQueue[] RouteQueues { get; set; } = Array.Empty<RouteQueue>();

        internal const string DelayPropsKey = "x-delay";

        /// <summary>
        /// 
        /// </summary>
        internal virtual void CheckDelayProps()
        {
            if (Type != RabbitExchangeType.Delayed && BasicProps != null && BasicProps.ContainsKey(DelayPropsKey))
                BasicProps.Remove(DelayPropsKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configureName"></param>
        internal virtual void UseRabbitOption(string configureName)
        {
            var rabbitOption = RabbitMQOptionsStore.GetRabbitMQOption(configureName) ?? throw new Exception($"cant not find the rabbitmq configuration with name:{configureName}");

            Hosts = rabbitOption.Hosts;
            Port = rabbitOption.Port;
            UserName = rabbitOption.UserName;
            Password = rabbitOption.Password;
            VirtualHost = rabbitOption.VirtualHost;
            Durable = rabbitOption.Durable;
            AutoDelete = rabbitOption.AutoDelete;
            Arguments = rabbitOption.Arguments ?? new Dictionary<string, object>();
            BasicProps = rabbitOption.BasicProps ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// 服务节点,可以是单独的hostname或者IP,也可以是host:port或者ip:port形式
        /// </summary>
        internal string[] Hosts { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 端口
        /// </summary>
        internal int Port { get; set; } = 5672;

        /// <summary>
        /// 账号
        /// </summary>
        internal string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        internal string Password { get; set; } = string.Empty;

        /// <summary>
        /// 虚拟机
        /// </summary>
        internal string VirtualHost { get; set; } = "/";

        /// <summary>
        /// 是否持久化
        /// </summary>
        internal bool Durable { get; set; } = true;

        /// <summary>
        /// 是否自动删除
        /// </summary>
        internal bool AutoDelete { get; set; } = false;

        /// <summary>
        /// 参数,例如：{ "x-queue-type", "classic" }
        /// </summary>
        internal Dictionary<string, object> Arguments { get; set; }

        /// <summary>
        /// 
        /// </summary>
        internal Dictionary<string, object> BasicProps { get; set; }
    }
}
