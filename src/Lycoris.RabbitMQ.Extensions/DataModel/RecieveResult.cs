using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace Lycoris.RabbitMQ.Extensions.DataModel
{
    /// <summary>
    /// 
    /// </summary>
    public class RecieveResult : IDisposable
    {
        CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="cancellationTokenSource"></param>
        public RecieveResult(BasicDeliverEventArgs arg, CancellationTokenSource cancellationTokenSource)
        {
            this.MessageId = arg.BasicProperties.MessageId;
            this.Body = Encoding.UTF8.GetString(arg.Body.ToArray());
            this.ConsumerTag = arg.ConsumerTag;
            this.DeliveryTag = arg.DeliveryTag;
            this.Exchange = arg.Exchange;
            this.Redelivered = arg.Redelivered;
            this.RoutingKey = arg.RoutingKey;
            this.cancellationTokenSource = cancellationTokenSource;
        }

        /// <summary>
        /// 消息编号
        /// </summary>
        public string MessageId { get; private set; }

        /// <summary>
        /// 消息体
        /// </summary>
        public string Body { get; private set; }

        /// <summary>
        /// 消费者标签
        /// </summary>
        public string ConsumerTag { get; private set; }

        /// <summary>
        /// Ack标签
        /// </summary>
        public ulong DeliveryTag { get; private set; }

        /// <summary>
        /// 交换机
        /// </summary>
        public string Exchange { get; private set; }

        /// <summary>
        /// 是否Ack
        /// </summary>
        public bool Redelivered { get; private set; }

        /// <summary>
        /// 路由
        /// </summary>
        public string RoutingKey { get; private set; }

        /// <summary>
        /// 是否提交
        /// </summary>
        public bool IsCommit { get; private set; }

        /// <summary>
        /// 提交失败是否重新进入队列
        /// </summary>
        public bool Requeue { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        private void CancelNotify()
        {
            if (cancellationTokenSource == null || cancellationTokenSource.IsCancellationRequested)
                return;

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        /// <summary>
        /// 提交消息
        /// </summary>
        public void Commit()
        {
            if (cancellationTokenSource == null || cancellationTokenSource.IsCancellationRequested)
                return;

            IsCommit = true;
            CancelNotify();
        }

        /// <summary>
        /// 回滚消息
        /// </summary>
        /// <param name="requeue">是否重新进入队列</param>
        public void RollBack(bool requeue = false)
        {
            if (cancellationTokenSource == null || cancellationTokenSource.IsCancellationRequested)
                return;

            Requeue = requeue;
            IsCommit = false;
            (this as IDisposable).Dispose();
        }

        void IDisposable.Dispose()
        {
            CancelNotify();
        }
    }
}
