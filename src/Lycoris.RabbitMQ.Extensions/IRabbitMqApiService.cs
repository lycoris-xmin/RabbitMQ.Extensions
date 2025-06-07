using Lycoris.RabbitMQ.Extensions.Models;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRabbitMqApiService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<RabbitMqMonitorResponse> MonitorApiAsync(RabbitMqMonitorRequest input);
    }
}
