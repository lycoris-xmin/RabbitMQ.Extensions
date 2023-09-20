using Lycoris.RabbitMQ.Extensions.Impl;
using Lycoris.RabbitMQ.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace Lycoris.RabbitMQ.Extensions.Base
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BaseProducerPool : IDisposable
    {
        private readonly ConcurrentQueue<RabbitProducer> rabbitProducers;
        private readonly RabbitOptions _rabbitOptions;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rabbitOptions"></param>
        protected BaseProducerPool(RabbitOptions rabbitOptions)
        {
            _rabbitOptions = rabbitOptions;
            rabbitProducers = new ConcurrentQueue<RabbitProducer>();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Disposed { get; private set; } = false;

        /// <summary>
        /// 保留发布者数
        /// </summary>
        protected virtual int InitializeCount { get; } = 5;

        /// <summary>
        /// 租赁一个发布者
        /// </summary>
        /// <returns></returns>
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
        /// 返回保存发布者
        /// </summary>
        /// <param name="rabbitProducer"></param>
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
        /// 释放
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
    }
}
