using System;
using System.Collections.Generic;

namespace Lycoris.RabbitMQ.Extensions.Options
{
    /// <summary>
    /// 
    /// </summary>
    public class RabbitOption : RabbitBaseOption
    {
        /// <summary>
        /// 是否持久化
        /// </summary>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// 是否自动删除
        /// </summary>
        public bool AutoDelete { get; set; } = false;

        /// <summary>
        /// 参数,例如：{ "x-queue-type", "classic" }
        /// </summary>
        public Dictionary<string, object> Arguments { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, object> BasicProps { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal void CheckBaseOptions()
        {
            if (this.Hosts == null || this.Hosts.Length == 0)
                throw new ArgumentNullException(nameof(this.Hosts));

            if (this.Port <= 0)
                throw new ArgumentOutOfRangeException(nameof(this.Port));

            if (string.IsNullOrEmpty(this.UserName))
                throw new ArgumentNullException(nameof(this.UserName));

            if (string.IsNullOrEmpty(this.Password))
                throw new ArgumentNullException(nameof(this.Password));

            if (string.IsNullOrEmpty(this.VirtualHost))
                throw new ArgumentNullException(nameof(this.VirtualHost));
        }
    }
}
