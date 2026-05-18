using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// RabbitMQ 事件处理器接口，用于捕获连接断开和回调异常等底层事件
    /// </summary>
    public interface IRabbitMqEventHandler
    {
        /// <summary>
        /// RabbitMQ 回调异常时触发
        /// </summary>
        /// <param name="sender">事件源对象</param>
        /// <param name="e">包含回调异常详细信息的事件参数</param>
        /// <returns></returns>
        Task CallbackExceptionAsync(object sender, CallbackExceptionEventArgs e);

        /// <summary>
        /// RabbitMQ 连接断开时触发
        /// </summary>
        /// <param name="sender">事件源对象</param>
        /// <param name="e">包含连接断开原因等详细信息的事件参数</param>
        /// <returns></returns>
        Task ConnectionShutdownAsync(object sender, ShutdownEventArgs e);
    }
}
