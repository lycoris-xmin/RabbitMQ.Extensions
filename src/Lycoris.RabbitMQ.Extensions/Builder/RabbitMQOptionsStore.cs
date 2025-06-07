using Lycoris.RabbitMQ.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Lycoris.RabbitMQ.Extensions.Builder
{
    internal static class RabbitMQOptionsStore
    {
        private const string DelayPropsKey = "x-delay";
        private static readonly ConcurrentDictionary<string, RabbitOption> _RabbitOptions = new ConcurrentDictionary<string, RabbitOption>();
        private static readonly ConcurrentDictionary<string, RabbitProducerOption> _RabbitProducerOptions = new ConcurrentDictionary<string, RabbitProducerOption>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configureName"></param>
        /// <param name="options"></param>
        internal static void AddOrUpdateRabbitMQOption(string configureName, RabbitOption options)
            => _RabbitOptions.TryAdd(configureName, options);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configureName"></param>
        /// <returns></returns>
        internal static RabbitOption GetRabbitMQOption(string configureName)
            => _RabbitOptions.ContainsKey(configureName) ? _RabbitOptions[configureName] : null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configureName"></param>
        /// <param name="options"></param>
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
        /// 
        /// </summary>
        /// <param name="configureName"></param>
        /// <returns></returns>
        internal static RabbitProducerOption GetRabbitProducerOptions(string configureName)
            => _RabbitProducerOptions.ContainsKey(configureName) ? _RabbitProducerOptions[configureName] : null;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static List<RabbitOption> GetAllRabbitMQOption()
        {
            var list = _RabbitOptions.Select(x => x.Value).ToList();
            return list.GroupBy(x => new { x.Hosts, x.Port }).Select(x => x.First()).ToList() ?? new List<RabbitOption>();
        }
    }
}
