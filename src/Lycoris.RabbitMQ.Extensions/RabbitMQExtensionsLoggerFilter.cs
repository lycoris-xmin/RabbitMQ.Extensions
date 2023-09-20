using System.Collections.Generic;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class RabbitMQExtensionsLoggerFilter
    {
        /// <summary>
        /// 
        /// </summary>
        public static List<string> Namespace
        {
            get
            {
                return new List<string>()
                {
                    "Lycoris.RabbitMQ.Extensions.Impl.Consumers.DefaultConsumerHostedService"
                };
            }
        }
    }
}
