using Lycoris.RabbitMQ.Extensions.DataModel;
using System;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Options
{
    internal class RabbitConsumerMap
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
        public Func<RecieveResult, Task> MessageRecieved { get; set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public bool KeyConsumer { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public string KeyName { get; set; }
    }
}
