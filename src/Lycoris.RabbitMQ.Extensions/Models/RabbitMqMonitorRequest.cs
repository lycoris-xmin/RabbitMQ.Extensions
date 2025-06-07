using Newtonsoft.Json;

namespace Lycoris.RabbitMQ.Extensions.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class RabbitMqMonitorRequest
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonRequired]
        public string Host { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonRequired]
        public int Prot { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Passwrod { get; set; }
    }
}
