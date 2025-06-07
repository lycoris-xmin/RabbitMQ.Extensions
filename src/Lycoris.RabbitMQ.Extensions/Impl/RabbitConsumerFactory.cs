using Lycoris.RabbitMQ.Extensions.Builder.Consumer;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RabbitConsumerFactory : IRabbitConsumerFactory
    {
        private readonly IEnumerable<IRabbitConsumerProvider> _rabbitConsumerProviders;
        private readonly IRabbitMqLogger _logger;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="rabbitConsumerProviders"></param>
        public RabbitConsumerFactory(ILoggerFactory factory, IEnumerable<IRabbitConsumerProvider> rabbitConsumerProviders)
        {
            _rabbitConsumerProviders = rabbitConsumerProviders;
            _logger = factory != null ? new RabbitMqLogger(factory.CreateLogger<DefaultRabbitConsumerHostedService>()) : null;
        }

        /// <summary>
        /// 启动监听
        /// </summary>
        /// <returns></returns>
        public async Task ManualStartListenAsync()
        {
            foreach (var provider in _rabbitConsumerProviders)
            {
                await provider.ListenAsync();
                _logger?.Info($"rabbitmq consumer listen:{provider}");
            }
        }

        /// <summary>
        /// 启动监听
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        public async Task ManualStartListenAsync(string queue)
        {
            foreach (var provider in _rabbitConsumerProviders)
            {
                if (string.IsNullOrEmpty(provider.Exchange) && provider.Queue == queue)
                {
                    await provider.ListenAsync();
                    _logger?.Info($"rabbitmq consumer listen:{provider}");
                }
            }
        }

        /// <summary>
        /// 启动监听
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        public async Task ManualStartListenAsync(string exchange, string queue)
        {
            foreach (var provider in _rabbitConsumerProviders)
            {
                if (provider.Exchange == exchange && provider.Queue == queue)
                {
                    await provider.ListenAsync();
                    _logger?.Info($"rabbitmq consumer listen:{provider}");
                }
            }
        }

        /// <summary>
        /// 暂停监听
        /// </summary>
        /// <returns></returns>
        public Task ManualStopListenAsync()
        {
            foreach (var provider in _rabbitConsumerProviders)
            {
                provider.Dispose();
                _logger?.Info($"rabbitmq consumer stoped:{provider}");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 暂停监听
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task ManualStopListenAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() =>
            {
                foreach (var provider in _rabbitConsumerProviders)
                {
                    provider.Dispose();
                    _logger?.Info($"rabbitmq consumer stoped:{provider}");
                }
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// 暂停监听
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        public Task ManualStopListenAsync(string queue)
        {
            var provider = _rabbitConsumerProviders.Where(x => string.IsNullOrEmpty(x.Exchange) && x.Queue == queue).SingleOrDefault();
            provider?.Dispose();
            _logger?.Info($"rabbitmq consumer stoped:{provider}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 暂停监听
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task ManualStopListenAsync(string queue, CancellationToken cancellationToken)
        {
            cancellationToken.Register(() =>
            {
                var provider = _rabbitConsumerProviders.Where(x => string.IsNullOrEmpty(x.Exchange) && x.Queue == queue).SingleOrDefault();
                provider?.Dispose();
                _logger?.Info($"rabbitmq consumer stoped:{provider}");
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// 暂停监听
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        public Task ManualStopListenAsync(string exchange, string queue)
        {
            var provider = _rabbitConsumerProviders.Where(x => x.Exchange == exchange && x.Queue == queue).SingleOrDefault();
            provider?.Dispose();
            _logger?.Info($"rabbitmq consumer stoped:{provider}");

            return Task.CompletedTask;
        }

        /// <summary>
        /// 暂停监听
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task ManualStopListenAsync(string exchange, string queue, CancellationToken cancellationToken)
        {
            cancellationToken.Register(() =>
            {
                var provider = _rabbitConsumerProviders.Where(x => x.Exchange == exchange && x.Queue == queue).SingleOrDefault();
                provider?.Dispose();
                _logger?.Info($"rabbitmq consumer stoped:{provider}");
            });

            return Task.CompletedTask;
        }
    }
}
