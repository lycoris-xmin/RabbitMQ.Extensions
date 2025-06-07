using Newtonsoft.Json;
using System.Collections.Generic;

namespace Lycoris.RabbitMQ.Extensions.Models
{
    /// <summary>
    /// RabbitMQ队列统计信息
    /// </summary>
    public class RabbitMqQueueTotalsModel
    {
        /// <summary>
        /// 队列中的消息总数
        /// </summary>
        public int Messages { get; set; }

        /// <summary>
        /// 准备好的消息数（可以消费的）
        /// </summary>
        [JsonProperty("messages_ready")]
        public int MessagesReady { get; set; }

        /// <summary>
        /// 未确认的消息数
        /// </summary>
        [JsonProperty("messages_unacknowledged")]
        public int MessagesUnacknowledged { get; set; }

        /// <summary>
        /// 消息总数的详细统计
        /// </summary>
        [JsonProperty("messages_details")]
        public Dictionary<string, double> MessagesDetails { get; set; }

        /// <summary>
        /// 未确认消息的详细统计
        /// </summary>
        [JsonProperty("messages_unacknowledged_details")]
        public Dictionary<string, double> MessagesUnacknowledgedDetails { get; set; }

        /// <summary>
        /// 准备消息的详细统计
        /// </summary>
        [JsonProperty("messages_ready_details")]
        public Dictionary<string, double> MessagesReadyDetails { get; set; }
    }
}
