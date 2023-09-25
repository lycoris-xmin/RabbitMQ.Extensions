using Lycoris.RabbitMQ.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RabbitMQBuilder : RabbitOption
    {
        internal readonly string configureName;
        internal readonly IServiceCollection services;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public RabbitMQBuilder(IServiceCollection services)
        {
            this.services = services;
            this.configureName = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 禁用消费者自启动监听
        /// </summary>
        public bool DisableRabbitConsumerHostedListen { get; set; } = false;
    }
}
