using Lycoris.RabbitMQ.Extensions.DataModel;
using System;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 消息消费者基类，实现 IRabbitConsumerListener。封装了消息处理、自动确认/回滚、指数退避重试和死信队列对接等完整消费逻辑
    /// </summary>
    public abstract class BaseRabbitConsumer : IRabbitConsumerListener
    {
        /// <summary>
        /// 当前消息所属的交换机
        /// </summary>
        protected string Exchange => this.Context.Exchange ?? "";

        /// <summary>
        /// 当前消息所属的队列
        /// </summary>
        protected string Queue => this.Context.Queue ?? "";

        /// <summary>
        /// 当前消息的路由键
        /// </summary>
        protected string Route => this.Context.Route ?? "";

        /// <summary>
        /// 当前消费的消息上下文
        /// </summary>
        protected RecieveResult Context { get; private set; }

        /// <summary>
        /// 重新投递基础时间间隔（毫秒），默认 1000。实际延迟按指数退避计算
        /// </summary>
        protected int ResubmitTimeSpan { get; set; } = 1000;

        /// <summary>
        /// 重新投递的最大时间间隔（毫秒），默认 60000（1分钟）
        /// </summary>
        protected int MaxResubmitTimeSpan { get; set; } = 60000;

        /// <summary>
        /// 最大重试次数，默认 3。超过后消息不再重新入队，若配置了死信队列则进入死信队列
        /// </summary>
        protected int MaxRetryCount { get; set; } = 3;

        private int _consecutiveFailures = 0;

        /// <summary>
        /// 消费消息入口方法，根据 ReceivedAsync 返回值决定提交、回滚或重新投递
        /// </summary>
        /// <param name="recieveResult">接收到的消息结果</param>
        public async Task ConsumeAsync(RecieveResult recieveResult)
        {
            this.Context = recieveResult;

            ReceivedHandler handlerResult;

            try
            {
                handlerResult = await ReceivedAsync(recieveResult.Body);
            }
            catch (Exception ex)
            {
                handlerResult = await HandleExceptionAsync(ex);
            }

            if (handlerResult == ReceivedHandler.Commit)
            {
                _consecutiveFailures = 0;
                recieveResult.Commit();
            }
            else if (handlerResult == ReceivedHandler.RollBack)
            {
                _consecutiveFailures = 0;
                recieveResult.RollBack();
            }
            else
            {
                _consecutiveFailures++;

                if (_consecutiveFailures > MaxRetryCount)
                {
                    _consecutiveFailures = 0;
                    recieveResult.RollBack(false);
                }
                else
                {
                    var delay = System.Math.Min(ResubmitTimeSpan * (int)System.Math.Pow(2, _consecutiveFailures - 1), MaxResubmitTimeSpan);
                    if (delay > 0)
                        await Task.Delay(delay);
                    recieveResult.RollBack(true);
                }
            }
        }

        /// <summary>
        /// 由子类实现的消息处理方法，返回 Commit/RollBack/Resubmit
        /// </summary>
        /// <param name="body">消息体内容</param>
        /// <returns><see cref="ReceivedHandler"/> 处理结果：Commit 确认消费、RollBack 拒绝并丢弃、Resubmit 重新入队</returns>
        protected abstract Task<ReceivedHandler> ReceivedAsync(string body);

        /// <summary>
        /// 处理消费过程中抛出的异常，默认返回 RollBack
        /// </summary>
        /// <param name="exception">异常信息</param>
        /// <returns>默认返回 <see cref="ReceivedHandler.RollBack"/></returns>
        protected virtual Task<ReceivedHandler> HandleExceptionAsync(Exception exception) => Task.FromResult(ReceivedHandler.RollBack);
    }
}
