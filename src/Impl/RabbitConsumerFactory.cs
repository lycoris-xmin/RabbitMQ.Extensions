using Lycoris.RabbitMQ.Extensions.Builder.Consumer;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// 消费者工厂，管理已注册的所有消费者监听器的启动、停止和生命周期
    /// </summary>
    public sealed class RabbitConsumerFactory : IRabbitConsumerFactory
    {
        private readonly IEnumerable<IRabbitConsumerProvider> _rabbitConsumerProviders;
        private readonly IRabbitMqLogger _logger;

        /// <summary>
        /// 使用日志工厂和消费者提供者列表初始化消费者工厂
        /// </summary>
        /// <param name="factory">日志工厂</param>
        /// <param name="rabbitConsumerProviders">已注册的消费者提供者列表</param>
        public RabbitConsumerFactory(ILoggerFactory factory, IEnumerable<IRabbitConsumerProvider> rabbitConsumerProviders)
        {
            _rabbitConsumerProviders = rabbitConsumerProviders;
            _logger = factory != null ? new RabbitMqLogger(factory.CreateLogger<DefaultRabbitConsumerHostedService>()) : null;
        }

        /// <summary>
        /// 启动所有已注册的消费者监听
        /// </summary>
        public async Task ManualStartListenAsync()
        {
            foreach (var provider in _rabbitConsumerProviders)
            {
                await provider.ListenAsync();
                _logger?.Info($"rabbitmq consumer listen:{provider}");
            }
        }

        /// <summary>
        /// 启动指定队列的消费者监听
        /// </summary>
        /// <param name="queue">队列名称</param>
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
        /// 启动指定交换机和队列的消费者监听
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
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
        /// 停止所有已注册的消费者监听
        /// </summary>
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
        /// 停止所有监听并注册取消令牌回调，当取消令牌触发时再次停止以确保资源释放
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        public Task ManualStopListenAsync(CancellationToken cancellationToken)
        {
            foreach (var provider in _rabbitConsumerProviders)
            {
                provider.Dispose();
                _logger?.Info($"rabbitmq consumer stoped:{provider}");
            }

            cancellationToken.Register(() =>
            {
                foreach (var provider in _rabbitConsumerProviders)
                {
                    provider.Dispose();
                }
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// 停止指定队列的消费者监听
        /// </summary>
        /// <param name="queue">队列名称</param>
        public Task ManualStopListenAsync(string queue)
        {
            var provider = _rabbitConsumerProviders.Where(x => string.IsNullOrEmpty(x.Exchange) && x.Queue == queue).SingleOrDefault();
            provider?.Dispose();
            _logger?.Info($"rabbitmq consumer stoped:{provider}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 停止指定队列的监听并注册取消令牌回调
        /// </summary>
        /// <param name="queue">队列名称</param>
        /// <param name="cancellationToken">取消令牌</param>
        public Task ManualStopListenAsync(string queue, CancellationToken cancellationToken)
        {
            var provider = _rabbitConsumerProviders.Where(x => string.IsNullOrEmpty(x.Exchange) && x.Queue == queue).SingleOrDefault();
            provider?.Dispose();
            _logger?.Info($"rabbitmq consumer stoped:{provider}");

            cancellationToken.Register(() =>
            {
                var p = _rabbitConsumerProviders.Where(x => string.IsNullOrEmpty(x.Exchange) && x.Queue == queue).SingleOrDefault();
                p?.Dispose();
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// 停止指定交换机和队列的消费者监听
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        public Task ManualStopListenAsync(string exchange, string queue)
        {
            var provider = _rabbitConsumerProviders.Where(x => x.Exchange == exchange && x.Queue == queue).SingleOrDefault();
            provider?.Dispose();
            _logger?.Info($"rabbitmq consumer stoped:{provider}");

            return Task.CompletedTask;
        }

        /// <summary>
        /// 停止指定交换机和队列的监听并注册取消令牌回调
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="cancellationToken">取消令牌</param>
        public Task ManualStopListenAsync(string exchange, string queue, CancellationToken cancellationToken)
        {
            var provider = _rabbitConsumerProviders.Where(x => x.Exchange == exchange && x.Queue == queue).SingleOrDefault();
            provider?.Dispose();
            _logger?.Info($"rabbitmq consumer stoped:{provider}");

            cancellationToken.Register(() =>
            {
                var p = _rabbitConsumerProviders.Where(x => x.Exchange == exchange && x.Queue == queue).SingleOrDefault();
                p?.Dispose();
            });

            return Task.CompletedTask;
        }
    }
}
