using Lycoris.RabbitMQ.Extensions.Builder;
using Lycoris.RabbitMQ.Extensions.Builder.Consumer;
using Lycoris.RabbitMQ.Extensions.Builder.Consumer.Impl;
using Lycoris.RabbitMQ.Extensions.Impl;
using Lycoris.RabbitMQ.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RabbitMQBuilder
    {
        private readonly IServiceCollection services;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public RabbitMQBuilder(IServiceCollection services) => this.services = services;

        /// <summary>
        /// 禁用消费者自启动监听
        /// </summary>
        public bool DisableRabbitConsumerHostedListen { get; set; } = false;

        /// <summary>
        /// 添加多个RabbitMQ配置(可以添加多个)
        /// </summary>
        /// <param name="configureName"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public RabbitMQBuilder AddRabbitMQOption(string configureName, Action<RabbitOptions> configure)
        {
            var option = new RabbitOptions();
            configure(option);
            RabbitMQOptionsStore.AddOrUpdateRabbitMQOption(configureName, option);
            return this;
        }

        /// <summary>
        /// 添加RabbitMQ生产者服务
        /// </summary>
        /// <returns></returns>
        public RabbitMQBuilder AddRabbitProducer(string configureName, Action<RabbitProducerOptions> configure)
        {
            var option = new RabbitProducerOptions();

            configure(option);

            // 验证基础配置
            option.CheckBaseOptions();

            // 验证延迟队列配置
            option.CheckDelayProps();

            RabbitMQOptionsStore.AddOrUpdateRabbitProducerOptions(configureName, option);

            // 注册服务
            services.TryAddSingleton<IRabbitProducerFactory, RabbitProducerFactory>();
            return this;
        }

        /// <summary>
        /// 添加RabbitMQ消费者服务
        /// </summary>
        /// <returns></returns>
        public IRabbitConsumerBuilder AddRabbitConsumer(Action<RabbitConsumerOptions> configure)
        {
            var option = new RabbitConsumerOptions();
            configure(option);

            // 验证基础配置
            option.CheckBaseOptions();

            if (!this.DisableRabbitConsumerHostedListen)
                services.AddRabbitConsumerHostedListen();

            return new RabbitConsumerBuilder(services, option);
        }
    }
}
