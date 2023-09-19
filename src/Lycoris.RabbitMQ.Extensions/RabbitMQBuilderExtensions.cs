using Lycoris.RabbitMQ.Extensions.Builder;
using Lycoris.RabbitMQ.Extensions.Builder.Consumer;
using Lycoris.RabbitMQ.Extensions.Builder.Consumer.Impl;
using Lycoris.RabbitMQ.Extensions.Impl;
using Lycoris.RabbitMQ.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class RabbitMQBuilderExtensions
    {
        /// <summary>
        /// 注册RabbitMQ简易扩展
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQExtensions(this IServiceCollection services, [NotNull] Action<RabbitMQBuilder> action)
        {
            var builder = new RabbitMQBuilder(services);
            action(builder);
            return services;
        }

        /// <summary>
        /// 注册添加一个生产者
        /// </summary>
        /// <param name="services"></param>
        /// <param name="name"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitProducer(this IServiceCollection services, string name, [NotNull] Action<RabbitProducerOptions> configure)
        {
            var option = new RabbitProducerOptions();
            configure(option);

            // 验证基础配置
            option.CheckBaseOptions();

            // 验证延迟队列配置
            option.CheckDelayProps();

            RabbitMQOptionsStore.AddOrUpdateRabbitProducerOptions(name, option);
            services.TryAddSingleton<IRabbitProducerFactory, RabbitProducerFactory>();
            return services;
        }

        /// <summary>
        /// 注册消费者服务,获取消费创建对象
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddRabbitConsumer(this IServiceCollection services, [NotNull] Action<RabbitConsumerOptions> configure)
        {
            var option = new RabbitConsumerOptions();
            configure(option);

            // 验证基础配置
            option.CheckBaseOptions();

            services.AddRabbitConsumerCore();
            return new RabbitConsumerBuilder(services, option);
        }

        /// <summary>
        /// 注册消费者服务对象
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IServiceCollection AddRabbitConsumerCore(this IServiceCollection services)
        {
            if (!services.Any(f => f.ImplementationType == typeof(RabbitConsumerHostedService)))
                services.AddHostedService<RabbitConsumerHostedService>();

            return services;
        }
    }
}
