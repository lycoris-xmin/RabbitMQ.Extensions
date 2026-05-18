using Newtonsoft.Json;
using System.Collections.Generic;

namespace Lycoris.RabbitMQ.Extensions.Models
{
    /// <summary>
    /// RabbitMQ 消息统计
    /// </summary>
    public class RabbitMqMessageStatsModel
    {
        /// <summary>
        /// 从队列获取的消息数
        /// </summary>
        public int Get { get; set; }

        /// <summary>
        /// 消息被送达的次数
        /// </summary>
        public int Deliver { get; set; }

        /// <summary>
        /// 确认消息数
        /// </summary>
        public int Confirm { get; set; }

        /// <summary>
        /// 确认的消息数
        /// </summary>
        public int Ack { get; set; }

        /// <summary>
        /// 发布的消息数
        /// </summary>
        public int Publish { get; set; }

        /// <summary>
        /// 磁盘读取次数
        /// </summary>
        [JsonProperty("disk_reads")]
        public int DiskReads { get; set; }

        /// <summary>
        /// 磁盘写入次数
        /// </summary>
        [JsonProperty("disk_writes")]
        public int DiskWrites { get; set; }

        /// <summary>
        /// 空队列的获取次数
        /// </summary>
        [JsonProperty("get_empty")]
        public int GetEmpty { get; set; }

        /// <summary>
        /// 没有确认的获取次数
        /// </summary>
        [JsonProperty("get_no_ack")]
        public int GetNoAck { get; set; }

        /// <summary>
        /// 没有确认的交付次数
        /// </summary>
        [JsonProperty("deliver_no_ack")]
        public int DeliverNoAck { get; set; }

        /// <summary>
        /// 被重新投递的消息数
        /// </summary>
        public int Redeliver { get; set; }

        /// <summary>
        /// 丢弃无法路由的消息数
        /// </summary>
        [JsonProperty("drop_unroutable")]
        public int DropUnroutable { get; set; }

        /// <summary>
        /// 返回无法路由的消息数
        /// </summary>
        [JsonProperty("return_unroutable")]
        public int ReturnUnroutable { get; set; }

        /// <summary>
        /// 消息交付并获取的次数
        /// </summary>
        [JsonProperty("deliver_get")]
        public int DeliverGet { get; set; }

        /// <summary>
        /// 获取空队列的详细统计
        /// </summary>
        [JsonProperty("get_empty_details")]
        public Dictionary<string, double> GetEmptyDetails { get; set; }

        /// <summary>
        /// 消息交付并获取的详细统计
        /// </summary>
        [JsonProperty("deliver_get_details")]
        public Dictionary<string, double> DeliverGetDetails { get; set; }

        /// <summary>
        /// 消息确认的详细统计
        /// </summary>
        [JsonProperty("ack_details")]
        public Dictionary<string, double> AckDetails { get; set; }

        /// <summary>
        /// 重新投递的消息详细统计
        /// </summary>
        [JsonProperty("redeliver_details")]
        public Dictionary<string, double> RedeliverDetails { get; set; }

        /// <summary>
        /// 无确认的消息交付的详细统计
        /// </summary>
        [JsonProperty("deliver_no_ack_details")]
        public Dictionary<string, double> DeliverNoAckDetails { get; set; }

        /// <summary>
        /// 消息交付的详细统计
        /// </summary>
        [JsonProperty("deliver_details")]
        public Dictionary<string, double> DeliverDetails { get; set; }

        /// <summary>
        /// 没有确认的获取详细统计
        /// </summary>
        [JsonProperty("get_no_ack_details")]
        public Dictionary<string, double> GetNoAckDetails { get; set; }

        /// <summary>
        /// 获取消息的详细统计
        /// </summary>
        [JsonProperty("get_details")]
        public Dictionary<string, double> GetDetails { get; set; }

        /// <summary>
        /// 丢弃无法路由消息的详细统计
        /// </summary>
        [JsonProperty("drop_unroutable_details")]
        public Dictionary<string, double> DropUnroutableDetails { get; set; }

        /// <summary>
        /// 返回无法路由消息的详细统计
        /// </summary>
        [JsonProperty("return_unroutable_details")]
        public Dictionary<string, double> ReturnUnroutableDetails { get; set; }

        /// <summary>
        /// 消息确认的详细统计
        /// </summary>
        [JsonProperty("confirm_details")]
        public Dictionary<string, double> ConfirmDetails { get; set; }

        /// <summary>
        /// 消息发布的详细统计
        /// </summary>
        [JsonProperty("publish_details")]
        public Dictionary<string, double> PublishDetails { get; set; }

        /// <summary>
        /// 磁盘写入的详细统计
        /// </summary>
        [JsonProperty("disk_writes_details")]
        public Dictionary<string, double> DiskWritesDetails { get; set; }

        /// <summary>
        /// 磁盘读取的详细统计
        /// </summary>
        [JsonProperty("disk_reads_details")]
        public Dictionary<string, double> DiskReadsDetails { get; set; }
    }
}
