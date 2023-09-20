using Lycoris.RabbitMQ.Extensions.DataModel;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRabbitConsumerListener
    {
        /// <summary>
        /// 消费消息
        /// </summary>
        /// <param name="recieveResult"><see cref="RecieveResult"/>消息体</param>
        /// <returns></returns>
        Task ConsumeAsync(RecieveResult recieveResult);
    }
}
