using Microsoft.Extensions.Logging;
using System;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RabbitMqLogger : IRabbitMqLogger
    {
        private readonly ILogger _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public RabbitMqLogger(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message) => _logger.LogInformation(message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Warn(string message) => _logger.LogWarning(message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Warn(string message, Exception ex) => _logger.LogWarning(ex, message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Error(string message) => _logger.LogError(message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Error(string message, Exception ex) => _logger.LogError(ex, message);
    }
}
