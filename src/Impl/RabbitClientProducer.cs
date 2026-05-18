using Lycoris.RabbitMQ.Extensions.Base;
using Lycoris.RabbitMQ.Extensions.Options;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// 客户端生产者，封装对象池管理和多队列/交换机批量发布逻辑。从对象池租用生产者，发送完成后归还
    /// </summary>
    public sealed class RabbitClientProducer : BaseProducerPool, IRabbitClientProducer
    {
        private readonly RabbitProducerOption _rabbitProducerOptions;

        /// <summary>
        /// 使用生产者配置初始化客户端生产者
        /// </summary>
        /// <param name="rabbitProducerOptions">生产者配置选项</param>
        public RabbitClientProducer(RabbitProducerOption rabbitProducerOptions) : base(rabbitProducerOptions)
        {
            _rabbitProducerOptions = rabbitProducerOptions;
        }

        /// <summary>
        /// 池中保留的最大生产者实例数，由配置中的 <see cref="RabbitProducerOption.InitializeCount"/> 指定
        /// </summary>
        protected override int InitializeCount => _rabbitProducerOptions.InitializeCount;

        /// <summary>
        /// 向配置的所有目标队列发送同一条消息（普通模式、Work模式）
        /// </summary>
        /// <param name="messages">消息内容</param>
        /// <param name="cancellationToken">取消令牌</param>
        public async Task PublishAsync(string messages, CancellationToken cancellationToken = default)
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
                        Durable = _rabbitProducerOptions.Durable,
                        DeadLetterExchange = _rabbitProducerOptions.DeadLetterExchange
                    }, null, cancellationToken);
                }
            }

            ReturnProducer(producer);
        }

        /// <summary>
        /// 向配置的所有目标队列发送多条消息（普通模式、Work模式）
        /// </summary>
        /// <param name="messages">消息内容数组</param>
        /// <param name="cancellationToken">取消令牌</param>
        public async Task PublishAsync(string[] messages, CancellationToken cancellationToken = default)
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
                        Durable = _rabbitProducerOptions.Durable,
                        DeadLetterExchange = _rabbitProducerOptions.DeadLetterExchange
                    }, null, cancellationToken);
                }
            }

            ReturnProducer(producer);
        }

        /// <summary>
        /// 通过配置的交换机发送单条消息（订阅模式、路由模式、Topic模式）
        /// </summary>
        /// <param name="routingKey">路由键</param>
        /// <param name="messages">消息内容</param>
        /// <param name="cancellationToken">取消令牌</param>
        public async Task PublishAsync(string routingKey, string messages, CancellationToken cancellationToken = default)
        {
            var producer = RentProducer();

            await producer.PublishAsync(_rabbitProducerOptions.Exchange, routingKey, messages, new ExchangeQueueOptions()
            {
                Arguments = _rabbitProducerOptions.Arguments ?? new Dictionary<string, object>(),
                AutoDelete = _rabbitProducerOptions.AutoDelete,
                Durable = _rabbitProducerOptions.Durable,
                RouteQueues = _rabbitProducerOptions.RouteQueues,
                Type = _rabbitProducerOptions.Type,
                BasicProps = _rabbitProducerOptions.BasicProps ?? new Dictionary<string, object>(),
                DeadLetterExchange = _rabbitProducerOptions.DeadLetterExchange
            }, cancellationToken);

            ReturnProducer(producer);
        }

        /// <summary>
        /// 通过配置的交换机发送多条消息（订阅模式、路由模式、Topic模式）
        /// </summary>
        /// <param name="routingKey">路由键</param>
        /// <param name="messages">消息内容数组</param>
        /// <param name="cancellationToken">取消令牌</param>
        public async Task PublishAsync(string routingKey, string[] messages, CancellationToken cancellationToken = default)
        {
            var producer = RentProducer();

            await producer.PublishAsync(_rabbitProducerOptions.Exchange, routingKey, messages, new ExchangeQueueOptions()
            {
                Arguments = _rabbitProducerOptions.Arguments ?? new Dictionary<string, object>(),
                AutoDelete = _rabbitProducerOptions.AutoDelete,
                Durable = _rabbitProducerOptions.Durable,
                RouteQueues = _rabbitProducerOptions.RouteQueues,
                Type = _rabbitProducerOptions.Type,
                BasicProps = _rabbitProducerOptions.BasicProps ?? new Dictionary<string, object>(),
                DeadLetterExchange = _rabbitProducerOptions.DeadLetterExchange
            }, cancellationToken);

            ReturnProducer(producer);
        }
    }
}
