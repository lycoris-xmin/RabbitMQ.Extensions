namespace Lycoris.RabbitMQ.Extensions.Options
{
    /// <summary>
    /// 消费者交换机与队列选项，继承自 <see cref="ExchangeQueueOptions"/>，增加了消息确认和拉取数量控制。
    /// </summary>
    public class ExchangeConsumeQueueOption : ExchangeQueueOptions
    {
        /// <summary>
        /// 是否自动确认消息，默认 false（需手动 Ack/Nack）。
        /// </summary>
        public bool AutoAck { get; set; } = false;

        /// <summary>
        /// 每次从队列拉取的消息数量上限。
        /// </summary>
        public ushort? FetchCount { get; set; }
    }
}
