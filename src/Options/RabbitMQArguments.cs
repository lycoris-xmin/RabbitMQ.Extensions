using System.Collections.Generic;

namespace Lycoris.RabbitMQ.Extensions.Options
{
    /// <summary>
    /// 
    /// </summary>
    internal class RabbitMQArguments
    {
        /// <summary>
        /// MQ普通基础配置参数
        /// </summary>
        public static Dictionary<string, object> Default
        {
            get => new Dictionary<string, object>()
            {
                { "x-queue-type", "classic" }
            };
        }

        /// <summary>
        /// MQ延迟队列基础配置参数
        /// </summary>
        public static Dictionary<string, object> Delayed
        {
            get => new Dictionary<string, object>()
            {
                { "x-delayed-type", "direct" },
                { "x-queue-type", "classic" }
            };
        }
    }
}
