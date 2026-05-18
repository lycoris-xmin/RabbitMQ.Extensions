using Lycoris.RabbitMQ.Extensions.Models;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// RabbitMQ 管理 API 服务接口，提供节点状态查询和监控功能
    /// </summary>
    public interface IRabbitMqApiService
    {
        /// <summary>
        /// 获取 RabbitMQ 节点的监控概览信息
        /// </summary>
        /// <param name="input">监控请求参数，包含目标节点信息</param>
        /// <returns>包含节点监控概览数据的响应对象</returns>
        Task<RabbitMqMonitorResponse> MonitorApiAsync(RabbitMqMonitorRequest input);
    }
}
