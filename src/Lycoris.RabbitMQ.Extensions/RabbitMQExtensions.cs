using Lycoris.RabbitMQ.Extensions.Builder.Consumer;
using Lycoris.RabbitMQ.Extensions.DataModel;
using Microsoft.Extensions.DependencyInjection;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class RabbitMQExtensions
    {
        /// <summary>
        /// 普通的往队列发送消息
        /// </summary>
        /// <param name="producer"></param>
        /// <param name="message"></param>
        public static void Publish(this IRabbitClientProducer producer, string message)
            => producer.Publish(new string[] { message });

        /// <summary>
        /// 使用交换机发送消息
        /// </summary>
        /// <param name="producer"></param>
        /// <param name="routingKey">路由名称</param>
        /// <param name="message"></param>
        public static void Publish(this IRabbitClientProducer producer, string routingKey, string message)
            => producer.Publish(routingKey, new string[] { message });

        /// <summary>
        /// 普通的往队列发送消息
        /// </summary>
        /// <param name="producer"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task PublishAsync(this IRabbitClientProducer producer, string message)
            => await producer.PublishAsync(new string[] { message });

        /// <summary>
        /// 使用交换机发送消息
        /// </summary>
        /// <param name="producer"></param>
        /// <param name="routingKey">路由名称</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task PublishAsync(this IRabbitClientProducer producer, string routingKey, string message)
                => await producer.PublishAsync(routingKey, new string[] { message });

        /// <summary>
        /// 普通的往队列发送消息
        /// </summary>
        /// <param name="producer"></param>
        /// <param name="messages"></param>
        public static async Task PublishAsync(this IRabbitClientProducer producer, string[] messages)
            => await Task.Run(() => { producer.Publish(messages); });

        /// <summary>
        /// 使用交换机发送消息
        /// </summary>
        /// <param name="producer"></param>
        /// <param name="routingKey">路由名称</param>
        /// <param name="messages"></param>
        public static async Task PublishAsync(this IRabbitClientProducer producer, string routingKey, string[] messages)
            => await Task.Run(() => { producer.Publish(routingKey, messages); });

        /// <summary>
        /// 添加队列监听
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="queue">队列名称</param>
        /// <param name="onMessageRecieved"></param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddListener(this IRabbitConsumerBuilder builder, string queue, Action<RecieveResult> onMessageRecieved)
            => builder.AddListener(queue, (_, result) => { onMessageRecieved?.Invoke(result); });

        /// <summary>
        /// 添加交换机监听
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="onMessageRecieved"></param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddListener(this IRabbitConsumerBuilder builder, string exchange, string queue, Action<RecieveResult> onMessageRecieved)
            => builder.AddListener(exchange, queue, (_, result) => { onMessageRecieved?.Invoke(result); });

        /// <summary>
        /// 添加自定义监听
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="queue">队列名称</param>
        /// <param name="listenerType"></param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddListener(this IRabbitConsumerBuilder builder, string queue, Type listenerType)
        {
            if (!typeof(IRabbitConsumerListener).IsAssignableFrom(listenerType) || !listenerType.IsClass || listenerType.IsAbstract)
                throw new ArgumentException("the listener type must be implement IRabbitConsumerListener and none abstract class", "listenerType");

            builder.Services.AddTransient(listenerType);
            return builder.AddListener(queue, (serviceProvider, result) =>
            {
                var listenner = serviceProvider.GetService(listenerType) as IRabbitConsumerListener;

                if (listenner == null)
                    throw new ArgumentNullException(nameof(listenner));

                listenner.ConsumeAsync(result).Wait();
            });
        }

        /// <summary>
        /// 添加自定义监听
        /// </summary>
        /// <param name="builder"></param>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue">队列名称</param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddListener<T>(this IRabbitConsumerBuilder builder, string queue) where T : class, IRabbitConsumerListener
            => builder.AddListener(queue, typeof(T));

        /// <summary>
        /// 添加自定义监听
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="listenerType"></param>
        /// <returns></returns>
        public static IRabbitConsumerBuilder AddListener(this IRabbitConsumerBuilder builder, string exchange, string queue, Type listenerType)
        {
            if (!typeof(IRabbitConsumerListener).IsAssignableFrom(listenerType) || !listenerType.IsClass || listenerType.IsAbstract)
                throw new ArgumentException($"the listener type must be implement IRabbitConsumerListener and none abstract class", "listenerType");

            builder.Services.AddTransient(listenerType);
            return builder.AddListener(exchange, queue, (serviceProvider, result) =>
            {
                var listenner = serviceProvider.GetService(listenerType) as IRabbitConsumerListener;

                if (listenner == null)
                    throw new ArgumentNullException(nameof(listenner));

                listenner.ConsumeAsync(result).Wait();
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
        public static IRabbitConsumerBuilder AddListener<T>(this IRabbitConsumerBuilder builder, string exchange, string queue) where T : class, IRabbitConsumerListener
            => builder.AddListener(exchange, queue, typeof(T));
    }
}
