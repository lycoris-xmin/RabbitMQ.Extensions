using Lycoris.RabbitMQ.Extensions.Builder;
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
    public static class RabbitMQBuilderExtension
    {
        /// <summary>
        /// 注册RabbitMQ扩展
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static RabbitMQBuilder AddRabbitMQExtensions(this IServiceCollection services, Action<RabbitMQBuilder> configure)
        {
            // 生产者工厂
            services.TryAddSingleton<IRabbitConsumerFactory, RabbitConsumerFactory>();

            // 配置构建
            var builder = new RabbitMQBuilder(services);
            configure.Invoke(builder);

            // 检查基础配置
            builder.CheckBaseOptions();

            // 添加配置项
            RabbitMQOptionsStore.AddOrUpdateRabbitMQOption(builder.configureName, new RabbitOption()
            {
                Hosts = builder.Hosts,
                Port = builder.Port,
                UserName = builder.UserName,
                Password = builder.Password,
                VirtualHost = builder.VirtualHost,
                Durable = builder.Durable,
                AutoDelete = builder.AutoDelete,
                Arguments = builder.Arguments,
                BasicProps = builder.BasicProps
            });

            return builder;
        }

        /// <summary>
        /// 添加生产者服务
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configName"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static RabbitMQBuilder AddRabbitProducer(this RabbitMQBuilder builder, string configName, Action<RabbitProducerOption> configure)
        {
            var option = new RabbitProducerOption();

            // 
            option.UseRabbitOption(builder.configureName);

            // 
            configure.Invoke(option);

            // 验证延迟队列配置
            option.CheckDelayProps();

            RabbitMQOptionsStore.AddOrUpdateRabbitProducerOptions(configName, option);

            // 注册服务
            builder.services.TryAddSingleton<IRabbitProducerFactory, RabbitProducerFactory>();

            return builder;
        }

        /// <summary>
        /// 添加生产者服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static RabbitMQBuilder AddRabbitProducer<T>(this RabbitMQBuilder builder, Action<RabbitProducerOption> configure) where T : BaseRabbitProducerService
        {
            var option = new RabbitProducerOption();

            // 
            option.UseRabbitOption(builder.configureName);

            // 
            configure.Invoke(option);

            // 验证延迟队列配置
            option.CheckDelayProps();

            RabbitMQOptionsStore.AddOrUpdateRabbitProducerOptions(typeof(T).FullName, option);

            builder.services.TryAddScoped<T>();

            // 注册服务
            builder.services.TryAddSingleton<IRabbitProducerFactory, RabbitProducerFactory>();

            return builder;
        }

        /// <summary>
        /// 添加生产者服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static RabbitMQBuilder AddRabbitProducer<TService, TImplementation>(this RabbitMQBuilder builder, Action<RabbitProducerOption> configure) where TService : class where TImplementation : BaseRabbitProducerService, TService
        {
            var option = new RabbitProducerOption();

            // 
            option.UseRabbitOption(builder.configureName);

            // 
            configure.Invoke(option);

            // 验证延迟队列配置
            option.CheckDelayProps();

            RabbitMQOptionsStore.AddOrUpdateRabbitProducerOptions(typeof(TImplementation).FullName, option);

            builder.services.TryAddScoped<TService, TImplementation>();

            // 注册服务
            builder.services.TryAddSingleton<IRabbitProducerFactory, RabbitProducerFactory>();

            return builder;
        }

        /// <summary>
        /// 注册消费者服务
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static RabbitMQBuilder AddRabbitConsumer(this RabbitMQBuilder builder, Action<RabbitConsumerOption> configure)
        {
            var option = new RabbitConsumerOption();

            option.UseRabbitOption(builder.configureName);

            configure.Invoke(option);

            var consoumerBuilder = new DafaultRabbitConsumerBuilder(builder.services, option);

            if (option.ListenerMaps.Count > 0)
            {
                foreach (var item in option.ListenerMaps)
                {
                    if (!option.RouteQueues.Any(x => x.Queue == item.Queue))
                        throw new Exception($"consumer binding queue:{item.Queue} does not exist in the route queues collection");

                    if (item.Listener == null && item.MessageRecieved == null)
                        continue;

                    if (string.IsNullOrEmpty(item.Exchange))
                    {
                        if (item.Listener != null)
                            consoumerBuilder.AddListener(item.Queue, item.Listener);
                        else
                            consoumerBuilder.AddListener(item.Queue, item.MessageRecieved);
                    }
                    else
                    {
                        if (item.Listener != null)
                            consoumerBuilder.AddListener(item.Exchange, item.Queue, item.Listener);
                        else
                            consoumerBuilder.AddListener(item.Exchange, item.Queue, item.MessageRecieved);
                    }
                }
            }

            if (!builder.DisableRabbitConsumerHostedListen && !builder.services.Any(f => f.ImplementationType == typeof(RabbitConsumerHostedService)))
                builder.services.AddHostedService<RabbitConsumerHostedService>();

            return builder;
        }
    }
}
