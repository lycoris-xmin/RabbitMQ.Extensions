using Lycoris.RabbitMQ.Extensions.DataModel;
using System;

namespace Lycoris.RabbitMQ.Extensions.Options
{
    internal class RabbitConsumerListenerMap
    {
        /// <summary>
        /// 
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Queue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Type Listener { get; set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public Action<RecieveResult> MessageRecieved { get; set; } = null;
    }
}
