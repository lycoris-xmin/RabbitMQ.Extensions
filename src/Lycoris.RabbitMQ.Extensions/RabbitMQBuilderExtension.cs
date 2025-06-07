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
    public static class RabbitMqBuilderExtension
    {
        /// <summary>
        /// 注册RabbitMQ扩展
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static RabbitMqBuilder AddRabbitMQExtensions(this IServiceCollection services, Action<RabbitMqBuilder> configure)
        {
            // 消费者工厂
            services.TryAddSingleton<IRabbitConsumerFactory, RabbitConsumerFactory>();

            // 配置构建
            var builder = new RabbitMqBuilder(services);

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

            builder.services.TryAddTransient<IRabbitMqEventHandler, DefaultRabbitMqEventHandler>();

            return builder;
        }

        /// <summary>
        /// 添加生产者服务
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configName"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static RabbitMqBuilder AddRabbitProducer(this RabbitMqBuilder builder, string configName, Action<RabbitProducerOption> configure)
        {
            var option = new RabbitProducerOption();

            // 
            option.UseRabbitOption(builder.configureName);

            // 
            configure.Invoke(option);

            // 验证延迟队列配置
            option.CheckDelayProps();

            foreach (var item in option.RouteQueues)
            {
                item.Options.Durable = option.Durable;
                item.Options.AutoDelete = option.AutoDelete;
            }

            RabbitMQOptionsStore.AddOrUpdateRabbitProducerOptions(configName, option);

            // 注册服务
            builder.services.TryAddSingleton<IRabbitProducerFactory, RabbitProducerFactory>();

            return builder;
        }

        /// <summary>
        /// 添加生产者服务
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static RabbitMqBuilder AddRabbitProducer(this RabbitMqBuilder builder, Action<RabbitProducerOption> configure)
        {
            var option = new RabbitProducerOption();

            // 
            option.UseRabbitOption(builder.configureName);

            // 
            configure.Invoke(option);

            // 验证延迟队列配置
            option.CheckDelayProps();

            foreach (var item in option.RouteQueues)
            {
                item.Options.Durable = option.Durable;
                item.Options.AutoDelete = option.AutoDelete;
            }

            foreach (var item in option.RabbitProducer)
            {
                RabbitMQOptionsStore.AddOrUpdateRabbitProducerOptions(item.Value.FullName, option);
                if (item.Key == item.Value)
                    builder.services.TryAddScoped(item.Value);
                else
                    builder.services.TryAddScoped(item.Key, item.Value);
            }

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
        public static RabbitMqBuilder AddRabbitConsumer(this RabbitMqBuilder builder, Action<RabbitConsumerOption> configure)
        {
            var option = new RabbitConsumerOption();

            option.UseRabbitOption(builder.configureName);

            configure.Invoke(option);

            foreach (var item in option.RouteQueues)
            {
                item.Options.Durable = option.Durable;
                item.Options.AutoDelete = option.AutoDelete;
            }

            var consoumerBuilder = new DafaultRabbitConsumerBuilder(builder.services, option);

            if (option.ConsumerMaps.Count > 0)
            {
                foreach (var item in option.ConsumerMaps)
                {
                    if (!option.RouteQueues.Any(x => x.Queue == item.Queue))
                        throw new Exception($"consumer binding queue:{item.Queue} does not exist in the route queues collection");

                    if (item.Listener == null && item.MessageRecieved == null)
                        continue;

                    if (item.Listener != null)
                    {
                        if (string.IsNullOrEmpty(item.Exchange))
                        {
                            if (item.KeyConsumer)
                            {
                                if (string.IsNullOrEmpty(item.KeyName))
                                    consoumerBuilder.AddDefaultKeyConsumer(item.Queue, item.Listener);
                                else
                                    consoumerBuilder.AddKeyConsumer(item.Queue, item.Listener, item.KeyName);
                            }
                            else
                                consoumerBuilder.AddConsumer(item.Queue, item.Listener);
                        }
                        else
                        {
                            if (item.KeyConsumer)
                            {
                                if (string.IsNullOrEmpty(item.KeyName))
                                    consoumerBuilder.AddDefaultKeyConsumer(item.Exchange, item.Queue, item.Listener);
                                else
                                    consoumerBuilder.AddKeyConsumer(item.Exchange, item.Queue, item.Listener, item.KeyName);
                            }
                            else
                                consoumerBuilder.AddConsumer(item.Exchange, item.Queue, item.Listener);
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(item.Exchange))
                            consoumerBuilder.AddConsumer(item.Queue, item.MessageRecieved);
                        else
                            consoumerBuilder.AddConsumer(item.Exchange, item.Queue, item.MessageRecieved);
                    }
                }
            }

            if (!builder.DisableRabbitConsumerHostedListen && !builder.services.Any(f => f.ImplementationType == typeof(DefaultRabbitConsumerHostedService)))
                builder.services.AddHostedService<DefaultRabbitConsumerHostedService>();

            return builder;
        }
    }
}
