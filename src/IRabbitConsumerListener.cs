using Lycoris.RabbitMQ.Extensions.DataModel;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 消费者监听器接口，实现此接口以处理从队列接收的消息
    /// </summary>
    public interface IRabbitConsumerListener
    {
        /// <summary>
        /// 处理接收到的消息
        /// </summary>
        /// <param name="recieveResult"><see cref="RecieveResult"/> 消息体，包含接收到的消息内容及元数据</param>
        /// <returns></returns>
        Task ConsumeAsync(RecieveResult recieveResult);
    }
}
