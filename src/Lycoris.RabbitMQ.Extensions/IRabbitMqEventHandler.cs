using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRabbitMqEventHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task CallbackExceptionAsync(object sender, CallbackExceptionEventArgs e);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task ConnectionShutdownAsync(object sender, ShutdownEventArgs e);
    }
}
