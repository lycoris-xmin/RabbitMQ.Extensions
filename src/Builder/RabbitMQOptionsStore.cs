using Lycoris.RabbitMQ.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Lycoris.RabbitMQ.Extensions.Builder
{
    /// <summary>
    /// RabbitMQ 配置选项存储（内部使用），管理全局连接配置和生产者配置选项的完整生命周期。
    /// 使用线程安全的 <see cref="ConcurrentDictionary{TKey, TValue}"/> 存储，支持多生产者场景下的独立配置管理。
    /// </summary>
    internal static class RabbitMQOptionsStore
    {
        private const string DelayPropsKey = "x-delay";
        private static readonly ConcurrentDictionary<string, RabbitOption> _RabbitOptions = new ConcurrentDictionary<string, RabbitOption>();
        private static readonly ConcurrentDictionary<string, RabbitProducerOption> _RabbitProducerOptions = new ConcurrentDictionary<string, RabbitProducerOption>();

        /// <summary>
        /// 添加全局连接配置（已存在则忽略）。
        /// 每个 <see cref="RabbitMqBuilder"/> 实例都有唯一的配置名称，确保不同配置实例之间互不覆盖。
        /// </summary>
        /// <param name="configureName">配置名称，通常由 <see cref="RabbitMqBuilder"/> 自动生成</param>
        /// <param name="options">RabbitMQ 连接参数配置</param>
        internal static void TryAddRabbitMQOption(string configureName, RabbitOption options)
            => _RabbitOptions.TryAdd(configureName, options);

        /// <summary>
        /// 根据配置名称获取全局连接配置。
        /// </summary>
        /// <param name="configureName">配置名称</param>
        /// <returns>对应的 <see cref="RabbitOption"/> 连接配置，若不存在则返回 <c>null</c></returns>
        internal static RabbitOption GetRabbitMQOption(string configureName)
        {
            _RabbitOptions.TryGetValue(configureName, out var option);
            return option;
        }

        /// <summary>
        /// 添加或更新生产者配置，自动处理延迟队列参数和默认值。
        /// 如果是延迟交换机（<see cref="RabbitExchangeType.Delayed"/>），会自动添加 <c>x-delay</c> 属性和延迟队列所需的 arguments。
        /// 所有生产者配置都会自动补充默认的 arguments。
        /// </summary>
        /// <param name="configureName">配置名称，用于标识和获取该生产者配置</param>
        /// <param name="options">生产者配置选项</param>
        internal static void AddOrUpdateRabbitProducerOptions(string configureName, RabbitProducerOption options)
        {
            if (options.Arguments == null)
                options.Arguments = new Dictionary<string, object>();

            if (options.Type == RabbitExchangeType.Delayed)
            {
                if (options.DelayTime <= 0)
                    throw new ArgumentOutOfRangeException(nameof(options.DelayTime));

                if (options.BasicProps == null)
                    options.BasicProps = new Dictionary<string, object>();

                if (options.BasicProps.ContainsKey(DelayPropsKey))
                    options.BasicProps[DelayPropsKey] = options.DelayTime * 1000;
                else
                    options.BasicProps.Add(DelayPropsKey, options.DelayTime * 1000);


                foreach (var item in RabbitMQArguments.Delayed)
                {
                    if (options.Arguments.ContainsKey(item.Key))
                        continue;

                    options.Arguments.Add(item.Key, item.Value);
                }
            }

            foreach (var item in RabbitMQArguments.Default)
            {
                if (options.Arguments.ContainsKey(item.Key))
                    continue;

                options.Arguments.Add(item.Key, item.Value);
            }

            _RabbitProducerOptions.TryAdd(configureName, options);
        }

        /// <summary>
        /// 根据配置名称获取生产者配置。
        /// </summary>
        /// <param name="configureName">配置名称</param>
        /// <returns>对应的 <see cref="RabbitProducerOption"/> 生产者配置，若不存在则返回 <c>null</c></returns>
        internal static RabbitProducerOption GetRabbitProducerOptions(string configureName)
        {
            _RabbitProducerOptions.TryGetValue(configureName, out var option);
            return option;
        }

        /// <summary>
        /// 获取所有去重的全局连接配置列表。
        /// 按 <c>Hosts</c> 和 <c>Port</c> 组合去重，返回不重复的 <see cref="RabbitOption"/> 列表。
        /// 用于健康检查、多节点监控等需要遍历所有 RabbitMQ 连接实例的场景。
        /// </summary>
        /// <returns>去重后的全局连接配置列表</returns>
        internal static List<RabbitOption> GetAllRabbitMQOption()
        {
            var list = _RabbitOptions.Select(x => x.Value).ToList();
            return list.GroupBy(x => new { x.Hosts, x.Port }).Select(x => x.First()).ToList() ?? new List<RabbitOption>();
        }
    }
}
