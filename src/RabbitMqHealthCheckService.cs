using Lycoris.RabbitMQ.Extensions.Builder;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// RabbitMQ 健康检查结果
    /// </summary>
    public class RabbitMqHealthCheckResult
    {
        /// <summary>
        /// 是否健康
        /// </summary>
        public bool IsHealthy { get; set; }

        /// <summary>
        /// 检查结果描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 各节点检查详情
        /// </summary>
        public List<RabbitMqNodeHealthDetail> Details { get; set; } = new List<RabbitMqNodeHealthDetail>();
    }

    /// <summary>
    /// 节点健康检查详情
    /// </summary>
    public class RabbitMqNodeHealthDetail
    {
        /// <summary>
        /// 节点地址
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 虚拟机
        /// </summary>
        public string VirtualHost { get; set; }

        /// <summary>
        /// 是否健康
        /// </summary>
        public bool IsHealthy { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; set; }
    }

    /// <summary>
    /// RabbitMQ 健康检查服务，通过尝试建立 AMQP 连接来检测各节点健康状态
    /// </summary>
    public class RabbitMqHealthCheckService
    {
        /// <summary>
        /// 检查所有已配置的 RabbitMQ 连接，返回综合健康检查结果
        /// </summary>
        /// <returns>包含所有节点健康状态的 <see cref="RabbitMqHealthCheckResult"/></returns>
        public async Task<RabbitMqHealthCheckResult> CheckHealthAsync()
        {
            var options = RabbitMQOptionsStore.GetAllRabbitMQOption();
            var result = new RabbitMqHealthCheckResult();

            if (options == null || options.Count == 0)
            {
                result.IsHealthy = false;
                result.Description = "未配置任何 RabbitMQ 连接";
                return result;
            }

            foreach (var opt in options)
            {
                foreach (var host in opt.Hosts)
                {
                    var detail = await CheckNodeAsync(host, opt.Port, opt.VirtualHost, opt.UserName, opt.Password);
                    result.Details.Add(detail);
                }
            }

            result.IsHealthy = result.Details.All(x => x.IsHealthy);
            result.Description = result.IsHealthy ? "所有节点连接正常" : "部分节点连接异常";

            return result;
        }

        /// <summary>
        /// 检查指定配置名称的 RabbitMQ 连接，返回健康检查结果
        /// </summary>
        /// <param name="configureName">RabbitMQ 配置名称</param>
        /// <returns>包含该配置节点健康状态的 <see cref="RabbitMqHealthCheckResult"/></returns>
        public async Task<RabbitMqHealthCheckResult> CheckHealthAsync(string configureName)
        {
            var opt = RabbitMQOptionsStore.GetRabbitMQOption(configureName);
            var result = new RabbitMqHealthCheckResult();

            if (opt == null || opt.Hosts == null || opt.Hosts.Length == 0)
            {
                result.IsHealthy = false;
                result.Description = $"未找到名称为 '{configureName}' 的 RabbitMQ 配置";
                return result;
            }

            foreach (var host in opt.Hosts)
            {
                var detail = await CheckNodeAsync(host, opt.Port, opt.VirtualHost, opt.UserName, opt.Password);
                result.Details.Add(detail);
            }

            result.IsHealthy = result.Details.All(x => x.IsHealthy);
            result.Description = result.IsHealthy ? "连接正常" : "连接异常";

            return result;
        }

        /// <summary>
        /// 尝试建立 AMQP 连接以检测单个节点健康状态
        /// </summary>
        /// <param name="host">主机地址</param>
        /// <param name="port">端口</param>
        /// <param name="virtualHost">虚拟主机</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>节点健康检查详情</returns>
        private static async Task<RabbitMqNodeHealthDetail> CheckNodeAsync(string host, int port, string virtualHost, string username, string password)
        {
            var parts = host.Split(new[] { ":" }, System.StringSplitOptions.RemoveEmptyEntries);
            var hostName = parts[0];
            var hostPort = parts.Length > 1 && int.TryParse(parts[1], out var p) ? p : port;

            var detail = new RabbitMqNodeHealthDetail
            {
                Host = host,
                Port = hostPort,
                VirtualHost = virtualHost
            };

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = hostName,
                    Port = hostPort,
                    VirtualHost = virtualHost,
                    UserName = username,
                    Password = password,
                    RequestedConnectionTimeout = System.TimeSpan.FromSeconds(5),
                    RequestedHeartbeat = System.TimeSpan.FromSeconds(5)
                };

                using (var connection = await factory.CreateConnectionAsync())
                using (var channel = await connection.CreateChannelAsync())
                {
                    detail.IsHealthy = connection.IsOpen;
                }
            }
            catch (System.Exception ex)
            {
                detail.IsHealthy = false;
                detail.Error = ex.Message;
            }

            return detail;
        }
    }
}
