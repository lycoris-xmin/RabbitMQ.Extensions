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
    /// 
    /// </summary>
    public class BaseRabbit : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        protected IServiceProvider ServiceProvider { get; private set; }

        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        readonly IRabbitMqEventHandler _eventHandler;
        readonly string[] hostAndPorts;
        IConnection connection;
        bool isDisposed = false;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="hostAndPorts"></param>
        /// <exception cref="ArgumentException"></exception>
        protected BaseRabbit(params string[] hostAndPorts)
        {
            if (hostAndPorts == null || hostAndPorts.Length == 0)
                throw new ArgumentException("invalid host and ports！", nameof(hostAndPorts));

            this.hostAndPorts = hostAndPorts.Select(f => f).ToArray();
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="hostAndPorts"></param>
        /// <exception cref="ArgumentException"></exception>
        protected BaseRabbit(IServiceProvider serviceProvider, params string[] hostAndPorts)
        {
            if (hostAndPorts == null || hostAndPorts.Length == 0)
                throw new ArgumentException("invalid host and ports！", nameof(hostAndPorts));

            this.ServiceProvider = serviceProvider;

            this.hostAndPorts = hostAndPorts.Select(f => f).ToArray();

            this._eventHandler = serviceProvider.GetService<IRabbitMqEventHandler>();
        }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// 账号
        /// </summary>
        public string UserName { get; set; } = ConnectionFactory.DefaultUser;

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = ConnectionFactory.DefaultPass;

        /// <summary>
        /// 虚拟机
        /// </summary>
        public string VirtualHost { get; set; } = ConnectionFactory.DefaultVHost;

        /// <summary>
        /// 连接是否打开
        /// </summary>
        public bool IsOpen { get { return connection != null && connection.IsOpen; } }

        /// <summary>
        /// 释放
        /// </summary>
        public virtual void Dispose()
        {
            isDisposed = true;
            CloseAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public async Task CloseAsync()
        {
            if (connection != null && connection.IsOpen)
                await connection?.CloseAsync();

            connection?.Dispose();
        }

        #region protected
        /// <summary>
        /// 获取rabbitmq的连接
        /// </summary>
        /// <returns></returns>
        protected async Task<IChannel> GetChannelAsync()
        {
            if (isDisposed)
                throw new ObjectDisposedException(GetType().Name);

            await semaphore.WaitAsync();

            try
            {
                if (connection == null)
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

                    connection = await factory.CreateConnectionAsync(amqpList);

                    connection.ConnectionShutdownAsync += ConnectionShutdownAsync;
                    connection.CallbackExceptionAsync += CallbackExceptionAsync;
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
        /// 准备连接
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="exchange"></param>
        /// <param name="options"></param>
        /// <exception cref="NotSupportedException"></exception>
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
                        //这里的exclusive参数表示是否与当前的channel绑定
                        await channel.QueueDeclareAsync(t.Queue, t.Options?.Durable ?? options.Durable, false, t.Options?.AutoDelete ?? options.AutoDelete, t.Options?.Arguments ?? options.Arguments ?? new Dictionary<string, object>());

                        await channel.QueueBindAsync(t.Queue, exchange, t.Route ?? "", options.Arguments ?? new Dictionary<string, object>());
                    }
                }
            }
        }

        /// <summary>
        /// 准备连接
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="queue"></param>
        /// <param name="options"></param>
        protected static async Task PrepareQueueChannelAsync(IChannel channel, string queue, QueueOption options)
        {
            //这里的exclusive参数表示是否与当前的channel绑定
            await channel.QueueDeclareAsync(queue, options.Durable, false, options.AutoDelete, options.Arguments ?? new Dictionary<string, object>());
        }

        /// <summary>
        /// 回调异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
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
        /// 断开连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task ConnectionShutdownAsync(object sender, ShutdownEventArgs e)
        {
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
        /// 
        /// </summary>
        /// <returns></returns>
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
