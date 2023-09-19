using Lycoris.RabbitMQ.Extensions.DataModel;
using Lycoris.RabbitMQ.Extensions.Impl;
using Lycoris.RabbitMQ.Extensions.Options;

namespace Lycoris.RabbitMQ.Extensions.Builder.Consumer.Impl
{
    /// <summary>
    /// 消费者提供者
    /// </summary>
    internal sealed class DefaultRabbitConsumerProvider : IRabbitConsumerProvider
    {
        private readonly RabbitConsumerOptions _rabbitConsumerOptions;
        private readonly string exchange;
        private readonly string queue;
        private ListenResult? listenResult;
        private bool disposed = false;
        private readonly Action<RecieveResult> action;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queue">队列名称</param>
        /// <param name="rabbitConsumerOptions"></param>
        /// <param name="action"></param>
        public DefaultRabbitConsumerProvider(string queue, RabbitConsumerOptions rabbitConsumerOptions, Action<RecieveResult> action)
        {
            _rabbitConsumerOptions = rabbitConsumerOptions;

            exchange = string.Empty;

            this.queue = queue;
            this.action = action;

            Consumer = RabbitConsumer.Create(rabbitConsumerOptions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="rabbitConsumerOptions"></param>
        /// <param name="action"></param>
        public DefaultRabbitConsumerProvider(string exchange, string queue, RabbitConsumerOptions rabbitConsumerOptions, Action<RecieveResult> action)
        {
            _rabbitConsumerOptions = rabbitConsumerOptions;

            this.exchange = exchange;
            this.queue = queue;
            this.action = action;

            Consumer = RabbitConsumer.Create(rabbitConsumerOptions);
        }

        /// <summary>
        /// 
        /// </summary>
        public RabbitConsumer Consumer { get; }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task ListenAsync()
        {
            if (disposed) throw new ObjectDisposedException(nameof(DefaultRabbitConsumerProvider));

            if (listenResult == null)
            {
                if (string.IsNullOrEmpty(exchange))
                {
                    listenResult = Consumer.Listen(queue, options =>
                    {
                        options.AutoDelete = _rabbitConsumerOptions.AutoDelete;
                        options.Durable = _rabbitConsumerOptions.Durable;
                        options.AutoAck = _rabbitConsumerOptions.AutoAck;
                        options.Arguments = _rabbitConsumerOptions.Arguments ?? new Dictionary<string, object>();
                        options.FetchCount = _rabbitConsumerOptions.FetchCount;
                    }, action);
                }
                else
                {
                    listenResult = Consumer.Listen(exchange, queue, options =>
                    {
                        options.AutoDelete = _rabbitConsumerOptions.AutoDelete;
                        options.Durable = _rabbitConsumerOptions.Durable;
                        options.AutoAck = _rabbitConsumerOptions.AutoAck;
                        options.Arguments = _rabbitConsumerOptions.Arguments ?? new Dictionary<string, object>();
                        options.FetchCount = _rabbitConsumerOptions.FetchCount;
                        options.Type = _rabbitConsumerOptions.Type;
                        options.RouteQueues = _rabbitConsumerOptions.RouteQueues;
                    }, action);
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(exchange))
            {
                return $"queue:{queue} from {Consumer}";
            }
            else
            {
                return $"queue:{queue}(exchange:{exchange}) from {Consumer}";
            }
        }
    }
}
