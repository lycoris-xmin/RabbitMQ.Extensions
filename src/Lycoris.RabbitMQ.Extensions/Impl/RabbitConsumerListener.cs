using Lycoris.RabbitMQ.Extensions.DataModel;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class RabbitConsumerListener : IRabbitConsumerListener
    {
        /// <summary>
        /// 交换机
        /// </summary>
        protected string Exchange { get; private set; } = string.Empty;

        /// <summary>
        /// 路由
        /// </summary>
        protected string Route { get; private set; } = string.Empty;

        /// <summary>
        /// 消费上下文
        /// </summary>
        protected RecieveResult? Context { get; private set; }

        /// <summary>
        /// 重新发布时间间隔(单位:毫秒,默认1000毫秒)
        /// </summary>
        protected int ResubmitTimeSpan { get; set; } = 1000;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recieveResult"></param>
        /// <returns></returns>
        public async Task ConsumeAsync(RecieveResult recieveResult)
        {
            Context = recieveResult;
            Exchange = recieveResult.Exchange ?? "";
            Route = recieveResult.RoutingKey ?? "";

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
                if (ResubmitTimeSpan > 0)
                    await Task.Delay(ResubmitTimeSpan);

                recieveResult.RollBack(true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        /// <returns><see cref="ReceivedHandler"/>处理结果</returns>
        protected abstract Task<ReceivedHandler> ReceivedAsync(string body);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        protected virtual Task<ReceivedHandler> HandleExceptionAsync(Exception exception) => Task.FromResult(ReceivedHandler.RollBack);
    }
}
