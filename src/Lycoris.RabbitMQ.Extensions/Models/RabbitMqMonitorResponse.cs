using Newtonsoft.Json;
using System.Collections.Generic;

namespace Lycoris.RabbitMQ.Extensions.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class RabbitMqMonitorResponse
    {
        /// <summary>
        /// 响应源数据
        /// </summary>
        [JsonIgnore]
        public string Body { get; set; }

        /// <summary>
        /// RabbitMQ管理界面版本
        /// </summary>
        [JsonProperty("management_version")]
        public string ManagementVersion { get; set; }

        /// <summary>
        /// Rate模式，可能的值包括 "basic"、"detailed" 等
        /// </summary>
        [JsonProperty("rates_mode")]
        public string RatesMode { get; set; }

        /// <summary>
        /// 样本保留策略：不同的策略会影响数据存储的时间 'global'、'basic'、'detailed' 是策略的名称
        /// </summary>
        [JsonProperty("sample_retention_policies")]
        public Dictionary<string, List<int>> SampleRetentionPolicies { get; set; }

        /// <summary>
        /// RabbitMQ支持的消息交换类型列表，描述了不同的交换类型
        /// </summary>
        [JsonProperty("exchange_types")]
        public List<RabbitMqExchangeTypeModel> ExchangeTypes { get; set; }

        /// <summary>
        /// 产品版本信息
        /// </summary>
        [JsonProperty("product_version")]
        public string ProductVersion { get; set; }

        /// <summary>
        /// 产品名称，这里应该是 "RabbitMQ"
        /// </summary>
        [JsonProperty("product_name")]
        public string ProductName { get; set; }

        /// <summary>
        /// RabbitMQ的版本信息
        /// </summary>
        [JsonProperty("rabbitmq_version")]
        public string RabbitmqVersion { get; set; }

        /// <summary>
        /// 集群名称，表示该RabbitMQ节点所属的集群
        /// </summary>
        [JsonProperty("cluster_name")]
        public string ClusterName { get; set; }

        /// <summary>
        /// 集群标签，通常用于对集群进行标记或分类
        /// </summary>
        [JsonProperty("cluster_tags")]
        public List<string> ClusterTags { get; set; }

        /// <summary>
        /// 节点标签，表示该节点的附加标记
        /// </summary>
        [JsonProperty("node_tags")]
        public List<string> NodeTags { get; set; }

        /// <summary>
        /// Erlang版本，RabbitMQ基于Erlang运行，因此Erlang版本至关重要
        /// </summary>
        [JsonProperty("erlang_version")]
        public string ErlangVersion { get; set; }

        /// <summary>
        /// Erlang的完整版本信息，包括操作系统等详细内容
        /// </summary>
        [JsonProperty("erlang_full_version")]
        public string ErlangFullVersion { get; set; }

        /// <summary>
        /// 发布系列的支持状态，通常为 "supported" 或 "unsupported"
        /// </summary>
        [JsonProperty("release_series_support_status")]
        public string ReleaseSeriesSupportStatus { get; set; }

        /// <summary>
        /// 是否禁用统计信息，设置为false表示启用统计
        /// </summary>
        [JsonProperty("disable_stats")]
        public bool DisableStats { get; set; }

        /// <summary>
        /// 是否允许操作策略更新
        /// </summary>
        [JsonProperty("is_op_policy_updating_enabled")]
        public bool IsOpPolicyUpdatingEnabled { get; set; }

        /// <summary>
        /// 是否启用队列总数统计
        /// </summary>
        [JsonProperty("enable_queue_totals")]
        public bool EnableQueueTotals { get; set; }

        /// <summary>
        /// 消息统计信息，跟踪发送、接收等各种消息活动
        /// </summary>
        [JsonProperty("message_stats")]
        public RabbitMqMessageStatsModel MessageStats { get; set; }

        /// <summary>
        /// 变化率统计信息，表示队列、连接等的变化频率
        /// </summary>
        [JsonProperty("churn_rates")]
        public RabbitMqChurnRatesModel ChurnRates { get; set; }

        /// <summary>
        /// 队列的统计信息，如消息数量、消息是否已确认等
        /// </summary>
        [JsonProperty("queue_totals")]
        public RabbitMqQueueTotalsModel QueueTotals { get; set; }

        /// <summary>
        /// RabbitMQ对象的总数，包括通道、消费者、交换机等
        /// </summary>
        [JsonProperty("object_totals")]
        public RabbitMqObjectTotalsModel ObjectTotals { get; set; }

        /// <summary>
        /// 用于统计数据库事件的队列数量
        /// </summary>
        [JsonProperty("statistics_db_event_queue")]
        public int StatisticsDbEventQueue { get; set; }

        /// <summary>
        /// 当前节点的名称
        /// </summary>
        public string Node { get; set; }

        /// <summary>
        /// RabbitMQ节点的监听器信息，显示了可用的协议和端口
        /// </summary>
        public List<RabbitMqListenerModel> Listeners { get; set; }

        /// <summary>
        /// RabbitMQ的上下文信息，例如HTTP管理界面、Prometheus监控等
        /// </summary>
        public List<RabbitMqContextModel> Contexts { get; set; }
    }
}
