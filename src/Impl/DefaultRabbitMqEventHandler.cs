using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// 默认 RabbitMQ 事件处理器，将连接断开和回调异常信息输出到控制台
    /// </summary>
    public class DefaultRabbitMqEventHandler : IRabbitMqEventHandler
    {
        /// <summary>
        /// 输出 RabbitMQ 回调异常的消息和堆栈信息到控制台
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">回调异常事件参数</param>
        public virtual Task CallbackExceptionAsync(object sender, CallbackExceptionEventArgs e)
        {
            Console.WriteLine($"rabbit mq callback exception -> {e.Exception.Message}");
            Console.WriteLine(e.Exception.StackTrace);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 输出连接断开信息到控制台
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">连接关闭事件参数</param>
        public virtual Task ConnectionShutdownAsync(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("rabbit mq disconnected");
            return Task.CompletedTask;
        }
    }
}
