namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 交换机类型
    /// </summary>
    public enum RabbitExchangeType
    {
        /// <summary>
        /// 普通模式
        /// </summary>
        None = 0,
        /// <summary>
        /// 路由模式
        /// </summary>
        Direct = 1,
        /// <summary>
        /// 发布/订阅模式
        /// </summary>
        Fanout = 2,
        /// <summary>
        /// 匹配订阅模式
        /// </summary>
        Topic = 3,
        /// <summary>
        /// 延迟交换机（设置延迟交换机时,还需要设置延时时间）
        /// </summary>
        Delayed = 4
    }
}
