using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultRabbitMqEventHandler : IRabbitMqEventHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public virtual Task CallbackExceptionAsync(object sender, CallbackExceptionEventArgs e)
        {
            Console.WriteLine($"rabbit mq callback exception -> {e.Exception.Message}");
            Console.WriteLine(e.Exception.StackTrace);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public virtual Task ConnectionShutdownAsync(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("rabbit mq disconnected");
            return Task.CompletedTask;
        }
    }
}
