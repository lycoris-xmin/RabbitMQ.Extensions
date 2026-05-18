using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lycoris.RabbitMQ.Extensions.DataModel
{
    /// <summary>
    ///
    /// </summary>
    public class RecieveResult
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="arg"></param>
        public RecieveResult(BasicDeliverEventArgs arg)
        {
            this.MessageId = arg.BasicProperties.MessageId;
            this.Body = Encoding.UTF8.GetString(arg.Body.ToArray());
            this.Exchange = arg.Exchange;
            this.Route = arg.RoutingKey;
            this.Redelivered = arg.Redelivered;
            this.DeliveryTag = arg.DeliveryTag;
            this.ConsumerTag = arg.ConsumerTag;
            this.Headers = arg.BasicProperties?.Headers ?? new Dictionary<string, object>();
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
        /// 交换机
        /// </summary>
        public string Exchange { get; private set; }

        /// <summary>
        /// 路由
        /// </summary>
        public string Route { get; private set; }

        /// <summary>
        /// 队列
        /// </summary>
        public string Queue { get; internal set; }

        /// <summary>
        /// 消费者标签
        /// </summary>
        public string ConsumerTag { get; private set; }

        /// <summary>
        /// Ack标签
        /// </summary>
        public ulong DeliveryTag { get; private set; }

        /// <summary>
        /// 是否为重新投递的消息
        /// </summary>
        public bool Redelivered { get; private set; }

        /// <summary>
        /// 是否提交
        /// </summary>
        public bool IsCommit { get; private set; }

        /// <summary>
        /// 提交失败是否重新进入队列
        /// </summary>
        public bool Requeue { get; private set; }

        /// <summary>
        /// 消息头
        /// </summary>
        public IDictionary<string, object> Headers { get; private set; }

        /// <summary>
        /// 提交消息
        /// </summary>
        public void Commit()
        {
            IsCommit = true;
        }

        /// <summary>
        /// 回滚消息
        /// </summary>
        /// <param name="requeue">是否重新进入队列</param>
        public void RollBack(bool requeue = false)
        {
            Requeue = requeue;
            IsCommit = false;
        }
    }
}
