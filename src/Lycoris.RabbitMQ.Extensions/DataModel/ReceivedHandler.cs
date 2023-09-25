namespace Lycoris.RabbitMQ.Extensions.DataModel
{
    /// <summary>
    /// 处理结果枚举
    /// </summary>
    public enum ReceivedHandler
    {
        /// <summary>
        /// 提交
        /// </summary>
        Commit = 0,
        /// <summary>
        /// 回滚
        /// </summary>
        RollBack = 1,
        /// <summary>
        /// 重新放回队列(默认延迟一秒)
        /// 可通过设置属性 <see cref="BaseRabbitConsumerListener.ResubmitTimeSpan"/> 设置延时时间
        /// </summary>
        Resubmit = 2
    }
}
