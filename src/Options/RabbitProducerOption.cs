using Lycoris.RabbitMQ.Extensions.Builder;
using System;
using System.Collections.Generic;

namespace Lycoris.RabbitMQ.Extensions.Options
{
    /// <summary>
    /// 生产者配置选项，用于配置消息生产者的连接、队列/交换机绑定、延迟及死信交换机等参数。
    /// </summary>
    public class RabbitProducerOption
    {
        /// <summary>
        /// 保留的生产者实例数量，默认 5。
        /// </summary>
        public int InitializeCount { get; set; } = 5;

        /// <summary>
        /// 目标队列名称列表（普通模式）。
        /// </summary>
        public string[] Queues { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 目标交换机名称（交换机模式）。
        /// </summary>
        public string Exchange { get; set; } = string.Empty;

        /// <summary>
        /// 交换机类型。
        /// </summary>
        public RabbitExchangeType Type { get; set; }

        /// <summary>
        /// 延迟队列的消息延迟时间（单位：秒），默认 1 秒。
        /// </summary>
        public int DelayTime { get; set; } = 1;

        /// <summary>
        /// 路由队列绑定列表。
        /// </summary>
        public RouteQueue[] RouteQueues { get; set; } = Array.Empty<RouteQueue>();

        /// <summary>
        /// 死信交换机配置（全局，将传播到所有路由队列）。
        /// </summary>
        public DeadLetterExchangeOption DeadLetterExchange { get; set; }

        /// <summary>
        /// 注册生产者服务，服务类型既是接口也是实现。
        /// </summary>
        /// <typeparam name="T">实现 <see cref="BaseRabbitProducerService"/> 的生产者服务类型。</typeparam>
        /// <returns>当前 <see cref="RabbitProducerOption"/> 实例，支持链式调用。</returns>
        public RabbitProducerOption AddRabbitProducer<T>() where T : BaseRabbitProducerService
        {
            this.RabbitProducer.Add(typeof(T), typeof(T));
            return this;
        }

        /// <summary>
        /// 注册生产者服务，分别指定接口和实现类型。
        /// </summary>
        /// <typeparam name="TService">生产者服务接口类型。</typeparam>
        /// <typeparam name="TImplementation">生产者服务实现类型，需继承 <see cref="BaseRabbitProducerService"/> 并实现 <typeparamref name="TService"/>。</typeparam>
        /// <returns>当前 <see cref="RabbitProducerOption"/> 实例，支持链式调用。</returns>
        public RabbitProducerOption AddRabbitProducer<TService, TImplementation>() where TService : class where TImplementation : BaseRabbitProducerService, TService
        {
            this.RabbitProducer.Add(typeof(TService), typeof(TImplementation));
            return this;
        }

        internal const string DelayPropsKey = "x-delay";

        /// <summary>
        /// 已注册的生产者服务映射表（接口类型 -> 实现类型）。
        /// </summary>
        internal Dictionary<Type, Type> RabbitProducer { get; set; } = new Dictionary<Type, Type>();

        /// <summary>
        /// 检查并清理延迟队列相关属性：若非延迟交换机类型，则移除 BasicProps 中的 x-delay 键。
        /// </summary>
        internal virtual void CheckDelayProps()
        {
            if (Type != RabbitExchangeType.Delayed && BasicProps != null && BasicProps.ContainsKey(DelayPropsKey))
                BasicProps.Remove(DelayPropsKey);
        }

        /// <summary>
        /// 应用指定名称的全局 RabbitMQ 配置到此生产者选项。
        /// </summary>
        /// <param name="configureName">配置名称。</param>
        internal virtual void UseRabbitOption(string configureName)
        {
            var rabbitOption = RabbitMQOptionsStore.GetRabbitMQOption(configureName) ?? throw new Exception($"cant not found the rabbitmq configuration with name:{configureName}");

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
        /// 自定义参数字典，例如 { "x-queue-type", "classic" }。
        /// </summary>
        public Dictionary<string, object> Arguments { get; set; }

        /// <summary>
        /// 消息基础属性，将作为消息 Headers 发送。
        /// </summary>
        public Dictionary<string, object> BasicProps { get; set; }
    }
}
