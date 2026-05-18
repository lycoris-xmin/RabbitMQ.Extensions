using System.Collections.Generic;

namespace Lycoris.RabbitMQ.Extensions.Options
{
    /// <summary>
    /// 路由队列绑定配置，包含路由键、队列名称、队列选项及自定义参数。
    /// </summary>
    public class RouteQueue
    {
        /// <summary>
        /// 初始化 <see cref="RouteQueue"/> 的新实例，并创建默认的 <see cref="QueueOption"/>。
        /// </summary>
        public RouteQueue() => Options = new QueueOption();

        /// <summary>
        /// 路由键（Routing Key）。
        /// </summary>
        public string Route { get; set; } = string.Empty;

        /// <summary>
        /// 队列名称。
        /// </summary>
        public string Queue { get; set; } = string.Empty;

        /// <summary>
        /// 队列声明选项。
        /// </summary>
        public QueueOption Options { get; set; }

        /// <summary>
        /// 自定义参数字典。
        /// </summary>
        public IDictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();
    }
}
