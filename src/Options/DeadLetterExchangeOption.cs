namespace Lycoris.RabbitMQ.Extensions.Options
{
    /// <summary>
    /// 死信交换机配置
    /// </summary>
    public class DeadLetterExchangeOption
    {
        /// <summary>
        /// 死信交换机名称
        /// </summary>
        public string Exchange { get; set; } = string.Empty;

        /// <summary>
        /// 死信路由键（不设置则使用原消息的路由键）
        /// </summary>
        public string RoutingKey { get; set; } = string.Empty;
    }
}
