using Lycoris.Base.Logging;
using Lycoris.RabbitMQ.Extensions.Builder.Consumer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RabbitConsumerHostedService : IHostedService
    {
        private readonly ILogger? logger;
        private readonly ILycorisLogger? LycorisLogger;
        private readonly IEnumerable<IRabbitConsumerProvider> _rabbitConsumerProviders;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="rabbitConsumerProviders"></param>
        public RabbitConsumerHostedService(IServiceProvider serviceProvider,
                                           IEnumerable<IRabbitConsumerProvider> rabbitConsumerProviders)
        {
            var factory = serviceProvider.GetService<ILycorisLoggerFactory>();
            if (factory != null)
                LycorisLogger = factory?.CreateLogger<RabbitConsumerHostedService>();
            else
                logger = serviceProvider.GetService<ILogger<RabbitConsumerHostedService>>();

            _rabbitConsumerProviders = rabbitConsumerProviders;
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

                if (LycorisLogger != null)
                {
                    LycorisLogger.Info($"rabbitmq consumer listen:{provider}");
                }
                else if (logger != null)
                {
                    logger.LogInformation($"rabbitmq consumer listen:{provider}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"rabbitmq consumer listen:{provider}");
                    Console.ResetColor();
                }
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

                    if (LycorisLogger != null)
                    {
                        LycorisLogger.Info($"rabbitmq consumer stoped:{provider}");

                    }
                    else if (logger != null)
                    {
                        logger.LogInformation($"rabbitmq consumer stoped:{provider}");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine($"rabbitmq consumer stoped:{provider}");
                        Console.ResetColor();
                    }
                }
            });

            await Task.CompletedTask;
        }
    }
}

