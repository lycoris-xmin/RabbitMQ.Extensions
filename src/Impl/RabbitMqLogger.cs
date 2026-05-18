using Microsoft.Extensions.Logging;
using System;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// RabbitMQ 日志记录器，包装 Microsoft.Extensions.Logging.ILogger 实现 IRabbitMqLogger 接口
    /// </summary>
    public sealed class RabbitMqLogger : IRabbitMqLogger
    {
        private readonly ILogger _logger;

        /// <summary>
        /// 使用 ILogger 实例初始化日志记录器
        /// </summary>
        /// <param name="logger">Microsoft.Extensions.Logging.ILogger 实例</param>
        public RabbitMqLogger(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 记录信息级别日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Info(string message) => _logger.LogInformation(message);

        /// <summary>
        /// 记录警告级别日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Warn(string message) => _logger.LogWarning(message);

        /// <summary>
        /// 记录警告级别日志并附带异常信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="ex">异常信息</param>
        public void Warn(string message, Exception ex) => _logger.LogWarning(ex, message);

        /// <summary>
        /// 记录错误级别日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Error(string message) => _logger.LogError(message);

        /// <summary>
        /// 记录错误级别日志并附带异常信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="ex">异常信息</param>
        public void Error(string message, Exception ex) => _logger.LogError(ex, message);
    }
}
