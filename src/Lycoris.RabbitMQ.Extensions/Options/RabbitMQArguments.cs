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
            get => new()
            {
                { "x-queue-type", "classic" }
            };
        }

        /// <summary>
        /// MQ延迟队列基础配置参数
        /// </summary>
        public static Dictionary<string, object> Delayed
        {
            get => new()
            {
                { "x-delayed-type", "direct" },
                { "x-queue-type", "classic" }
            };
        }
    }
}
