using Lycoris.RabbitMQ.Extensions.Impl;
using Lycoris.RabbitMQ.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// RabbitMQ 扩展构建器，用于配置连接参数、注册生产者和消费者服务、添加事件处理器和健康检查。
    /// 继承自 <see cref="RabbitOption"/>，提供 RabbitMQ 客户端所需的全部基础配置能力。
    /// </summary>
    public sealed class RabbitMqBuilder : RabbitOption
    {
        internal readonly string configureName;
        internal readonly IServiceCollection services;

        /// <summary>
        /// 使用服务集合初始化 <see cref="RabbitMqBuilder"/> 构建器。
        /// 自动生成唯一的配置名称以支持多实例场景。
        /// </summary>
        /// <param name="services">依赖注入服务集合</param>
        public RabbitMqBuilder(IServiceCollection services)
        {
            this.services = services;
            this.configureName = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 禁用消费者自启动监听。
        /// 设为 <c>true</c> 则禁用消费者 HostedService 自动启动，需手动调用 <see cref="IRabbitConsumerFactory.ManualStartListenAsync()"/> 启动监听。
        /// </summary>
        public bool DisableRabbitConsumerHostedListen { get; set; } = false;

        /// <summary>
        /// 注册 RabbitMQ 管理 API 服务（<see cref="IRabbitMqApiService"/>），
        /// 用于通过 RabbitMQ Management HTTP API 查询节点状态、队列信息和消息统计等。
        /// </summary>
        public void AddApiService() => services.TryAddSingleton<IRabbitMqApiService, RabbitMqApiService>();

        /// <summary>
        /// 注册自定义事件处理器，替换默认的控制台输出。
        /// 实现 <see cref="IRabbitMqEventHandler"/> 接口即可捕获连接状态变更、消息收发异常等事件。
        /// </summary>
        /// <typeparam name="T">实现 <see cref="IRabbitMqEventHandler"/> 的事件处理器类型</typeparam>
        public void AddEventHandler<T>() where T : class, IRabbitMqEventHandler => services.AddTransient<IRabbitMqEventHandler, T>();

        /// <summary>
        /// 注册健康检查服务（<see cref="RabbitMqHealthCheckService"/>），
        /// 用于检测 RabbitMQ 连接是否正常，可结合 ASP.NET Core 健康检查中间件使用。
        /// </summary>
        public void AddHealthCheck() => services.TryAddSingleton<RabbitMqHealthCheckService>();
    }
}
