using Lycoris.RabbitMQ.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Lycoris.RabbitMQ.Extensions.Builder
{
    internal static class RabbitMQOptionsStore
    {
        private const string DelayPropsKey = "x-delay";
        private static readonly ConcurrentDictionary<string, RabbitOptions> _RabbitOptions = new ConcurrentDictionary<string, RabbitOptions>();
        private static readonly ConcurrentDictionary<string, RabbitProducerOptions> _RabbitProducerOptions = new ConcurrentDictionary<string, RabbitProducerOptions>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configureName"></param>
        /// <param name="options"></param>
        public static void AddOrUpdateRabbitMQOption(string configureName, RabbitOptions options)
            => _RabbitOptions.TryAdd(configureName, options);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configureName"></param>
        /// <returns></returns>
        public static RabbitOptions GetRabbitMQOption(string configureName)
            => _RabbitOptions.ContainsKey(configureName) ? _RabbitOptions[configureName] : null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configureName"></param>
        /// <param name="options"></param>
        public static void AddOrUpdateRabbitProducerOptions(string configureName, RabbitProducerOptions options)
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
        public static RabbitProducerOptions GetRabbitProducerOptions(string configureName)
            => _RabbitProducerOptions.ContainsKey(configureName) ? _RabbitProducerOptions[configureName] : null;
    }
}
