using Lycoris.RabbitMQ.Extensions.Impl;
using Lycoris.RabbitMQ.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RabbitMqBuilder : RabbitOption
    {
        internal readonly string configureName;
        internal readonly IServiceCollection services;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public RabbitMqBuilder(IServiceCollection services)
        {
            this.services = services;
            this.configureName = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 禁用消费者自启动监听
        /// 禁用后，需要在你需要使用消费者服务前，主动调用 <see cref="IRabbitConsumerFactory.ManualStartListenAsync()"/> 启用已添加的消费者服务监听
        /// </summary>
        public bool DisableRabbitConsumerHostedListen { get; set; } = false;

        /// <summary>
        /// Api服务
        /// </summary>
        public void AddApiService() => services.TryAddSingleton<IRabbitMqApiService, RabbitMqApiService>();

        /// <summary>
        /// 添加事件捕捉器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddEventHandler<T>() where T : class, IRabbitMqEventHandler => services.AddTransient<IRabbitMqEventHandler, T>();
    }
}
