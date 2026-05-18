using Lycoris.RabbitMQ.Extensions.Builder;
using Lycoris.RabbitMQ.Extensions.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Options
{
    /// <summary>
    /// 消费者配置选项，用于配置消费者的连接、队列/交换机绑定、消息确认及死信交换机等参数，并注册消费者监听器。
    /// </summary>
    public class RabbitConsumerOption
    {
        /// <summary>
        /// 是否自动确认消息，默认 false（需手动 Ack/Nack）。
        /// </summary>
        public bool AutoAck { get; set; } = false;

        /// <summary>
        /// 每次从队列拉取的消息数量上限，默认 2。
        /// </summary>
        public ushort? FetchCount { get; set; } = 2;

        /// <summary>
        /// 交换机类型（私有字段，用于属性内部逻辑）。
        /// </summary>
        private RabbitExchangeType _Type = RabbitExchangeType.None;

        /// <summary>
        /// 交换机类型。设为 <see cref="RabbitExchangeType.Delayed"/> 时将自动配置延迟队列参数。
        /// </summary>
        public RabbitExchangeType Type
        {
            get => _Type;
            set
            {
                _Type = value;
                if (Arguments == null)
                    Arguments = new Dictionary<string, object>();

                if (_Type == RabbitExchangeType.Delayed)
                {
                    foreach (var item in RabbitMQArguments.Delayed)
                    {
                        if (!Arguments.ContainsKey(item.Key))
                            Arguments[item.Key] = item.Value;
                    }
                }
                else
                {
                    Arguments.Remove("x-delayed-type");
                    if (!Arguments.Any())
                    {
                        foreach (var item in RabbitMQArguments.Default)
                        {
                            Arguments[item.Key] = item.Value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 路由队列绑定列表。
        /// </summary>
        public RouteQueue[] RouteQueues { get; set; } = Array.Empty<RouteQueue>();

        /// <summary>
        /// 死信交换机配置（全局，会传播到所有路由队列）
        /// </summary>
        public DeadLetterExchangeOption DeadLetterExchange { get; set; }

        /// <summary>
        /// 添加指定队列的消费者监听器。
        /// </summary>
        /// <typeparam name="T">实现 <see cref="IRabbitConsumerListener"/> 的消费者监听器类型。</typeparam>
        /// <param name="queue">队列名称。</param>
        public void AddConsumer<T>(string queue) where T : class, IRabbitConsumerListener => AddConsumer<T>("", queue);

        /// <summary>
        /// 添加指定交换机和队列的消费者监听器。
        /// </summary>
        /// <typeparam name="T">实现 <see cref="IRabbitConsumerListener"/> 的消费者监听器类型。</typeparam>
        /// <param name="exchange">交换机名称。</param>
        /// <param name="queue">队列名称。</param>
        public void AddConsumer<T>(string exchange, string queue) where T : class, IRabbitConsumerListener
        {
            ConsumerMaps.Add(new RabbitConsumerMap()
            {
                Exchange = exchange,
                Queue = queue,
                Listener = typeof(T)
            });
        }

        /// <summary>
        /// 添加指定键名的键控队列消费者监听器。
        /// </summary>
        /// <typeparam name="T">实现 <see cref="IRabbitConsumerListener"/> 的消费者监听器类型。</typeparam>
        /// <param name="keyName">键名，用于标识该键控消费者。</param>
        /// <param name="queue">队列名称。</param>
        public void AddKeyConsumer<T>(string keyName, string queue) where T : class, IRabbitConsumerListener => AddKeyConsumer<T>(keyName, "", queue);

        /// <summary>
        /// 添加指定键名的键控交换机消费者监听器。
        /// </summary>
        /// <typeparam name="T">实现 <see cref="IRabbitConsumerListener"/> 的消费者监听器类型。</typeparam>
        /// <param name="keyName">键名，用于标识该键控消费者。</param>
        /// <param name="exchange">交换机名称。</param>
        /// <param name="queue">队列名称。</param>
        public void AddKeyConsumer<T>(string keyName, string exchange, string queue) where T : class, IRabbitConsumerListener
        {
            ConsumerMaps.Add(new RabbitConsumerMap()
            {
                Exchange = exchange,
                Queue = queue,
                Listener = typeof(T),
                KeyConsumer = true,
                KeyName = keyName
            });
        }

        /// <summary>
        /// 添加自动生成键名的默认键控队列消费者监听器。
        /// </summary>
        /// <typeparam name="T">实现 <see cref="IRabbitConsumerListener"/> 的消费者监听器类型。</typeparam>
        /// <param name="queue">队列名称。</param>
        public void AddDefaultKeyConsumer<T>(string queue) where T : class, IRabbitConsumerListener => AddDefaultKeyConsumer<T>("", queue);

        /// <summary>
        /// 添加自动生成键名的默认键控交换机消费者监听器。
        /// </summary>
        /// <typeparam name="T">实现 <see cref="IRabbitConsumerListener"/> 的消费者监听器类型。</typeparam>
        /// <param name="exchange">交换机名称。</param>
        /// <param name="queue">队列名称。</param>
        public void AddDefaultKeyConsumer<T>(string exchange, string queue) where T : class, IRabbitConsumerListener
        {
            ConsumerMaps.Add(new RabbitConsumerMap()
            {
                Exchange = exchange,
                Queue = queue,
                Listener = typeof(T),
                KeyConsumer = true
            });
        }

        /// <summary>
        /// 添加队列监听回调函数。
        /// </summary>
        /// <param name="queue">队列名称。</param>
        /// <param name="onMessageRecieved">接收到消息时的异步回调函数。</param>
        public void AddConsumer(string queue, Func<RecieveResult, Task> onMessageRecieved) => AddConsumer("", queue, onMessageRecieved);

        /// <summary>
        /// 添加交换机监听回调函数。
        /// </summary>
        /// <param name="exchange">交换机名称。</param>
        /// <param name="queue">队列名称。</param>
        /// <param name="onMessageRecieved">接收到消息时的异步回调函数。</param>
        public void AddConsumer(string exchange, string queue, Func<RecieveResult, Task> onMessageRecieved)
        {
            ConsumerMaps.Add(new RabbitConsumerMap()
            {
                Exchange = exchange,
                Queue = queue,
                MessageRecieved = onMessageRecieved
            });
        }

        /// <summary>
        /// 已注册的消费者映射列表。
        /// </summary>
        internal List<RabbitConsumerMap> ConsumerMaps { get; set; } = new List<RabbitConsumerMap>();

        /// <summary>
        /// 应用指定名称的全局 RabbitMQ 配置到此消费者选项。
        /// </summary>
        /// <param name="configureName">配置名称。</param>
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
        public Dictionary<string, object> Arguments { get; set; }

        /// <summary>
        /// 消息基础属性，将作为消息 Headers 发送。
        /// </summary>
        public Dictionary<string, object> BasicProps { get; set; }
    }
}
