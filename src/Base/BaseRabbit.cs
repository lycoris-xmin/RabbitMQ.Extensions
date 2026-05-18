using Lycoris.RabbitMQ.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Base
{
    /// <summary>
    /// RabbitMQ 连接基础类，提供连接管理、Channel 获取、队列与交换机声明等基础设施。实现 IDisposable 和 IAsyncDisposable
    /// </summary>
    public class BaseRabbit : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// 依赖注入服务提供者
        /// </summary>
        protected IServiceProvider ServiceProvider { get; private set; }

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        readonly IRabbitMqEventHandler _eventHandler;
        readonly string[] hostAndPorts;
        IConnection connection;
        bool isDisposed = false;

        /// <summary>
        /// 使用指定主机列表初始化 RabbitMQ 连接
        /// </summary>
        /// <param name="hostAndPorts">主机地址列表，格式为 "host:port" 或 "host"</param>
        /// <exception cref="ArgumentException">主机列表为空或为 null 时抛出</exception>
        protected BaseRabbit(params string[] hostAndPorts)
        {
            if (hostAndPorts == null || hostAndPorts.Length == 0)
                throw new ArgumentException("invalid host and ports！", nameof(hostAndPorts));

            this.hostAndPorts = hostAndPorts.Select(f => f).ToArray();
        }

        /// <summary>
        /// 使用服务提供者和主机列表初始化 RabbitMQ 连接
        /// </summary>
        /// <param name="serviceProvider">依赖注入服务提供者，用于获取事件处理器等依赖项</param>
        /// <param name="hostAndPorts">主机地址列表，格式为 "host:port" 或 "host"</param>
        /// <exception cref="ArgumentException">主机列表为空或为 null 时抛出</exception>
        protected BaseRabbit(IServiceProvider serviceProvider, params string[] hostAndPorts)
        {
            if (hostAndPorts == null || hostAndPorts.Length == 0)
                throw new ArgumentException("invalid host and ports！", nameof(hostAndPorts));

            this.ServiceProvider = serviceProvider;

            this.hostAndPorts = hostAndPorts.Select(f => f).ToArray();

            this._eventHandler = serviceProvider.GetService<IRabbitMqEventHandler>();
        }

        /// <summary>
        /// AMQP 协议端口，默认 5672
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// 登录账号，默认 "guest"
        /// </summary>
        public string UserName { get; set; } = ConnectionFactory.DefaultUser;

        /// <summary>
        /// 登录密码，默认 "guest"
        /// </summary>
        public string Password { get; set; } = ConnectionFactory.DefaultPass;

        /// <summary>
        /// 虚拟主机，默认 "/"
        /// </summary>
        public string VirtualHost { get; set; } = ConnectionFactory.DefaultVHost;

        /// <summary>
        /// 当前连接是否处于打开状态
        /// </summary>
        public bool IsOpen { get { return connection != null && connection.IsOpen; } }

        /// <summary>
        /// 连接断开后的最大重连尝试次数，默认 3
        /// </summary>
        protected int MaxReconnectAttempts { get; set; } = 3;

        /// <summary>
        /// 重连基础延迟（毫秒），实际延迟按指数退避计算，默认 1000
        /// </summary>
        protected int ReconnectDelay { get; set; } = 1000;

        /// <summary>
        /// 释放所有资源（同步）
        /// </summary>
        public virtual void Dispose()
        {
            isDisposed = true;
            CloseAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 异步释放所有资源
        /// </summary>
        public virtual async ValueTask DisposeAsync()
        {
            isDisposed = true;
            await CloseAsync();
        }

        /// <summary>
        /// 异步关闭连接
        /// </summary>
        public async Task CloseAsync()
        {
            if (connection != null && connection.IsOpen)
                await connection?.CloseAsync();

            connection?.Dispose();
        }

        #region protected
        /// <summary>
        /// 获取 RabbitMQ 的 Channel，含自动重连和指数退避机制
        /// </summary>
        /// <returns>打开的 AMQP 信道</returns>
        /// <exception cref="ObjectDisposedException">当前对象已释放时抛出</exception>
        /// <exception cref="ConnectFailureException">连接丢失且重连失败时抛出</exception>
        protected async Task<IChannel> GetChannelAsync()
        {
            if (isDisposed)
                throw new ObjectDisposedException(GetType().Name);

            await semaphore.WaitAsync();

            try
            {
                if (connection == null || !connection.IsOpen)
                {
                    connection?.Dispose();
                    connection = null;

                    for (int attempt = 0; attempt < MaxReconnectAttempts; attempt++)
                    {
                        try
                        {
                            connection = await CreateConnectionInternalAsync();

                            connection.ConnectionShutdownAsync += ConnectionShutdownAsync;
                            connection.CallbackExceptionAsync += CallbackExceptionAsync;
                            break;
                        }
                        catch (Exception)
                        {
                            if (attempt == MaxReconnectAttempts - 1)
                                throw;

                            await Task.Delay(ReconnectDelay * (int)Math.Pow(2, attempt));
                        }
                    }
                }
            }
            finally
            {
                semaphore.Release();
            }

            if (!IsOpen)
                throw new ConnectFailureException("connection lost", null);

            return await connection.CreateChannelAsync();
        }

        /// <summary>
        /// 创建 AMQP 连接（内部实现）
        /// </summary>
        /// <returns>AMQP 连接实例</returns>
        private async Task<IConnection> CreateConnectionInternalAsync()
        {
            var array = hostAndPorts.Select(f => f.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries))
                .Select(f => (f.FirstOrDefault(), f.Length > 1 ? int.Parse(f.ElementAt(1)) : Port))
                .ToArray();

            var amqpList = new List<AmqpTcpEndpoint>();
            amqpList.AddRange(array.Select(f => new AmqpTcpEndpoint(f.Item1, f.Item2)));

            var factory = new ConnectionFactory
            {
                Port = Port,
                UserName = UserName,
                VirtualHost = VirtualHost,
                Password = Password
            };

            return await factory.CreateConnectionAsync(amqpList);
        }

        /// <summary>
        /// 声明交换机、队列并绑定路由关系
        /// </summary>
        /// <param name="channel">AMQP 信道</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="options">交换机和队列配置选项</param>
        /// <exception cref="NotSupportedException">未指定交换机类型时抛出</exception>
        protected async Task PrepareExchangeChannelAsync(IChannel channel, string exchange, ExchangeQueueOptions options)
        {
            if (options.Type == RabbitExchangeType.None)
                throw new NotSupportedException($"{nameof(RabbitExchangeType)} must be specified");

            var type = options.Type != RabbitExchangeType.Delayed ? options.Type.ToString().ToLower() : "x-delayed-message";

            await channel.ExchangeDeclareAsync(exchange, type, options.Durable, options.AutoDelete, options.Arguments ?? new Dictionary<string, object>());

            if (options.RouteQueues != null)
            {
                foreach (var t in options.RouteQueues)
                {
                    if (!string.IsNullOrEmpty(t.Queue))
                    {
                        var queueArgs = BuildQueueArguments(t.Options, options.Arguments, options.DeadLetterExchange);

                        //这里的exclusive参数表示是否与当前的channel绑定
                        await channel.QueueDeclareAsync(t.Queue, t.Options?.Durable ?? options.Durable, false, t.Options?.AutoDelete ?? options.AutoDelete, queueArgs);

                        await channel.QueueBindAsync(t.Queue, exchange, t.Route ?? "", options.Arguments ?? new Dictionary<string, object>());
                    }
                }
            }
        }

        /// <summary>
        /// 声明普通队列（不含交换机绑定）
        /// </summary>
        /// <param name="channel">AMQP 信道</param>
        /// <param name="queue">队列名称</param>
        /// <param name="options">队列配置选项</param>
        protected static async Task PrepareQueueChannelAsync(IChannel channel, string queue, QueueOption options)
        {
            var queueArgs = BuildQueueArguments(options, null, options?.DeadLetterExchange);

            //这里的exclusive参数表示是否与当前的channel绑定
            await channel.QueueDeclareAsync(queue, options.Durable, false, options.AutoDelete, queueArgs);
        }

        /// <summary>
        /// 构建队列参数（含死信队列配置）
        /// </summary>
        /// <param name="queueOption">队列选项</param>
        /// <param name="fallbackArgs">备用参数字典</param>
        /// <param name="deadLetterExchange">死信交换机配置</param>
        /// <returns>合并后的队列参数字典</returns>
        private static Dictionary<string, object> BuildQueueArguments(QueueOption queueOption, IDictionary<string, object> fallbackArgs, DeadLetterExchangeOption deadLetterExchange)
        {
            var args = new Dictionary<string, object>(queueOption?.Arguments ?? fallbackArgs ?? new Dictionary<string, object>());

            var dlx = queueOption?.DeadLetterExchange ?? deadLetterExchange;
            if (dlx != null && !string.IsNullOrEmpty(dlx.Exchange))
            {
                args["x-dead-letter-exchange"] = dlx.Exchange;
                if (!string.IsNullOrEmpty(dlx.RoutingKey))
                    args["x-dead-letter-routing-key"] = dlx.RoutingKey;
            }

            return args;
        }

        /// <summary>
        /// 处理 RabbitMQ 回调异常事件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">回调异常事件参数</param>
        private async Task CallbackExceptionAsync(object sender, CallbackExceptionEventArgs e)
        {
            try
            {
                await this._eventHandler?.CallbackExceptionAsync(sender, e);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// 处理连接断开事件，清空当前连接引用并委派给事件处理器
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">连接关闭事件参数</param>
        private async Task ConnectionShutdownAsync(object sender, ShutdownEventArgs e)
        {
            connection = null;

            try
            {
                await this._eventHandler?.ConnectionShutdownAsync(sender, e);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
        #endregion

        /// <summary>
        /// 返回当前连接的描述信息（主机、虚拟主机、用户名）
        /// </summary>
        /// <returns>格式为 "host=hosts;virtualHost=...;username=...;" 的连接描述字符串</returns>
        public override string ToString()
        {
            var array = hostAndPorts.Select(f => f.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries))
                                    .Select(f => (f.FirstOrDefault(), f.Length > 1 ? int.Parse(f.ElementAt(1)) : Port))
                                    .ToArray();

            string hosts = string.Join(",", array.Select(f => $"{f.Item1}:{f.Item2}"));

            return $"host={hosts};virtualHost={VirtualHost};username={UserName};";
        }
    }
}
