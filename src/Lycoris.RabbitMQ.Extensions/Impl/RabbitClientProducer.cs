using Lycoris.RabbitMQ.Extensions.Base;
using Lycoris.RabbitMQ.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RabbitClientProducer : BaseProducerPool, IRabbitClientProducer
    {
        private readonly RabbitProducerOption _rabbitProducerOptions;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="rabbitProducerOptions"></param>
        public RabbitClientProducer(RabbitProducerOption rabbitProducerOptions) : base(rabbitProducerOptions)
        {
            _rabbitProducerOptions = rabbitProducerOptions;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override int InitializeCount => _rabbitProducerOptions.InitializeCount;

        /// <summary>
        /// 普通的往队列发送消息
        /// 普通模式、Work模式
        /// </summary>
        /// <param name="messages"></param>
        public async Task PublishAsync(string messages)
        {
            var producer = RentProducer();

            foreach (var queue in _rabbitProducerOptions.Queues)
            {
                if (!string.IsNullOrEmpty(queue))
                {
                    await producer.PublishAsync(queue, messages, new QueueOption()
                    {
                        Arguments = _rabbitProducerOptions.Arguments ?? new Dictionary<string, object>(),
                        AutoDelete = _rabbitProducerOptions.AutoDelete,
                        Durable = _rabbitProducerOptions.Durable
                    });
                }
            }

            ReturnProducer(producer);
        }

        /// <summary>
        /// 普通的往队列发送消息
        /// 普通模式、Work模式
        /// </summary>
        /// <param name="messages"></param>
        public async Task PublishAsync(string[] messages)
        {
            var producer = RentProducer();

            foreach (var queue in _rabbitProducerOptions.Queues)
            {
                if (!string.IsNullOrEmpty(queue))
                {
                    await producer.PublishAsync(queue, messages, new QueueOption()
                    {
                        Arguments = _rabbitProducerOptions.Arguments ?? new Dictionary<string, object>(),
                        AutoDelete = _rabbitProducerOptions.AutoDelete,
                        Durable = _rabbitProducerOptions.Durable
                    });
                }
            }

            ReturnProducer(producer);
        }

        /// <summary>
        /// 使用交换机发送消息
        /// 订阅模式、路由模式、Topic模式
        /// </summary>
        /// <param name="routingKey"></param>
        /// <param name="messages"></param>
        public async Task PublishAsync(string routingKey, string messages)
        {
            var producer = RentProducer();

            await producer.PublishAsync(_rabbitProducerOptions.Exchange, routingKey, messages, new ExchangeQueueOptions()
            {
                Arguments = _rabbitProducerOptions.Arguments ?? new Dictionary<string, object>(),
                AutoDelete = _rabbitProducerOptions.AutoDelete,
                Durable = _rabbitProducerOptions.Durable,
                RouteQueues = _rabbitProducerOptions.RouteQueues,
                Type = _rabbitProducerOptions.Type,
                BasicProps = _rabbitProducerOptions.BasicProps ?? new Dictionary<string, object>()
            });

            ReturnProducer(producer);
        }

        /// <summary>
        /// 使用交换机发送消息
        /// 订阅模式、路由模式、Topic模式
        /// </summary>
        /// <param name="routingKey"></param>
        /// <param name="messages"></param>
        public async Task PublishAsync(string routingKey, string[] messages)
        {
            var producer = RentProducer();

            await producer.PublishAsync(_rabbitProducerOptions.Exchange, routingKey, messages, new ExchangeQueueOptions()
            {
                Arguments = _rabbitProducerOptions.Arguments ?? new Dictionary<string, object>(),
                AutoDelete = _rabbitProducerOptions.AutoDelete,
                Durable = _rabbitProducerOptions.Durable,
                RouteQueues = _rabbitProducerOptions.RouteQueues,
                Type = _rabbitProducerOptions.Type,
                BasicProps = _rabbitProducerOptions.BasicProps ?? new Dictionary<string, object>()
            });

            ReturnProducer(producer);
        }
    }
}
