using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRabbitConsumerFactory
    {
        /// <summary>
        /// 启动监听
        /// </summary>
        /// <returns></returns>
        Task ManualStartListenAsync();

        /// <summary>
        /// 启动监听
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        Task ManualStartListenAsync(string exchange, string queue);

        /// <summary>
        /// 停止监听
        /// </summary>
        /// <returns></returns>
        Task ManualStopListenAsync();

        /// <summary>
        /// 停止监听
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        Task ManualStopListenAsync(string exchange, string queue);
    }
}
