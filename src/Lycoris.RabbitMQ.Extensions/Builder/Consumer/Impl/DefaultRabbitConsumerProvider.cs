using Lycoris.RabbitMQ.Extensions.DataModel;
using Lycoris.RabbitMQ.Extensions.Impl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Builder.Consumer.Impl
{
    /// <summary>
    /// 消费者提供者
    /// </summary>
    internal sealed class DefaultRabbitConsumerProvider : IRabbitConsumerProvider
    {
        private readonly Options.RabbitConsumerOption _rabbitConsumerOptions;
        private ListenResult listenResult;
        private bool disposed = false;
        private readonly Func<RecieveResult, Task> recieved;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="queue"></param>
        /// <param name="rabbitConsumerOptions"></param>
        /// <param name="recieved"></param>
        public DefaultRabbitConsumerProvider(IServiceProvider serviceProvider, string queue, Options.RabbitConsumerOption rabbitConsumerOptions, Func<RecieveResult, Task> recieved)
        {
            _rabbitConsumerOptions = rabbitConsumerOptions;

            this.Exchange = string.Empty;
            this.Queue = queue;
            this.recieved = recieved;

            Consumer = RabbitConsumer.Create(serviceProvider, rabbitConsumerOptions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        /// <param name="rabbitConsumerOptions"></param>
        /// <param name="recieved"></param>
        public DefaultRabbitConsumerProvider(IServiceProvider serviceProvider, string exchange, string queue, Options.RabbitConsumerOption rabbitConsumerOptions, Func<RecieveResult, Task> recieved)
        {
            _rabbitConsumerOptions = rabbitConsumerOptions;

            this.Exchange = exchange;
            this.Queue = queue;
            this.recieved = recieved;

            Consumer = RabbitConsumer.Create(serviceProvider, rabbitConsumerOptions);
        }

        /// <summary>
        /// 
        /// </summary>
        public string Exchange { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Queue { get; }

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
                if (string.IsNullOrEmpty(this.Exchange))
                {
                    listenResult = await Consumer.ListenAsync(this.Queue, options =>
                    {
                        options.AutoDelete = _rabbitConsumerOptions.AutoDelete;
                        options.Durable = _rabbitConsumerOptions.Durable;
                        options.AutoAck = _rabbitConsumerOptions.AutoAck;
                        options.Arguments = _rabbitConsumerOptions.Arguments ?? new Dictionary<string, object>();
                        options.FetchCount = _rabbitConsumerOptions.FetchCount;
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
                    }, recieved);
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
