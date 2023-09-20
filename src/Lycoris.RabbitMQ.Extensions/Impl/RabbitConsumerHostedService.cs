using Lycoris.RabbitMQ.Extensions.Builder.Consumer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RabbitConsumerHostedService : IHostedService
    {
        private readonly IEnumerable<IRabbitConsumerProvider> _rabbitConsumerProviders;
        private readonly IRabbitMqLogger _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="rabbitConsumerProviders"></param>
        public RabbitConsumerHostedService(ILoggerFactory factory, IEnumerable<IRabbitConsumerProvider> rabbitConsumerProviders)
        {
            _rabbitConsumerProviders = rabbitConsumerProviders;
            _logger = factory != null ? new RabbitMqLogger(factory.CreateLogger<RabbitConsumerHostedService>()) : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var provider in _rabbitConsumerProviders)
            {
                await provider.ListenAsync();
                _logger?.Info($"rabbitmq consumer listen:{provider}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() =>
            {
                foreach (var provider in _rabbitConsumerProviders)
                {
                    provider.Dispose();
                    _logger?.Info($"rabbitmq consumer stoped:{provider}");
                }
            });

            await Task.CompletedTask;
        }
    }
}

