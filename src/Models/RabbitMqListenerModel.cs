namespace Lycoris.RabbitMQ.Extensions.Models
{
    /// <summary>
    /// RabbitMQ监听器
    /// </summary>
    public class RabbitMqListenerModel
    {
        /// <summary>
        /// 监听的节点名
        /// </summary>
        public string Node { get; set; }

        /// <summary>
        /// 使用的协议（如amqp、http等）
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// 监听的IP地址
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// 监听的端口号
        /// </summary>
        public int Port { get; set; }
    }
}
