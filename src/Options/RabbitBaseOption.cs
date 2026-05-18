using System;

namespace Lycoris.RabbitMQ.Extensions.Options
{
    /// <summary>
    /// RabbitMQ 基础连接配置选项，提供连接到 RabbitMQ 服务所需的通用参数。
    /// </summary>
    public abstract class RabbitBaseOption
    {
        /// <summary>
        /// 服务节点列表，支持 hostname、IP 地址或 host:port 格式。
        /// </summary>
        public string[] Hosts { get; set; } = Array.Empty<string>();

        /// <summary>
        /// AMQP 协议端口，默认 5672。
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// 管理 API 端口，默认 15672。
        /// </summary>
        public int ApiPort { get; set; } = 15672;

        /// <summary>
        /// 登录账号。
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 登录密码。
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 虚拟主机名称，默认 "/"。
        /// </summary>
        public string VirtualHost { get; set; } = "/";
    }
}
