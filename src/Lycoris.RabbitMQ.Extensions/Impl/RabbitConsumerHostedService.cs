using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RabbitConsumerHostedService : IHostedService
    {
        private readonly IRabbitConsumerFactory _consumerFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerFactory"></param>
        public RabbitConsumerHostedService(IRabbitConsumerFactory consumerFactory)
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

