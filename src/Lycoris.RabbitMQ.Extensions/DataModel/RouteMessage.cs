namespace Lycoris.RabbitMQ.Extensions.DataModel
{
    /// <summary>
    /// 
    /// </summary>
    public class RouteMessage
    {
        /// <summary>
        /// 路由
        /// </summary>
        public string RoutingKey { get; set; } = string.Empty;

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
