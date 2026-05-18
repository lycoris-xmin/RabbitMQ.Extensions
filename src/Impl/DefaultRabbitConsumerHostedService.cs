using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// 默认消费者托管服务，实现 IHostedService，在应用启动时自动开始所有消费者监听，停止时自动停止
    /// </summary>
    public sealed class DefaultRabbitConsumerHostedService : IHostedService
    {
        private readonly IRabbitConsumerFactory _consumerFactory;

        /// <summary>
        /// 使用消费者工厂初始化托管服务
        /// </summary>
        /// <param name="consumerFactory">消费者工厂实例</param>
        public DefaultRabbitConsumerHostedService(IRabbitConsumerFactory consumerFactory)
        {
            _consumerFactory = consumerFactory;
        }

        /// <summary>
        /// 应用启动时自动启动所有消费者监听
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        public Task StartAsync(CancellationToken cancellationToken) => _consumerFactory.ManualStartListenAsync();

        /// <summary>
        /// 应用停止时停止所有消费者监听
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        public Task StopAsync(CancellationToken cancellationToken) => _consumerFactory.ManualStopListenAsync(cancellationToken);
    }
}
