using Lycoris.RabbitMQ.Extensions.Builder.Consumer;
using Lycoris.RabbitMQ.Extensions.DataModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions
{
    /// <summary>
    /// RabbitMQ 便捷扩展方法集合，提供发布消息、添加消费者监听器以及键控消费者注册等常用操作的简化 API。
    /// 将单条消息包装为数组发送，并支持基于 <see cref="IRabbitConsumerListener"/> 接口的强类型消费者注册。
    /// </summary>
    public static class RabbitMqExtension
    {
        /// <summary>
        /// 便捷扩展方法，将单条消息包装为数组后发送到队列。
        /// </summary>
        /// <param name="producer">生产者实例</param>
        /// <param name="message">待发送的消息内容</param>
        public static Task PublishAsync(this IRabbitClientProducer producer, string message)
            => producer.PublishAsync(new string[] { message });

        /// <summary>
        /// 便捷扩展方法，将单条消息包装为数组后通过交换机发送。
        /// </summary>
        /// <param name="producer">生产者实例</param>
        /// <param name="routingKey">路由键名称</param>
        /// <param name="message">待发送的消息内容</param>
        public static Task PublishAsync(this IRabbitClientProducer producer, string routingKey, string message)
            => producer.PublishAsync(routingKey, new string[] { message });

        /// <summary>
        /// 便捷扩展方法，将单条消息包装为数组后发送到队列，并支持取消操作。
        /// </summary>
        /// <param name="producer">生产者实例</param>
        /// <param name="message">待发送的消息内容</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static Task PublishAsync(this IRabbitClientProducer producer, string message, CancellationToken cancellationToken)
            => producer.PublishAsync(new string[] { message }, cancellationToken);

        /// <summary>
        /// 便捷扩展方法，将单条消息包装为数组后通过交换机发送，并支持取消操作。
        /// </summary>
        /// <param name="producer">生产者实例</param>
        /// <param name="routingKey">路由键名称</param>
        /// <param name="message">待发送的消息内容</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static Task PublishAsync(this IRabbitClientProducer producer, string routingKey, string message, CancellationToken cancellationToken)
            => producer.PublishAsync(routingKey, new string[] { message }, cancellationToken);

        /// <summary>
        /// 添加队列监听，使用委托回调处理接收到的消息。
        /// 将简单的 <see cref="RecieveResult"/> 回调委托适配为完整的 <see cref="IRabbitConsumerBuilder"/> 监听签名。
        /// </summary>
        /// <param name="builder">消费者构建器</param>
        /// <param name="queue">队列名称</param>
        /// <param name="onMessageRecieved">消息接收回调委托</param>
        /// <returns>消费者构建器，支持链式调用</returns>
        public static IRabbitConsumerBuilder AddConsumer(this IRabbitConsumerBuilder builder, string queue, Func<RecieveResult, Task> onMessageRecieved)
            => builder.AddConsumer(queue, async (_, result) => { await onMessageRecieved?.Invoke(result); });

        /// <summary>
        /// 添加交换机监听，使用委托回调处理接收到的消息。
        /// 将简单的 <see cref="RecieveResult"/> 回调委托适配为完整的 <see cref="IRabbitConsumerBuilder"/> 监听签名。
        /// </summary>
        /// <param name="builder">消费者构建器</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="onMessageRecieved">消息接收回调委托</param>
        /// <returns>消费者构建器，支持链式调用</returns>
        public static IRabbitConsumerBuilder AddConsumer(this IRabbitConsumerBuilder builder, string exchange, string queue, Func<RecieveResult, Task> onMessageRecieved)
            => builder.AddConsumer(exchange, queue, async (_, result) => { await onMessageRecieved?.Invoke(result); });

        /// <summary>
        /// 通过监听器类型注册队列消费者。该类型必须实现 <see cref="IRabbitConsumerListener"/> 接口且为非抽象类。
        /// 监听器类型会以 Transient 生命周期注册到 DI 容器，每次消费消息时从容器中解析出新实例。
        /// </summary>
        /// <param name="builder">消费者构建器</param>
        /// <param name="queue">队列名称</param>
        /// <param name="listenerType">实现 <see cref="IRabbitConsumerListener"/> 的监听器类型</param>
        /// <returns>消费者构建器，支持链式调用</returns>
        /// <exception cref="ArgumentException">当 listenerType 未实现 <see cref="IRabbitConsumerListener"/> 或为抽象类时抛出</exception>
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
        /// 通过泛型监听器类型注册队列消费者。该类型必须实现 <see cref="IRabbitConsumerListener"/> 接口且为非抽象类。
        /// 监听器类型会以 Transient 生命周期注册到 DI 容器，每次消费消息时从容器中解析出新实例。
        /// </summary>
        /// <typeparam name="T">实现 <see cref="IRabbitConsumerListener"/> 的监听器泛型类型</typeparam>
        /// <param name="builder">消费者构建器</param>
        /// <param name="queue">队列名称</param>
        /// <returns>消费者构建器，支持链式调用</returns>
        public static IRabbitConsumerBuilder AddConsumer<T>(this IRabbitConsumerBuilder builder, string queue) where T : class, IRabbitConsumerListener
            => builder.AddConsumer(queue, typeof(T));

        /// <summary>
        /// 通过监听器类型注册交换机和队列消费者。该类型必须实现 <see cref="IRabbitConsumerListener"/> 接口且为非抽象类。
        /// 监听器类型会以 Transient 生命周期注册到 DI 容器，每次消费消息时从容器中解析出新实例。
        /// </summary>
        /// <param name="builder">消费者构建器</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="listenerType">实现 <see cref="IRabbitConsumerListener"/> 的监听器类型</param>
        /// <returns>消费者构建器，支持链式调用</returns>
        /// <exception cref="ArgumentException">当 listenerType 未实现 <see cref="IRabbitConsumerListener"/> 或为抽象类时抛出</exception>
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
        /// 通过泛型监听器类型注册交换机和队列消费者。该类型必须实现 <see cref="IRabbitConsumerListener"/> 接口且为非抽象类。
        /// 监听器类型会以 Transient 生命周期注册到 DI 容器，每次消费消息时从容器中解析出新实例。
        /// </summary>
        /// <typeparam name="T">实现 <see cref="IRabbitConsumerListener"/> 的监听器泛型类型</typeparam>
        /// <param name="builder">消费者构建器</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <returns>消费者构建器，支持链式调用</returns>
        public static IRabbitConsumerBuilder AddConsumer<T>(this IRabbitConsumerBuilder builder, string exchange, string queue) where T : class, IRabbitConsumerListener
            => builder.AddConsumer(exchange, queue, typeof(T));

        /// <summary>
        /// 使用自动生成的 GUID 作为键名前缀注册键控队列消费者。
        /// 适用于同一种 <see cref="IRabbitConsumerListener"/> 实现需要被多个队列使用但希望 DI 容器中保持独立实例的场景。
        /// </summary>
        /// <typeparam name="T">实现 <see cref="IRabbitConsumerListener"/> 的监听器泛型类型</typeparam>
        /// <param name="builder">消费者构建器</param>
        /// <param name="queue">队列名称</param>
        /// <returns>消费者构建器，支持链式调用</returns>
        public static IRabbitConsumerBuilder AddKeyConsumer<T>(this IRabbitConsumerBuilder builder, string queue) where T : class, IRabbitConsumerListener
            => builder.AddDefaultKeyConsumer(queue, typeof(T));

        /// <summary>
        /// 通过键控服务注册队列消费者，使用指定的键名区分同一类型的多个监听器实例。
        /// 监听器类型通过 <c>AddKeyedTransient</c> 注册到 DI 容器，在消费时通过 <c>GetRequiredKeyedService</c> 解析。
        /// </summary>
        /// <typeparam name="T">实现 <see cref="IRabbitConsumerListener"/> 的监听器泛型类型</typeparam>
        /// <param name="builder">消费者构建器</param>
        /// <param name="queue">队列名称</param>
        /// <param name="keyName">键名，用于在 DI 容器中区分同一类型的不同实例</param>
        /// <returns>消费者构建器，支持链式调用</returns>
        public static IRabbitConsumerBuilder AddKeyConsumer<T>(this IRabbitConsumerBuilder builder, string queue, string keyName) where T : class, IRabbitConsumerListener
            => builder.AddKeyConsumer(queue, typeof(T), keyName);

        /// <summary>
        /// 通过键控服务注册交换机和队列消费者，使用指定的键名区分同一类型的多个监听器实例。
        /// 监听器类型通过 <c>AddKeyedTransient</c> 注册到 DI 容器，在消费时通过 <c>GetRequiredKeyedService</c> 解析。
        /// </summary>
        /// <typeparam name="T">实现 <see cref="IRabbitConsumerListener"/> 的监听器泛型类型</typeparam>
        /// <param name="builder">消费者构建器</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="keyName">键名，用于在 DI 容器中区分同一类型的不同实例</param>
        /// <returns>消费者构建器，支持链式调用</returns>
        public static IRabbitConsumerBuilder AddKeyConsumer<T>(this IRabbitConsumerBuilder builder, string exchange, string queue, string keyName) where T : class, IRabbitConsumerListener
            => builder.AddKeyConsumer(exchange, queue, typeof(T), keyName);

        /// <summary>
        /// 使用自动生成的 GUID 作为键名前缀注册键控交换机和队列消费者。
        /// 适用于同一种 <see cref="IRabbitConsumerListener"/> 实现需要被多个交换机/队列使用但希望 DI 容器中保持独立实例的场景。
        /// </summary>
        /// <typeparam name="T">实现 <see cref="IRabbitConsumerListener"/> 的监听器泛型类型</typeparam>
        /// <param name="builder">消费者构建器</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <returns>消费者构建器，支持链式调用</returns>
        public static IRabbitConsumerBuilder AddDefaultKeyConsumer<T>(this IRabbitConsumerBuilder builder, string exchange, string queue) where T : class, IRabbitConsumerListener
            => builder.AddDefaultKeyConsumer(exchange, queue, typeof(T));

        /// <summary>
        /// 使用自动生成的 GUID 作为键名前缀，通过监听器类型注册键控队列消费者。
        /// 键名由 GUID 自动生成，确保 DI 容器中每个监听器实例都是独立的。
        /// </summary>
        /// <param name="builder">消费者构建器</param>
        /// <param name="queue">队列名称</param>
        /// <param name="listenerType">实现 <see cref="IRabbitConsumerListener"/> 的监听器类型</param>
        /// <returns>消费者构建器，支持链式调用</returns>
        /// <exception cref="ArgumentException">当 listenerType 未实现 <see cref="IRabbitConsumerListener"/> 或为抽象类时抛出</exception>
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
        /// 通过监听器类型和指定键名注册键控队列消费者。
        /// 监听器类型通过 <c>AddKeyedTransient</c> 注册到 DI 容器，在消费时通过 <c>GetRequiredKeyedService</c> 按键名解析。
        /// </summary>
        /// <param name="builder">消费者构建器</param>
        /// <param name="queue">队列名称</param>
        /// <param name="listenerType">实现 <see cref="IRabbitConsumerListener"/> 的监听器类型</param>
        /// <param name="keyName">键名，用于在 DI 容器中区分同一类型的不同实例</param>
        /// <returns>消费者构建器，支持链式调用</returns>
        /// <exception cref="ArgumentException">当 listenerType 未实现 <see cref="IRabbitConsumerListener"/> 或为抽象类时抛出</exception>
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
        /// 使用自动生成的 GUID 作为键名前缀，通过监听器类型注册键控交换机和队列消费者。
        /// 键名由 GUID 自动生成，确保 DI 容器中每个监听器实例都是独立的。
        /// </summary>
        /// <param name="builder">消费者构建器</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="listenerType">实现 <see cref="IRabbitConsumerListener"/> 的监听器类型</param>
        /// <returns>消费者构建器，支持链式调用</returns>
        /// <exception cref="ArgumentException">当 listenerType 未实现 <see cref="IRabbitConsumerListener"/> 或为抽象类时抛出</exception>
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
        /// 通过监听器类型和指定键名注册键控交换机和队列消费者。
        /// 监听器类型通过 <c>AddKeyedTransient</c> 注册到 DI 容器，在消费时通过 <c>GetRequiredKeyedService</c> 按键名解析。
        /// </summary>
        /// <param name="builder">消费者构建器</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="listenerType">实现 <see cref="IRabbitConsumerListener"/> 的监听器类型</param>
        /// <param name="keyName">键名，用于在 DI 容器中区分同一类型的不同实例</param>
        /// <returns>消费者构建器，支持链式调用</returns>
        /// <exception cref="ArgumentException">当 listenerType 未实现 <see cref="IRabbitConsumerListener"/> 或为抽象类时抛出</exception>
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
