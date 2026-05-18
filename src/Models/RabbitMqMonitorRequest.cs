using Newtonsoft.Json;

namespace Lycoris.RabbitMQ.Extensions.Models
{
    /// <summary>
    /// RabbitMQ 管理 API 监控请求参数。
    /// 用于调用 RabbitMQ Management HTTP API 时传递连接凭据和目标主机信息。
    /// </summary>
    public class RabbitMqMonitorRequest
    {
        /// <summary>
        /// RabbitMQ 服务主机地址。
        /// </summary>
        [JsonRequired]
        public string Host { get; set; }

        /// <summary>
        /// RabbitMQ 管理 API 端口。
        /// </summary>
        [JsonRequired]
        public int Prot { get; set; }

        /// <summary>
        /// RabbitMQ 管理界面登录账号。
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// RabbitMQ 管理界面登录密码。
        /// </summary>
        public string Passwrod { get; set; }
    }
}
