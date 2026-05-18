using Lycoris.RabbitMQ.Extensions.Impl;
using Lycoris.RabbitMQ.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Base
{
    /// <summary>
    /// 生产者对象池基类，管理 RabbitProducer 实例的租用与归还，控制池中实例数量。实现 IDisposable 和 IAsyncDisposable
    /// </summary>
    public abstract class BaseProducerPool : IDisposable, IAsyncDisposable
    {
        private readonly ConcurrentQueue<RabbitProducer> rabbitProducers;
        private readonly RabbitProducerOption _rabbitOptions;

        /// <summary>
        /// 使用生产者配置选项初始化对象池
        /// </summary>
        /// <param name="rabbitOptions">生产者配置选项</param>
        protected BaseProducerPool(RabbitProducerOption rabbitOptions)
        {
            _rabbitOptions = rabbitOptions;
            rabbitProducers = new ConcurrentQueue<RabbitProducer>();
        }

        /// <summary>
        /// 当前对象池是否已释放
        /// </summary>
        public bool Disposed { get; private set; } = false;

        /// <summary>
        /// 池中保留的最大生产者实例数，默认 5
        /// </summary>
        protected virtual int InitializeCount { get; } = 5;

        /// <summary>
        /// 从对象池中租用一个生产者实例，池为空时创建新实例
        /// </summary>
        /// <returns>可用的 <see cref="RabbitProducer"/> 实例</returns>
        /// <exception cref="ObjectDisposedException">对象池已释放时抛出</exception>
        public RabbitProducer RentProducer()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(nameof(RabbitClientProducer));
            }

            RabbitProducer rabbitProducer;
            lock (rabbitProducers)
            {
                if (!rabbitProducers.TryDequeue(out rabbitProducer) || rabbitProducer == null || !rabbitProducer.IsOpen)
                    rabbitProducer = RabbitProducer.Create(_rabbitOptions);
            }
            return rabbitProducer;
        }

        /// <summary>
        /// 将生产者实例归还对象池，超过保留数量则释放
        /// </summary>
        /// <param name="rabbitProducer">待归还的生产者实例</param>
        public void ReturnProducer(RabbitProducer rabbitProducer)
        {
            if (Disposed) return;

            lock (rabbitProducers)
            {
                if (rabbitProducers.Count < InitializeCount && rabbitProducer != null && rabbitProducer.IsOpen)
                    rabbitProducers.Enqueue(rabbitProducer);
                else
                    rabbitProducer?.Dispose();
            }
        }

        /// <summary>
        /// 释放对象池中的所有生产者实例（同步）
        /// </summary>
        public void Dispose()
        {
            if (!Disposed)
            {
                Disposed = true;
                while (!rabbitProducers.IsEmpty)
                {
                    rabbitProducers.TryDequeue(out RabbitProducer rabbitProducer);
                    rabbitProducer?.Dispose();
                }
            }
        }

        /// <summary>
        /// 异步释放对象池中的所有生产者实例
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (!Disposed)
            {
                Disposed = true;
                while (!rabbitProducers.IsEmpty)
                {
                    rabbitProducers.TryDequeue(out RabbitProducer rabbitProducer);
                    if (rabbitProducer != null)
                        await rabbitProducer.DisposeAsync();
                }
            }
        }
    }
}
