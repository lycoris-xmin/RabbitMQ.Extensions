using Lycoris.RabbitMQ.Extensions.DataModel;
using Lycoris.RabbitMQ.Extensions.Impl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Builder.Consumer.Impl
{
    /// <summary>
    /// 默认消费者提供者，封装单个 <see cref="RabbitConsumer"/> 实例及其监听生命周期管理。
    /// 负责创建 RabbitMQ 消费者、配置队列绑定选项、启动/停止监听以及释放资源。
    /// </summary>
    internal sealed class DefaultRabbitConsumerProvider : IRabbitConsumerProvider
    {
        private readonly Options.RabbitConsumerOption _rabbitConsumerOptions;
        private ListenResult listenResult;
        private bool disposed = false;
        private readonly Func<RecieveResult, Task> recieved;

        /// <summary>
        /// 使用队列名称和消费者配置初始化提供者，创建不绑定交换机的队列消费者。
        /// </summary>
        /// <param name="serviceProvider">依赖注入服务提供者</param>
        /// <param name="queue">队列名称</param>
        /// <param name="rabbitConsumerOptions">消费者配置选项</param>
        /// <param name="recieved">消息接收回调委托</param>
        public DefaultRabbitConsumerProvider(IServiceProvider serviceProvider, string queue, Options.RabbitConsumerOption rabbitConsumerOptions, Func<RecieveResult, Task> recieved)
        {
            _rabbitConsumerOptions = rabbitConsumerOptions;

            this.Exchange = string.Empty;
            this.Queue = queue;
            this.recieved = recieved;

            Consumer = RabbitConsumer.Create(serviceProvider, rabbitConsumerOptions);
        }

        /// <summary>
        /// 使用交换机、队列名称和消费者配置初始化提供者，创建绑定交换机的队列消费者。
        /// </summary>
        /// <param name="serviceProvider">依赖注入服务提供者</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="rabbitConsumerOptions">消费者配置选项</param>
        /// <param name="recieved">消息接收回调委托</param>
        public DefaultRabbitConsumerProvider(IServiceProvider serviceProvider, string exchange, string queue, Options.RabbitConsumerOption rabbitConsumerOptions, Func<RecieveResult, Task> recieved)
        {
            _rabbitConsumerOptions = rabbitConsumerOptions;

            this.Exchange = exchange;
            this.Queue = queue;
            this.recieved = recieved;

            Consumer = RabbitConsumer.Create(serviceProvider, rabbitConsumerOptions);
        }

        /// <summary>
        /// 交换机名称。若未绑定交换机则为空字符串。
        /// </summary>
        public string Exchange { get; }

        /// <summary>
        /// 队列名称。
        /// </summary>
        public string Queue { get; }

        /// <summary>
        /// RabbitMQ 消费者实例，封装底层的连接管理和消息消费逻辑。
        /// </summary>
        public RabbitConsumer Consumer { get; }

        /// <summary>
        /// 停止监听并释放消费者资源。
        /// 会停止当前监听（<see cref="ListenResult.Stop"/>）并 Dispose 底层的 <see cref="RabbitConsumer"/> 实例。
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                listenResult?.Stop();
                Consumer?.Dispose();
            }
        }

        /// <summary>
        /// 开始监听消息，若已启动则跳过。
        /// 根据是否指定交换机选择不同的监听方式：队列直连监听或交换机绑定监听。
        /// 监听参数（持久化、自动删除、自动 ACK、拉取数量等）从消费者配置选项继承。
        /// </summary>
        /// <returns>异步任务</returns>
        /// <exception cref="ObjectDisposedException">当提供者已被释放时抛出</exception>
        public async Task ListenAsync()
        {
            if (disposed) throw new ObjectDisposedException(nameof(DefaultRabbitConsumerProvider));

            if (listenResult == null)
            {
                if (string.IsNullOrEmpty(this.Exchange))
                {
                    listenResult = await Consumer.ListenAsync(this.Queue, options =>
                    {
                        options.AutoDelete = _rabbitConsumerOptions.AutoDelete;
                        options.Durable = _rabbitConsumerOptions.Durable;
                        options.AutoAck = _rabbitConsumerOptions.AutoAck;
                        options.Arguments = _rabbitConsumerOptions.Arguments ?? new Dictionary<string, object>();
                        options.FetchCount = _rabbitConsumerOptions.FetchCount;
                        options.DeadLetterExchange = _rabbitConsumerOptions.DeadLetterExchange;
                    }, recieved);
                }
                else
                {
                    listenResult = await Consumer.ListenAsync(this.Exchange, this.Queue, options =>
                    {
                        options.AutoDelete = _rabbitConsumerOptions.AutoDelete;
                        options.Durable = _rabbitConsumerOptions.Durable;
                        options.AutoAck = _rabbitConsumerOptions.AutoAck;
                        options.Arguments = _rabbitConsumerOptions.Arguments ?? new Dictionary<string, object>();
                        options.FetchCount = _rabbitConsumerOptions.FetchCount;
                        options.Type = _rabbitConsumerOptions.Type;
                        options.RouteQueues = _rabbitConsumerOptions.RouteQueues;
                        options.DeadLetterExchange = _rabbitConsumerOptions.DeadLetterExchange;
                    }, recieved);
                }
            }
        }

        /// <summary>
        /// 返回监听描述信息。
        /// 队列直连监听格式为 "{Queue} from {Consumer}"，
        /// 交换机绑定监听格式为 "{Queue}(exchange:{Exchange}) from {Consumer}"。
        /// </summary>
        /// <returns>包含队列名和连接信息的描述字符串</returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Exchange))
            {
                return $"{this.Queue} from {Consumer}";
            }
            else
            {
                return $"{this.Queue}(exchange:{this.Exchange}) from {Consumer}";
            }
        }
    }
}
