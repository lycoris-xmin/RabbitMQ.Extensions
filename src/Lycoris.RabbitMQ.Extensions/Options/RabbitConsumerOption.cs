using Lycoris.RabbitMQ.Extensions.Builder;
using Lycoris.RabbitMQ.Extensions.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lycoris.RabbitMQ.Extensions.Options
{
    /// <summary>
    /// 
    /// </summary>
    public class RabbitConsumerOption
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
                    Arguments = RabbitMQArguments.Delayed;
                else if (Arguments == null || !Arguments.Any())
                    Arguments = RabbitMQArguments.Default;
            }
        }

        /// <summary>
        /// 队列及路由值
        /// </summary>
        public RouteQueue[] RouteQueues { get; set; } = Array.Empty<RouteQueue>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue"></param>
        public void AddListener<T>(string queue) where T : class, IRabbitConsumerListener => AddListener<T>("", queue);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        public void AddListener<T>(string exchange, string queue) where T : class, IRabbitConsumerListener
        {
            ListenerMaps.Add(new RabbitConsumerListenerMap()
            {
                Exchange = exchange,
                Queue = queue,
                Listener = typeof(T)
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="onMessageRecieved"></param>
        public void AddListener(string queue, Action<RecieveResult> onMessageRecieved) => AddListener("", queue, onMessageRecieved);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        /// <param name="onMessageRecieved"></param>
        public void AddListener(string exchange, string queue, Action<RecieveResult> onMessageRecieved)
        {
            ListenerMaps.Add(new RabbitConsumerListenerMap()
            {
                Exchange = exchange,
                Queue = queue,
                MessageRecieved = onMessageRecieved
            });
        }

        internal List<RabbitConsumerListenerMap> ListenerMaps { get; set; } = new List<RabbitConsumerListenerMap>();

        /// <summary>
        /// 使用基础配置
        /// </summary>
        /// <param name="configureName"></param>
        internal virtual void UseRabbitOption(string configureName)
        {
            var rabbitOption = RabbitMQOptionsStore.GetRabbitMQOption(configureName);
            if (rabbitOption != null)
            {
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
