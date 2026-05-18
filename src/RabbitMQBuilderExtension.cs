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
    /// RabbitMQ 构建器扩展方法，简化生产者/消费者服务的注册流程。
    /// 提供 <see cref="IServiceCollection"/> 上的流畅 API，用于配置连接参数、注册生产者和消费者。
    /// </summary>
    public static class RabbitMqBuilderExtension
    {
        /// <summary>
        /// 注册 RabbitMQ 扩展基础设施，配置基础连接参数并初始化内部服务。
        /// 会自动注册消费者工厂（<see cref="IRabbitConsumerFactory"/>）和默认事件处理器（<see cref="DefaultRabbitMqEventHandler"/>）。
        /// </summary>
        /// <param name="services">依赖注入服务集合</param>
        /// <param name="configure">用于配置 RabbitMQ 连接参数（主机、端口、账号密码、虚拟主机等）的委托</param>
        /// <returns><see cref="RabbitMqBuilder"/> 构建器，支持链式调用以进一步注册生产者和消费者服务</returns>
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
            RabbitMQOptionsStore.TryAddRabbitMQOption(builder.configureName, new RabbitOption()
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
        /// 添加带自定义配置名称的生产者服务，适用于多生产者场景。
        /// 通过独立的配置名称为不同生产者维护各自的路由队列和交换器设置。
        /// </summary>
        /// <param name="builder"><see cref="RabbitMqBuilder"/> 构建器实例</param>
        /// <param name="configName">自定义配置名称，用于标识和区分不同的生产者配置</param>
        /// <param name="configure">用于配置生产者参数（交换机类型、路由队列、延迟队列设置等）的委托</param>
        /// <returns><see cref="RabbitMqBuilder"/> 构建器，支持链式调用</returns>
        public static RabbitMqBuilder AddRabbitProducer(this RabbitMqBuilder builder, string configName, Action<RabbitProducerOption> configure)
        {
            var option = new RabbitProducerOption();

            option.UseRabbitOption(builder.configureName);

            configure.Invoke(option);

            // 验证延迟队列配置
            option.CheckDelayProps();

            foreach (var item in option.RouteQueues)
            {
                item.Options.Durable = option.Durable;
                item.Options.AutoDelete = option.AutoDelete;
                if (item.Options.DeadLetterExchange == null)
                    item.Options.DeadLetterExchange = option.DeadLetterExchange;
            }

            RabbitMQOptionsStore.AddOrUpdateRabbitProducerOptions(configName, option);

            // 注册服务
            builder.services.TryAddSingleton<IRabbitProducerFactory, RabbitProducerFactory>();

            return builder;
        }

        /// <summary>
        /// 添加生产者服务，自动注册所有通过 <c>AddRabbitProducer&lt;T&gt;</c> 声明的生产者类型。
        /// 遍历 <see cref="RabbitProducerOption.RabbitProducer"/> 字典，将每个生产者实现类注册为 Scoped 服务。
        /// </summary>
        /// <param name="builder"><see cref="RabbitMqBuilder"/> 构建器实例</param>
        /// <param name="configure">用于配置生产者参数（交换机类型、路由队列、延迟队列设置等）的委托</param>
        /// <returns><see cref="RabbitMqBuilder"/> 构建器，支持链式调用</returns>
        public static RabbitMqBuilder AddRabbitProducer(this RabbitMqBuilder builder, Action<RabbitProducerOption> configure)
        {
            var option = new RabbitProducerOption();

            option.UseRabbitOption(builder.configureName);

            configure.Invoke(option);

            // 验证延迟队列配置
            option.CheckDelayProps();

            foreach (var item in option.RouteQueues)
            {
                item.Options.Durable = option.Durable;
                item.Options.AutoDelete = option.AutoDelete;
                if (item.Options.DeadLetterExchange == null)
                    item.Options.DeadLetterExchange = option.DeadLetterExchange;
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
        /// 注册消费者服务，按配置绑定消费者监听器并自动启动托管服务。
        /// 支持队列监听和交换机监听，支持键控服务（Keyed Service）和普通委托回调两种消费模式。
        /// 除非已设置 <see cref="RabbitMqBuilder.DisableRabbitConsumerHostedListen"/>，否则会自动注册后台托管服务来启动消费者监听。
        /// </summary>
        /// <param name="builder"><see cref="RabbitMqBuilder"/> 构建器实例</param>
        /// <param name="configure">用于配置消费者参数（绑定队列、交换机、消费回调等）的委托</param>
        /// <returns><see cref="RabbitMqBuilder"/> 构建器，支持链式调用</returns>
        public static RabbitMqBuilder AddRabbitConsumer(this RabbitMqBuilder builder, Action<RabbitConsumerOption> configure)
        {
            var option = new RabbitConsumerOption();

            option.UseRabbitOption(builder.configureName);

            configure.Invoke(option);

            foreach (var item in option.RouteQueues)
            {
                item.Options.Durable = option.Durable;
                item.Options.AutoDelete = option.AutoDelete;
                if (item.Options.DeadLetterExchange == null)
                    item.Options.DeadLetterExchange = option.DeadLetterExchange;
            }

            var consoumerBuilder = new DefaultRabbitConsumerBuilder(builder.services, option);

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
