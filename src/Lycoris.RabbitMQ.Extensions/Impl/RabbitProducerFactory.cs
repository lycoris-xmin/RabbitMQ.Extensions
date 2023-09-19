using Lycoris.RabbitMQ.Extensions.Builder;
using Lycoris.RabbitMQ.Extensions.Options;
using System.Collections.Concurrent;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// ctor
    /// </summary>
    public sealed class RabbitProducerFactory : IRabbitProducerFactory
    {
        private readonly ConcurrentDictionary<string, IRabbitClientProducer> _clientProducers;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        public RabbitProducerFactory(IServiceProvider serviceProvider)
        {
            _clientProducers = new ConcurrentDictionary<string, IRabbitClientProducer>();
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 创建客户端发送
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IRabbitClientProducer Create(string name)
        {
            var rabbitProducerOptions = RabbitMQOptionsStore.GetRabbitProducerOptions(name);

            if (rabbitProducerOptions == null || rabbitProducerOptions.Hosts == null || rabbitProducerOptions.Hosts.Length == 0)
                throw new InvalidOperationException($"{nameof(RabbitProducerOptions)} named '{name}' is not configured");

            lock (_clientProducers)
            {
                if (_clientProducers.TryGetValue(name, out IRabbitClientProducer? rabbitClientProducer) && rabbitClientProducer != null)
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
