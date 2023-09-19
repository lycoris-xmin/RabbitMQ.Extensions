using Lycoris.RabbitMQ.Extensions.Builder;

namespace Lycoris.RabbitMQ.Extensions.Options
{
    /// <summary>
    /// 
    /// </summary>
    public class RabbitProducerOptions : RabbitOptions
    {
        internal const string DelayPropsKey = "x-delay";

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
        public virtual void UseRabbitOption(string configureName)
        {
            var rabbitOption = RabbitMQOptionsStore.GetRabbitMQOption(configureName);
            if (rabbitOption == null)
                throw new Exception($"cant not find the rabbitmq configuration with name:{configureName}");


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
