using System;

namespace Lycoris.RabbitMQ.Extensions.Options
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class RabbitBaseOption
    {
        /// <summary>
        /// 服务节点,可以是单独的hostname或者IP,也可以是host:port或者ip:port形式
        /// </summary>
        public string[] Hosts { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// 账号
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 虚拟机
        /// </summary>
        public string VirtualHost { get; set; } = "/";
    }
}
