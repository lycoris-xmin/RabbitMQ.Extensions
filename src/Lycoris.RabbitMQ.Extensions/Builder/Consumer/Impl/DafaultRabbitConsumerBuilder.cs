using Lycoris.RabbitMQ.Extensions.DataModel;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Lycoris.RabbitMQ.Extensions.Builder.Consumer.Impl
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class DafaultRabbitConsumerBuilder : IRabbitConsumerBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// 
        /// </summary>
        private readonly Options.RabbitConsumerOption _rabbitConsumerOptions;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="rabbitConsumerOptions"></param>
        public DafaultRabbitConsumerBuilder(IServiceCollection services, Options.RabbitConsumerOption rabbitConsumerOptions)
        {
            Services = services;
            _rabbitConsumerOptions = rabbitConsumerOptions;
        }

        /// <summary>
        /// 添加队列监听
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="onMessageRecieved"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public IRabbitConsumerBuilder AddListener(string queue, Action<IServiceProvider, RecieveResult> onMessageRecieved)
        {
            if (string.IsNullOrEmpty(queue))
                throw new ArgumentException("queue cann't be empty", nameof(queue));

            Services.AddSingleton<IRabbitConsumerProvider>(serviceProvider =>
            {
                return new DefaultRabbitConsumerProvider(queue, _rabbitConsumerOptions, result =>
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        onMessageRecieved?.Invoke(scope.ServiceProvider, result);
                    }
                });
            });

            return this;
        }

        /// <summary>
        /// 添加交换机监听
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        /// <param name="onMessageRecieved"></param>
        /// <returns></returns>
        public IRabbitConsumerBuilder AddListener(string exchange, string queue, Action<IServiceProvider, RecieveResult> onMessageRecieved)
        {
            if (string.IsNullOrEmpty(exchange))
                throw new ArgumentException("exchange cann't be empty", nameof(exchange));

            if (string.IsNullOrEmpty(queue))
                throw new ArgumentException("queue cann't be empty", nameof(queue));

            Services.AddSingleton<IRabbitConsumerProvider>(serviceProvider =>
            {
                return new DefaultRabbitConsumerProvider(exchange, queue, _rabbitConsumerOptions, result =>
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        onMessageRecieved?.Invoke(scope.ServiceProvider, result);
                    }
                });
            });

            return this;
        }
    }
}
