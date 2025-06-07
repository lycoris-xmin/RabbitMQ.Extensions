using Lycoris.RabbitMQ.Extensions.DataModel;
using System;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BaseRabbitConsumer : IRabbitConsumerListener
    {
        /// <summary>
        /// 交换机
        /// </summary>
        protected string Exchange => this.Context.Exchange ?? "";

        /// <summary>
        /// 路由
        /// </summary>
        protected string Queue => this.Context.Queue ?? "";

        /// <summary>
        /// 路由
        /// </summary>
        protected string Route => this.Context.Route ?? "";

        /// <summary>
        /// 消费上下文
        /// </summary>
        protected RecieveResult Context { get; private set; }

        /// <summary>
        /// 重新发布时间间隔(单位:毫秒,默认1000毫秒)
        /// </summary>
        protected int ResubmitTimeSpan { get; set; } = 1000;

        /// <summary>
        /// 消费消息
        /// </summary>
        /// <param name="recieveResult"></param>
        /// <returns></returns>
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
                recieveResult.Commit();
            }
            else if (handlerResult == ReceivedHandler.RollBack)
            {
                recieveResult.RollBack();
            }
            else
            {
                //延迟发布
                if (this.ResubmitTimeSpan > 0)
                    await Task.Delay(this.ResubmitTimeSpan);

                recieveResult.RollBack(true);
            }
        }

        /// <summary>
        /// 消费消息
        /// </summary>
        /// <param name="body"></param>
        /// <returns><see cref="ReceivedHandler"/>处理结果</returns>
        protected abstract Task<ReceivedHandler> ReceivedAsync(string body);

        /// <summary>
        /// 消费消息
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        protected virtual Task<ReceivedHandler> HandleExceptionAsync(Exception exception) => Task.FromResult(ReceivedHandler.RollBack);
    }
}
