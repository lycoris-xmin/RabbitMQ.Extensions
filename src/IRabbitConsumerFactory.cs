using System.Threading;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// RabbitMQ 消费者工厂，负责启动和停止所有已注册的消费者监听
    /// </summary>
    public interface IRabbitConsumerFactory
    {
        /// <summary>
        /// 启动所有已注册的消费者监听
        /// </summary>
        /// <returns></returns>
        Task ManualStartListenAsync();

        /// <summary>
        /// 启动指定队列的消费者监听
        /// </summary>
        /// <param name="queue">要启动监听的队列名称</param>
        /// <returns></returns>
        Task ManualStartListenAsync(string queue);

        /// <summary>
        /// 启动指定交换机和队列的消费者监听
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <returns></returns>
        Task ManualStartListenAsync(string exchange, string queue);

        /// <summary>
        /// 停止所有消费者监听
        /// </summary>
        /// <returns></returns>
        Task ManualStopListenAsync();

        /// <summary>
        /// 停止所有消费者监听并注册取消令牌回调
        /// </summary>
        /// <param name="cancellationToken">取消令牌，用于注册取消回调</param>
        /// <returns></returns>
        Task ManualStopListenAsync(CancellationToken cancellationToken);

        /// <summary>
        /// 停止指定队列的消费者监听
        /// </summary>
        /// <param name="queue">要停止监听的队列名称</param>
        /// <returns></returns>
        Task ManualStopListenAsync(string queue);

        /// <summary>
        /// 停止指定队列的消费者监听并注册取消令牌回调
        /// </summary>
        /// <param name="queue">要停止监听的队列名称</param>
        /// <param name="cancellationToken">取消令牌，用于注册取消回调</param>
        /// <returns></returns>
        Task ManualStopListenAsync(string queue, CancellationToken cancellationToken);

        /// <summary>
        /// 停止指定交换机和队列的消费者监听
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <returns></returns>
        Task ManualStopListenAsync(string exchange, string queue);

        /// <summary>
        /// 停止指定交换机和队列的消费者监听并注册取消令牌回调
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="cancellationToken">取消令牌，用于注册取消回调</param>
        /// <returns></returns>
        Task ManualStopListenAsync(string exchange, string queue, CancellationToken cancellationToken);
    }
}
