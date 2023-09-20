using System.Collections.Generic;
namespace Lycoris.RabbitMQ.Extensions.Options
{
    /// <summary>
    /// 
    /// </summary>
    public class RouteQueue
    {
        /// <summary>
        /// 
        /// </summary>
        public RouteQueue()
        {
            Options = new QueueOptions();
        }

        /// <summary>
        /// 路由
        /// </summary>
        public string Route { get; set; } = string.Empty;

        /// <summary>
        /// 队列
        /// </summary>
        public string Queue { get; set; } = string.Empty;

        /// <summary>
        /// 队列选项
        /// </summary>
        public QueueOptions Options { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public IDictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();
    }
}
