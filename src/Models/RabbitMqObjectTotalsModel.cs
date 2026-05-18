namespace Lycoris.RabbitMQ.Extensions.Models
{
    /// <summary>
    /// RabbitMQ对象统计信息
    /// </summary>
    public class RabbitMqObjectTotalsModel
    {
        /// <summary>
        /// 当前通道数
        /// </summary>
        public int Channels { get; set; }

        /// <summary>
        /// 当前消费者数
        /// </summary>
        public int Consumers { get; set; }

        /// <summary>
        /// 当前交换机数
        /// </summary>
        public int Exchanges { get; set; }

        /// <summary>
        /// 当前队列数
        /// </summary>
        public int Queues { get; set; }

        /// <summary>
        /// 当前连接数
        /// </summary>
        public int Connections { get; set; }
    }
}
