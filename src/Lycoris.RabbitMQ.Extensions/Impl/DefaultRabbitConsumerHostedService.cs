using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class DefaultRabbitConsumerHostedService : IHostedService
    {
        private readonly IRabbitConsumerFactory _consumerFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerFactory"></param>
        public DefaultRabbitConsumerHostedService(IRabbitConsumerFactory consumerFactory)
        {
            _consumerFactory = consumerFactory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken) => _consumerFactory.ManualStartListenAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken) => _consumerFactory.ManualStopListenAsync(cancellationToken);
    }
}

