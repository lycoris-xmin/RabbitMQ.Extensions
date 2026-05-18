using Lycoris.RabbitMQ.Extensions.DataModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Builder.Consumer.Impl
{
    /// <summary>
    /// 默认消费者构建器，实现 <see cref="IRabbitConsumerBuilder"/>，将监听配置注册到 DI 容器。
    /// 使用 <see cref="DefaultRabbitConsumerProvider"/> 封装每个消费者的创建和监听逻辑，以 Singleton 生命周期注册。
    /// </summary>
    internal sealed class DefaultRabbitConsumerBuilder : IRabbitConsumerBuilder
    {
        /// <summary>
        /// 依赖注入服务集合。
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// 消费者配置选项，包含连接参数、队列声明配置和交换机绑定配置。
        /// </summary>
        private readonly Options.RabbitConsumerOption _rabbitConsumerOptions;

        /// <summary>
        /// 使用服务集合和消费者配置初始化 <see cref="DefaultRabbitConsumerBuilder"/>。
        /// </summary>
        /// <param name="services">依赖注入服务集合</param>
        /// <param name="rabbitConsumerOptions">消费者配置选项，包含连接参数、队列声明和路由配置</param>
        public DefaultRabbitConsumerBuilder(IServiceCollection services, Options.RabbitConsumerOption rabbitConsumerOptions)
        {
            Services = services;
            _rabbitConsumerOptions = rabbitConsumerOptions;
        }

        /// <summary>
        /// 添加队列监听，注册 <see cref="DefaultRabbitConsumerProvider"/> 到 DI 容器。
        /// 每次消费消息时会创建新的 DI Scope，确保 Scoped 服务在消费回调中正常工作。
        /// </summary>
        /// <param name="queue">队列名称</param>
        /// <param name="onMessageRecieved">消息接收回调，接收 <see cref="IServiceProvider"/> 和 <see cref="RecieveResult"/> 两个参数</param>
        /// <returns>消费者构建器，支持链式调用</returns>
        /// <exception cref="ArgumentException">当队列名称为空时抛出</exception>
        public IRabbitConsumerBuilder AddConsumer(string queue, Func<IServiceProvider, RecieveResult, Task> onMessageRecieved)
        {
            if (string.IsNullOrEmpty(queue))
                throw new ArgumentException("queue cannot be empty", nameof(queue));

            Services.AddSingleton<IRabbitConsumerProvider>(serviceProvider =>
            {
                return new DefaultRabbitConsumerProvider(serviceProvider, queue, _rabbitConsumerOptions, async result =>
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        await onMessageRecieved?.Invoke(scope.ServiceProvider, result);
                    }
                });
            });

            return this;
        }

        /// <summary>
        /// 添加交换机监听，注册 <see cref="DefaultRabbitConsumerProvider"/> 到 DI 容器。
        /// 每次消费消息时会创建新的 DI Scope，确保 Scoped 服务在消费回调中正常工作。
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="onMessageRecieved">消息接收回调，接收 <see cref="IServiceProvider"/> 和 <see cref="RecieveResult"/> 两个参数</param>
        /// <returns>消费者构建器，支持链式调用</returns>
        /// <exception cref="ArgumentException">当交换机名称或队列名称为空时抛出</exception>
        public IRabbitConsumerBuilder AddConsumer(string exchange, string queue, Func<IServiceProvider, RecieveResult, Task> onMessageRecieved)
        {
            if (string.IsNullOrEmpty(exchange))
                throw new ArgumentException("exchange cannot be empty", nameof(exchange));

            if (string.IsNullOrEmpty(queue))
                throw new ArgumentException("queue cannot be empty", nameof(queue));

            Services.AddSingleton<IRabbitConsumerProvider>(serviceProvider =>
            {
                return new DefaultRabbitConsumerProvider(serviceProvider, exchange, queue, _rabbitConsumerOptions, async result =>
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        await onMessageRecieved?.Invoke(scope.ServiceProvider, result);
                    }
                });
            });

            return this;
        }
    }
}
