using System;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// RabbitMQ 日志记录器接口，封装日志输出功能
    /// </summary>
    public interface IRabbitMqLogger
    {
        /// <summary>
        /// 记录信息级别日志
        /// </summary>
        /// <param name="message">日志消息内容</param>
        void Info(string message);

        /// <summary>
        /// 记录警告级别日志
        /// </summary>
        /// <param name="message">日志消息内容</param>
        void Warn(string message);

        /// <summary>
        /// 记录警告级别日志并附带异常信息
        /// </summary>
        /// <param name="message">日志消息内容</param>
        /// <param name="ex">关联的异常对象</param>
        void Warn(string message, Exception ex);

        /// <summary>
        /// 记录错误级别日志
        /// </summary>
        /// <param name="message">日志消息内容</param>
        void Error(string message);

        /// <summary>
        /// 记录错误级别日志并附带异常信息
        /// </summary>
        /// <param name="message">日志消息内容</param>
        /// <param name="ex">关联的异常对象</param>
        void Error(string message, Exception ex);
    }
}
