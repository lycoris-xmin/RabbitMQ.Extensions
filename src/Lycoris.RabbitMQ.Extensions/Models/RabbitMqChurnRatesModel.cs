using Newtonsoft.Json;
using System.Collections.Generic;

namespace Lycoris.RabbitMQ.Extensions.Models
{
    /// <summary>
    /// RabbitMQ的连接、通道、队列的创建和关闭速率
    /// </summary>
    public class RabbitMqChurnRatesModel
    {
        /// <summary>
        ///  关闭的连接数
        /// </summary>
        [JsonProperty("connection_closed")]
        public int ConnectionClosed { get; set; }

        /// <summary>
        /// 声明的队列数
        /// </summary>
        [JsonProperty("queue_declared")]
        public int QueueDeclared { get; set; }

        /// <summary>
        /// 创建的队列数
        /// </summary>
        [JsonProperty("queue_created")]
        public int QueueCreated { get; set; }

        /// <summary>
        /// 创建的连接数
        /// </summary>
        [JsonProperty("connection_created")]
        public int ConnectionCreated { get; set; }

        /// <summary>
        /// 删除的队列数
        /// </summary>
        [JsonProperty("queue_deleted")]
        public int QueueDeleted { get; set; }

        /// <summary>
        /// 创建的通道数
        /// </summary>
        [JsonProperty("channel_created")]
        public int ChannelCreated { get; set; }

        /// <summary>
        /// 关闭的通道数
        /// </summary>
        [JsonProperty("channel_closed")]
        public int ChannelClosed { get; set; }

        /// <summary>
        /// 删除队列的详细统计
        /// </summary>
        [JsonProperty("queue_deleted_details")]
        public Dictionary<string, double> QueueDeletedDetails { get; set; }

        /// <summary>
        /// 创建队列的详细统计
        /// </summary>
        [JsonProperty("queue_created_details")]
        public Dictionary<string, double> QueueCreatedDetails { get; set; }

        /// <summary>
        /// 声明队列的详细统计
        /// </summary>
        [JsonProperty("queue_declared_details")]
        public Dictionary<string, double> QueueDeclaredDetails { get; set; }

        /// <summary>
        /// 关闭通道的详细统计
        /// </summary>
        [JsonProperty("channel_closed_details")]
        public Dictionary<string, double> ChannelClosedDetails { get; set; }

        /// <summary>
        /// 创建通道的详细统计
        /// </summary>
        [JsonProperty("channel_created_details")]
        public Dictionary<string, double> ChannelCreatedDetails { get; set; }

        /// <summary>
        /// 关闭连接的详细统计关闭连接的详细统计
        /// </summary>
        [JsonProperty("connection_closed_details")]
        public Dictionary<string, double> ConnectionClosedDetails { get; set; }

        /// <summary>
        /// 创建连接的详细统计
        /// </summary>
        [JsonProperty("connection_created_details")]
        public Dictionary<string, double> ConnectionCreatedDetails { get; set; }
    }
}
