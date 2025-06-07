namespace Lycoris.RabbitMQ.Extensions.Models
{
    /// <summary>
    /// RabbitMQ交换机类型
    /// </summary>
    public class RabbitMqExchangeTypeModel
    {
        /// <summary>
        /// 交换机的名称（如：direct、topic等）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 交换机的描述信息
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 该交换机是否启用
        /// </summary>
        public bool Enabled { get; set; }
    }
}
