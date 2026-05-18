using Lycoris.RabbitMQ.Extensions.Builder;
using Lycoris.RabbitMQ.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// 生产者工厂，负责创建和缓存 <see cref="IRabbitClientProducer"/> 实例，确保同一名称的生产者全局唯一
    /// </summary>
    public sealed class RabbitProducerFactory : IRabbitProducerFactory
    {
        private readonly ConcurrentDictionary<string, IRabbitClientProducer> _clientProducers;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 使用服务提供者初始化生产者工厂
        /// </summary>
        /// <param name="serviceProvider">依赖注入服务提供者</param>
        public RabbitProducerFactory(IServiceProvider serviceProvider)
        {
            _clientProducers = new ConcurrentDictionary<string, IRabbitClientProducer>();
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 根据配置名称创建或获取已缓存的生产者实例，不存在时自动创建
        /// </summary>
        /// <param name="name">生产者配置名称</param>
        /// <returns>对应名称的 <see cref="IRabbitClientProducer"/> 实例</returns>
        /// <exception cref="InvalidOperationException">指定名称的配置不存在或 Hosts 未配置时抛出</exception>
        public IRabbitClientProducer Create(string name)
        {
            var rabbitProducerOptions = RabbitMQOptionsStore.GetRabbitProducerOptions(name);

            if (rabbitProducerOptions == null || rabbitProducerOptions.Hosts == null || rabbitProducerOptions.Hosts.Length == 0)
                throw new InvalidOperationException($"{nameof(RabbitProducerOption)} named '{name}' is not configured");

            lock (_clientProducers)
            {
                if (_clientProducers.TryGetValue(name, out IRabbitClientProducer rabbitClientProducer) && rabbitClientProducer != null)
                {
                    if (rabbitClientProducer is RabbitClientProducer producer && !producer.Disposed)
                        return rabbitClientProducer;
                }

                rabbitClientProducer = new RabbitClientProducer(rabbitProducerOptions);
                _clientProducers.AddOrUpdate(name, rabbitClientProducer, (n, b) => rabbitClientProducer);

                return rabbitClientProducer;
            }
        }
    }
}
