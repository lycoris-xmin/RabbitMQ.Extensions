using Lycoris.RabbitMQ.Extensions.Builder.Consumer;
using Lycoris.RabbitMQ.Extensions.DataModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class RabbitMqExtension
    {
        /// <summary>
        /// 普通的往队列发送消息
        /// </summary>
        /// <param name="producer"></param>
        /// <param name="message"></param>
        public static Task PublishAsync(this IRabbitClientProducer producer, string message)
            => producer.PublishAsync(new string[] { message });

        /// <summary>
        /// 使用交换机发送消息
        /// </summary>
        /// <param name="producer"></param>
        /// <param name="routingKey">路由名称</param>
        /// <param name="message"></param>
        public static Task PublishAsync(this IRabbitClientProducer producer, string routingKey, string message)
            => producer.PublishAsync(routingKey, new string[] { message });

        /// <summary>
        /// 添加队列监听
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="queue">队列名称</param>
        /// <param name="onMessageRecieved"></param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddConsumer(this IRabbitConsumerBuilder builder, string queue, Func<RecieveResult, Task> onMessageRecieved)
            => builder.AddConsumer(queue, async (_, result) => { await onMessageRecieved?.Invoke(result); });

        /// <summary>
        /// 添加交换机监听
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="onMessageRecieved"></param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddConsumer(this IRabbitConsumerBuilder builder, string exchange, string queue, Func<RecieveResult, Task> onMessageRecieved)
            => builder.AddConsumer(exchange, queue, async (_, result) => { await onMessageRecieved?.Invoke(result); });

        /// <summary>
        /// 添加自定义监听
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="queue">队列名称</param>
        /// <param name="listenerType"></param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddConsumer(this IRabbitConsumerBuilder builder, string queue, Type listenerType)
        {
            if (!typeof(IRabbitConsumerListener).IsAssignableFrom(listenerType) || !listenerType.IsClass || listenerType.IsAbstract)
                throw new ArgumentException("the listener type must be implement IRabbitConsumerListener and none abstract class", "listenerType");

            builder.Services.AddTransient(listenerType);
            return builder.AddConsumer(queue, async (serviceProvider, result) =>
            {
                if (!(serviceProvider.GetService(listenerType) is IRabbitConsumerListener listenner))
                    throw new ArgumentNullException(nameof(listenner));

                result.Queue = queue;

                await listenner.ConsumeAsync(result);
            });
        }

        /// <summary>
        /// 添加自定义监听
        /// </summary>
        /// <param name="builder"></param>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue">队列名称</param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddConsumer<T>(this IRabbitConsumerBuilder builder, string queue) where T : class, IRabbitConsumerListener
            => builder.AddConsumer(queue, typeof(T));

        /// <summary>
        /// 添加自定义监听
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="listenerType"></param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddConsumer(this IRabbitConsumerBuilder builder, string exchange, string queue, Type listenerType)
        {
            if (!typeof(IRabbitConsumerListener).IsAssignableFrom(listenerType) || !listenerType.IsClass || listenerType.IsAbstract)
                throw new ArgumentException($"the listener type must be implement IRabbitConsumerListener and none abstract class", "listenerType");

            builder.Services.AddTransient(listenerType);
            return builder.AddConsumer(exchange, queue, async (serviceProvider, result) =>
            {
                if (!(serviceProvider.GetService(listenerType) is IRabbitConsumerListener listenner))
                    throw new ArgumentNullException(nameof(listenner));

                result.Queue = queue;

                await listenner.ConsumeAsync(result);
            });
        }

        /// <summary>
        /// 添加自定义监听
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddConsumer<T>(this IRabbitConsumerBuilder builder, string exchange, string queue) where T : class, IRabbitConsumerListener
            => builder.AddConsumer(exchange, queue, typeof(T));

        /// <summary>
        /// 添加自定义监听
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddKeyConsumer<T>(this IRabbitConsumerBuilder builder, string queue) where T : class, IRabbitConsumerListener
            => builder.AddDefaultKeyConsumer(queue, typeof(T));

        /// <summary>
        /// 添加自定义监听
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="queue"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddKeyConsumer<T>(this IRabbitConsumerBuilder builder, string queue, string keyName) where T : class, IRabbitConsumerListener
            => builder.AddKeyConsumer(queue, typeof(T), keyName);

        /// <summary>
        /// 添加自定义监听
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddKeyConsumer<T>(this IRabbitConsumerBuilder builder, string exchange, string queue, string keyName) where T : class, IRabbitConsumerListener
            => builder.AddKeyConsumer(exchange, queue, typeof(T), keyName);

        /// <summary>
        /// 添加自定义监听
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddDefaultKeyConsumer<T>(this IRabbitConsumerBuilder builder, string exchange, string queue) where T : class, IRabbitConsumerListener
            => builder.AddDefaultKeyConsumer(exchange, queue, typeof(T));

        /// <summary>
        /// 添加自定义监听
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="queue">队列名称</param>
        /// <param name="listenerType"></param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddDefaultKeyConsumer(this IRabbitConsumerBuilder builder, string queue, Type listenerType)
        {
            if (!typeof(IRabbitConsumerListener).IsAssignableFrom(listenerType) || !listenerType.IsClass || listenerType.IsAbstract)
                throw new ArgumentException("the listener type must be implement IRabbitConsumerListener and none abstract class", "listenerType");

            var keyName = Guid.NewGuid().ToString("N");

            builder.Services.AddKeyedTransient(listenerType, keyName);
            return builder.AddConsumer(queue, async (serviceProvider, result) =>
            {
                var listener = serviceProvider.GetRequiredKeyedService(listenerType, keyName) as IRabbitConsumerListener;

                result.Queue = queue;

                await listener.ConsumeAsync(result);
            });
        }

        /// <summary>
        /// 添加自定义监听
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="keyName"></param>
        /// <param name="queue">队列名称</param>
        /// <param name="listenerType"></param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddKeyConsumer(this IRabbitConsumerBuilder builder, string queue, Type listenerType, string keyName)
        {
            if (!typeof(IRabbitConsumerListener).IsAssignableFrom(listenerType) || !listenerType.IsClass || listenerType.IsAbstract)
                throw new ArgumentException("the listener type must be implement IRabbitConsumerListener and none abstract class", "listenerType");

            builder.Services.AddKeyedTransient(listenerType, keyName);
            return builder.AddConsumer(queue, async (serviceProvider, result) =>
            {
                var listener = serviceProvider.GetRequiredKeyedService(listenerType, keyName) as IRabbitConsumerListener;

                result.Queue = queue;

                await listener.ConsumeAsync(result);
            });
        }

        /// <summary>
        /// 添加自定义监听
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="listenerType"></param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddDefaultKeyConsumer(this IRabbitConsumerBuilder builder, string exchange, string queue, Type listenerType)
        {
            if (!typeof(IRabbitConsumerListener).IsAssignableFrom(listenerType) || !listenerType.IsClass || listenerType.IsAbstract)
                throw new ArgumentException($"the listener type must be implement IRabbitConsumerListener and none abstract class", "listenerType");

            var keyName = Guid.NewGuid().ToString("N");

            builder.Services.AddKeyedTransient(listenerType, keyName);
            return builder.AddConsumer(exchange, queue, async (serviceProvider, result) =>
            {
                var listener = serviceProvider.GetRequiredKeyedService(listenerType, keyName) as IRabbitConsumerListener;

                result.Queue = queue;

                await listener.ConsumeAsync(result);
            });
        }

        /// <summary>
        /// 添加自定义监听
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="listenerType"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddKeyConsumer(this IRabbitConsumerBuilder builder, string exchange, string queue, Type listenerType, string keyName)
        {
            if (!typeof(IRabbitConsumerListener).IsAssignableFrom(listenerType) || !listenerType.IsClass || listenerType.IsAbstract)
                throw new ArgumentException($"the listener type must be implement IRabbitConsumerListener and none abstract class", "listenerType");

            builder.Services.AddKeyedTransient(listenerType, keyName);
            return builder.AddConsumer(exchange, queue, async (serviceProvider, result) =>
            {
                var listener = serviceProvider.GetRequiredKeyedService(listenerType, keyName) as IRabbitConsumerListener;

                result.Queue = queue;

                await listener.ConsumeAsync(result);
            });
        }
    }
}
