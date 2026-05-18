using Newtonsoft.Json;
using System.Collections.Generic;

namespace Lycoris.RabbitMQ.Extensions.Models
{
    /// <summary>
    /// RabbitMQ上下文
    /// </summary>
    public class RabbitMqContextModel
    {
        /// <summary>
        /// SSL选项（如果有）
        /// </summary>
        [JsonProperty("ssl_opts")]
        public List<string> SslOpts { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string Node { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 上下文的路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// cowboy配置选项
        /// </summary>
        [JsonProperty("cowboy_opts")]
        public string CowboyOpts { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// 使用的协议（如http、https等）   
        /// </summary>
        public string Protocol { get; set; }
    }
}
