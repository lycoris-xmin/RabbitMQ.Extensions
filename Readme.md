# Lycoris.RabbitMQ.Extensions

[![NuGet](https://img.shields.io/nuget/v/Lycoris.RabbitMQ.Extensions.svg)](https://www.nuget.org/packages/Lycoris.RabbitMQ.Extensions)

RabbitMQ 使用扩展库，简化生产者与消费者的使用，支持普通队列、工作队列、发布/订阅、路由、Topic、延迟队列、死信队列等所有 RabbitMQ 标准模式。

### 功能特性

- 普通队列 / 工作队列模式
- 发布/订阅模式、路由模式、Topic 模式
- 延迟队列（需安装 `rabbitmq_delayed_message_exchange` 插件）
- 死信队列（DLX）支持
- 连接断线自动恢复（指数退避重连）
- 消费者指数退避重试
- 生产者 Channel 池化复用
- 健康检查服务
- 键控依赖注入（Keyed DI）消费者支持
- `CancellationToken` 超时/取消支持
- `IAsyncDisposable` 异步释放支持
- 手动控制消费者启停

---

## 安装方式

```shell
// .NET CLI
dotnet add package Lycoris.RabbitMQ.Extensions

// Package Manager
Install-Package Lycoris.RabbitMQ.Extensions
```

### 延迟队列插件

如需使用延迟队列，请先安装 RabbitMQ 延迟消息插件：
**[rabbitmq_delayed_message_exchange](https://github.com/rabbitmq/rabbitmq-delayed-message-exchange)**

---

## 一、注册扩展

### 1.1 基础连接信息注册

```csharp
var mqBuilder = builder.Services.AddRabbitMQExtensions(opt =>
{
    // RabbitMQ 服务地址（支持 hostname、IP、host:port 格式）
    opt.Hosts = new string[] { "192.168.1.100:5672", "192.168.1.101:5672" };
    // 端口号，默认 5672
    opt.Port = 5672;
    // 账号，默认 guest
    opt.UserName = "guest";
    // 密码，默认 guest
    opt.Password = "guest";
    // 虚拟主机，默认 /
    opt.VirtualHost = "/";
    // 队列/交换机是否持久化，默认 true
    opt.Durable = true;
    // 队列/交换机是否自动删除，默认 false
    opt.AutoDelete = false;

    // ----- 可选配置 -----

    // 全局自定义参数
    opt.Arguments = new Dictionary<string, object>
    {
        { "x-queue-type", "classic" }
    };

    // 全局消息基础属性（将作为消息 Headers 发送）
    opt.BasicProps = new Dictionary<string, object>
    {
        { "x-custom-header", "value" }
    };

    // 禁用消费者自启动监听（见第六节）
    opt.DisableRabbitConsumerHostedListen = true;

    // 注册管理 API 服务（监控用）
    opt.AddApiService();

    // 注册健康检查服务
    opt.AddHealthCheck();

    // 注册自定义事件处理器
    opt.AddEventHandler<YourEventHandler>();
});
```

### 1.2 生产者注册

#### 生产者工厂模式（手动获取生产者）

```csharp
mqBuilder.AddRabbitProducer("MyProducer", opt =>
{
    opt.InitializeCount = 5;
    opt.Exchange = "exchange.order";
    opt.Type = RabbitExchangeType.Direct;
    opt.RouteQueues = new RouteQueue[]
    {
        new RouteQueue
        {
            Route = "route.order.created",
            Queue = "queue.order.created"
        }
    };
});
```

#### 生产者服务模式（依赖注入，生命周期 Scoped）

**单实现模式：**

```csharp
mqBuilder.AddRabbitProducer(opt =>
{
    opt.InitializeCount = 5;
    opt.Exchange = "exchange.order";
    opt.Type = RabbitExchangeType.Direct;
    opt.RouteQueues = new RouteQueue[]
    {
        new RouteQueue { Route = "route.a", Queue = "queue.a" },
        new RouteQueue { Route = "route.b", Queue = "queue.b" }
    };

    // 注册生产者服务
    opt.AddRabbitProducer<OrderProducerService>();
});
```

**接口实现模式：**

```csharp
mqBuilder.AddRabbitProducer(opt =>
{
    opt.InitializeCount = 5;
    opt.Exchange = "exchange.order";
    opt.Type = RabbitExchangeType.Delayed;
    opt.DelayTime = 5; // 延迟秒数
    opt.RouteQueues = new RouteQueue[]
    {
        new RouteQueue { Route = "route.a", Queue = "queue.a" }
    };

    opt.AddRabbitProducer<IOrderProducer, OrderProducerService>();
});
```

### 1.3 消费者注册

```csharp
mqBuilder.AddRabbitConsumer(opt =>
{
    // 是否自动 Ack，默认 false（需手动确认）
    opt.AutoAck = false;
    // 每次拉取消息条数，默认 2
    opt.FetchCount = 2;
    // 交换机类型
    opt.Type = RabbitExchangeType.Direct;
    // 路由队列
    opt.RouteQueues = new RouteQueue[]
    {
        new RouteQueue { Route = "route.a", Queue = "queue.a" },
        new RouteQueue { Route = "route.b", Queue = "queue.b" }
    };

    // 添加消费者监听
    // 普通队列模式
    opt.AddConsumer<OrderConsumer>("queue.order.created");
    // 交换机模式
    opt.AddConsumer<OrderConsumer>("exchange.order", "queue.order.created");
    // 回调函数模式
    opt.AddConsumer("queue.order.created", async result =>
    {
        Console.WriteLine($"收到消息: {result.Body}");
        result.Commit();
    });
});
```

---

## 二、生产者使用示例

### 2.1 工厂模式发送

```csharp
public class OrderService
{
    private readonly IRabbitProducerFactory _factory;

    public OrderService(IRabbitProducerFactory factory)
    {
        _factory = factory;
    }

    public async Task SendOrderCreated()
    {
        var producer = _factory.Create("MyProducer");

        // 通过路由键发送单条消息
        await producer.PublishAsync("route.order.created", "订单创建成功");

        // 通过路由键发送多条消息
        await producer.PublishAsync("route.order.created",
            new[] { "消息1", "消息2", "消息3" });

        // 带取消令牌
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await producer.PublishAsync("route.order.created", "消息", cts.Token);
    }
}
```

### 2.2 服务模式发送

```csharp
public class OrderProducerService : BaseRabbitProducerService, IOrderProducer
{
    public OrderProducerService(IRabbitProducerFactory factory) : base(factory) { }

    public async Task SendOrderCreated()
    {
        // 直接使用 Producer 属性发送
        await Producer.PublishAsync("route.a", "发往 route.a");
        await Producer.PublishAsync("route.b", "发往 route.b");
    }

    public async Task SendWithDelay()
    {
        // 如果在配置中设置了延迟交换机，消息会按 DelayTime 延迟投递
        await Producer.PublishAsync("route.a", "延迟消息");
    }
}
```

### 2.3 RabbitProducer 直接使用（低级 API）

```csharp
var producer = new RabbitProducer("192.168.1.100:5672")
{
    UserName = "guest",
    Password = "guest",
    VirtualHost = "/"
};

// 普通队列模式
await producer.PublishAsync("queue.test", "单条消息");

await producer.PublishAsync("queue.test", new[] { "消息1", "消息2" });

// 带自定义 BasicProperties（设置 Headers、CorrelationId 等）
var props = new BasicProperties
{
    CorrelationId = Guid.NewGuid().ToString(),
    ReplyTo = "reply.queue"
};
await producer.PublishAsync("queue.test", "消息", basicProperties: props);

// 交换机模式
await producer.PublishAsync("exchange.test", "route.key", "消息");

// 交换机模式 + 路由消息数组（每条消息可指定不同路由键）
var routeMessages = new RouteMessage[]
{
    new RouteMessage { RoutingKey = "route.a", Message = "消息A" },
    new RouteMessage { RoutingKey = "route.b", Message = "消息B" }
};
await producer.PublishAsync("exchange.test", routeMessages, new ExchangeQueueOptions
{
    Type = RabbitExchangeType.Direct
});

// 带 CancellationToken
await producer.PublishAsync("queue.test", "消息", cancellationToken: cts.Token);

// 释放
await producer.DisposeAsync();
```

---

## 三、消费者使用示例

### 3.1 继承 BaseRabbitConsumer（推荐）

内置指数退避重试、异常捕捉和死信队列对接。

```csharp
public class OrderConsumer : BaseRabbitConsumer
{
    public OrderConsumer()
    {
        // 重试配置（可选，以下均为默认值）
        ResubmitTimeSpan = 1000;     // 基础重试延迟（ms）
        MaxResubmitTimeSpan = 60000; // 最大重试延迟（ms）
        MaxRetryCount = 3;           // 最大重试次数
    }

    /// <summary>
    /// 处理消息
    /// </summary>
    /// <param name="body">消息体</param>
    /// <returns>
    /// Commit   - 提交（Ack，消息从队列移除）
    /// RollBack - 回滚（Nack，不重新入队，若配置了 DLX 则进入死信队列）
    /// Resubmit - 重新投递（Nack 并重新入队，延迟按指数退避递增）
    /// </returns>
    protected override Task<ReceivedHandler> ReceivedAsync(string body)
    {
        Console.WriteLine($"收到消息: {body}");
        Console.WriteLine($"来源队列: {Queue}");
        Console.WriteLine($"来源交换机: {Exchange}");
        Console.WriteLine($"路由键: {Route}");
        Console.WriteLine($"消息ID: {Context.MessageId}");
        Console.WriteLine($"是否重投递: {Context.Redelivered}");

        // 从消息头读取自定义数据
        if (Context.Headers.TryGetValue("custom-key", out var val))
            Console.WriteLine($"自定义头: {val}");

        // 处理成功，提交
        return Task.FromResult(ReceivedHandler.Commit);
    }

    /// <summary>
    /// 全局异常处理（可选重写）
    /// </summary>
    protected override Task<ReceivedHandler> HandleExceptionAsync(Exception exception)
    {
        _logger.LogError(exception, "消息处理异常");

        // 返回 RollBack（不重新入队，进入 DLX）
        return Task.FromResult(ReceivedHandler.RollBack);

        // 或返回 Resubmit（重新入队，延迟重试）
        // return Task.FromResult(ReceivedHandler.Resubmit);
    }
}
```

### 3.2 实现 IRabbitConsumerListener 接口（完全自定义）

```csharp
public class CustomConsumer : IRabbitConsumerListener
{
    public async Task ConsumeAsync(RecieveResult recieveResult)
    {
        try
        {
            Console.WriteLine($"消息: {recieveResult.Body}");

            // 提交
            recieveResult.Commit();

            // 回滚（不重新入队，若有 DLX 则进入死信队列）
            // recieveResult.RollBack();

            // 重新投递（重新入队）
            // recieveResult.RollBack(requeue: true);
        }
        catch (Exception ex)
        {
            // 异常处理
            recieveResult.RollBack();
        }
    }
}
```

### 3.3 回调函数模式（简单场景）

```csharp
mqBuilder.AddRabbitConsumer(opt =>
{
    opt.AddConsumer("queue.order.created", async result =>
    {
        Console.WriteLine($"消息: {result.Body}");
        result.Commit();
    });

    opt.AddConsumer("exchange.order", "queue.order.created", async result =>
    {
        Console.WriteLine($"消息: {result.Body}, 路由键: {result.Route}");
        result.Commit();
    });
});
```

---

## 四、延迟队列使用

### 4.1 生产者配置延迟队列

```csharp
mqBuilder.AddRabbitProducer(opt =>
{
    opt.Exchange = "exchange.delayed";
    // 设置为延迟交换机类型
    opt.Type = RabbitExchangeType.Delayed;
    // 延迟时间（秒）
    opt.DelayTime = 10;
    // 可自定义延迟交换机类型（默认 direct）
    opt.Arguments = new Dictionary<string, object>
    {
        { "x-delayed-type", "topic" } // 延迟 topic 交换机
    };
    opt.RouteQueues = new RouteQueue[]
    {
        new RouteQueue { Route = "route.order", Queue = "queue.order" }
    };

    opt.AddRabbitProducer<DelayProducerService>();
});
```

### 4.2 消费者监听延迟队列

```csharp
mqBuilder.AddRabbitConsumer(opt =>
{
    opt.Type = RabbitExchangeType.Delayed;
    opt.RouteQueues = new RouteQueue[]
    {
        new RouteQueue { Route = "route.order", Queue = "queue.order" }
    };

    opt.AddConsumer<DelayConsumer>("exchange.delayed", "queue.order");
});
```

### 4.3 运行时动态延迟

```csharp
// 对于非全局固定延迟的场景，可通过 Arguments 在运行时覆盖
var props = new BasicProperties();
props.Headers = new Dictionary<string, object>
{
    { "x-delay", 30000 } // 30 秒延迟（毫秒）
};

var producer = new RabbitProducer("host:5672");
await producer.PublishAsync("queue.test", "消息", basicProperties: props);
```

---

## 五、死信队列（DLX）使用

### 5.1 配置 DLX

```csharp
mqBuilder.AddRabbitConsumer(opt =>
{
    // ... 基础配置 ...

    // 配置死信交换机（全局，传播到所有路由队列）
    opt.DeadLetterExchange = new DeadLetterExchangeOption
    {
        Exchange = "exchange.dlx",
        RoutingKey = "route.dlx" // 可选，默认使用原消息路由键
    };

    opt.AddConsumer<OrderConsumer>("exchange.order", "queue.order");
});
```

### 5.2 为单个路由队列配置独立的 DLX

```csharp
mqBuilder.AddRabbitConsumer(opt =>
{
    opt.Type = RabbitExchangeType.Direct;
    opt.RouteQueues = new RouteQueue[]
    {
        new RouteQueue
        {
            Route = "route.order",
            Queue = "queue.order",
            Options = new QueueOption
            {
                DeadLetterExchange = new DeadLetterExchangeOption
                {
                    Exchange = "exchange.dlx.order",
                    RoutingKey = "route.dlx.order"
                }
            }
        }
    };

    opt.AddConsumer<OrderConsumer>("exchange.order", "queue.order");
});
```

### 5.3 DLX 消费处理

```csharp
// 创建 DLX 消费者监听死信消息
mqBuilder.AddRabbitConsumer(dlxOpt =>
{
    dlxOpt.Type = RabbitExchangeType.Direct;
    dlxOpt.RouteQueues = new RouteQueue[]
    {
        new RouteQueue { Route = "route.dlx", Queue = "queue.dlx" }
    };
    dlxOpt.AddConsumer<DeadLetterConsumer>("exchange.dlx", "queue.dlx");
});

public class DeadLetterConsumer : BaseRabbitConsumer
{
    protected override Task<ReceivedHandler> ReceivedAsync(string body)
    {
        Console.WriteLine($"死信消息: {body}");
        Console.WriteLine($"原队列: {Context.Headers.GetValueOrDefault("x-first-death-queue")}");
        Console.WriteLine($"死亡原因: {Context.Headers.GetValueOrDefault("x-first-death-reason")}");

        // 记录或告警后提交
        return Task.FromResult(ReceivedHandler.Commit);
    }
}
```

### 5.4 DLX + 重试次数上限

```csharp
// 配合 BaseRabbitConsumer 的 MaxRetryCount
// 重试 N 次后自动 RollBack(false)，消息进入 DLX
public class OrderConsumer : BaseRabbitConsumer
{
    public OrderConsumer()
    {
        MaxRetryCount = 3; // 重试 3 次后进入 DLX
    }

    protected override Task<ReceivedHandler> ReceivedAsync(string body)
    {
        // 处理逻辑。返回 Resubmit 会触发重试
        return Task.FromResult(ReceivedHandler.Resubmit);
    }
}
```

---

## 六、手动控制消费者启停

```csharp
// 注册时禁用自动启动
mqBuilder.DisableRabbitConsumerHostedListen = true;
```

```csharp
public class ApplicationStartService : IHostedService
{
    private readonly IRabbitConsumerFactory _consumerFactory;

    public ApplicationStartService(IRabbitConsumerFactory consumerFactory)
    {
        _consumerFactory = consumerFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // 前置操作：数据库迁移、缓存预热等

        // 启动所有消费者监听
        await _consumerFactory.ManualStartListenAsync();

        // 或启动指定队列
        // await _consumerFactory.ManualStartListenAsync("queue.order");

        // 或启动指定交换机+队列
        // await _consumerFactory.ManualStartListenAsync("exchange.order", "queue.order");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // 停止所有监听
        await _consumerFactory.ManualStopListenAsync(cancellationToken);

        // 或停止指定队列
        // await _consumerFactory.ManualStopListenAsync("queue.order", cancellationToken);

        // 或停止指定交换机+队列
        // await _consumerFactory.ManualStopListenAsync("exchange.order", "queue.order", cancellationToken);
    }
}
```

---

## 七、健康检查

```csharp
// 注册
opt.AddHealthCheck();
```

```csharp
public class HealthController : ControllerBase
{
    private readonly RabbitMqHealthCheckService _healthCheck;

    public HealthController(RabbitMqHealthCheckService healthCheck)
    {
        _healthCheck = healthCheck;
    }

    [HttpGet("/health/rabbitmq")]
    public async Task<IActionResult> Check()
    {
        // 检查所有节点
        var result = await _healthCheck.CheckHealthAsync();

        // 或检查指定配置
        // var result = await _healthCheck.CheckHealthAsync("MyConfigName");

        if (result.IsHealthy)
            return Ok(result);

        return StatusCode(503, result);
    }
}

// 返回值结构：
// result.IsHealthy    - 整体是否健康
// result.Description  - 描述
// result.Details[0].Host       - 节点地址
// result.Details[0].Port       - 端口
// result.Details[0].IsHealthy  - 节点健康状态
// result.Details[0].Error      - 错误信息
```

### 包装为 ASP.NET Core IHealthCheck

```csharp
public class RabbitMqHealthCheck : IHealthCheck
{
    private readonly RabbitMqHealthCheckService _service;

    public RabbitMqHealthCheck(RabbitMqHealthCheckService service) => _service = service;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken ct = default)
    {
        var result = await _service.CheckHealthAsync();
        return result.IsHealthy
            ? HealthCheckResult.Healthy(result.Description)
            : HealthCheckResult.Unhealthy(result.Description);
    }
}

// 注册
builder.Services.AddHealthChecks().AddCheck<RabbitMqHealthCheck>("rabbitmq");
```

---

## 八、高级用法

### 8.1 键控消费者（Keyed DI）

```csharp
// 使用指定键名注册
mqBuilder.AddRabbitConsumer(opt =>
{
    opt.AddKeyConsumer<OrderConsumer>("myKey", "queue.order");
});

// 使用自动生成的 GUID 键名
mqBuilder.AddRabbitConsumer(opt =>
{
    opt.AddDefaultKeyConsumer<OrderConsumer>("queue.order");
});
```

### 8.2 自定义事件处理

```csharp
public class MyEventHandler : IRabbitMqEventHandler
{
    private readonly ILogger<MyEventHandler> _logger;

    public MyEventHandler(ILogger<MyEventHandler> logger) => _logger = logger;

    public Task CallbackExceptionAsync(object sender, CallbackExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "RabbitMQ 回调异常");
        return Task.CompletedTask;
    }

    public Task ConnectionShutdownAsync(object sender, ShutdownEventArgs e)
    {
        _logger.LogWarning("RabbitMQ 连接断开: {ReplyText}", e.ReplyText);
        return Task.CompletedTask;
    }
}

// 注册
mqBuilder.AddEventHandler<MyEventHandler>();
```

### 8.3 连接恢复配置

```csharp
// 通过 RabbitProducer 直接设置
var producer = new RabbitProducer("host:5672")
{
    MaxReconnectAttempts = 5,  // 最大重连次数（默认 3）
    ReconnectDelay = 2000      // 重连基础延迟 ms（默认 1000）
};
// 重连延迟按指数退避：2000 → 4000 → 8000 → 16000 → 32000 ms
```

### 8.4 管理 API 监控

```csharp
// 注册
opt.AddApiService();
```

```csharp
public class MonitorService
{
    private readonly IRabbitMqApiService _api;

    public MonitorService(IRabbitMqApiService api) => _api = api;

    public async Task<RabbitMqMonitorResponse> GetOverview()
    {
        var request = new RabbitMqMonitorRequest
        {
            Host = "192.168.1.100",
            Prot = 15672,
            UserName = "guest",
            Passwrod = "guest"
        };

        var result = await _api.MonitorApiAsync(request);

        Console.WriteLine($"版本: {result.RabbitmqVersion}");
        Console.WriteLine($"集群: {result.ClusterName}");
        Console.WriteLine($"消息速率: {result.MessageStats?.PublishDetails?.Rate}");

        return result;
    }
}
```

---

## 九、配置选项速查

### RabbitExchangeType 交换机类型

| 值 | 说明 |
|----|------|
| `None` | 普通队列模式（不使用交换机） |
| `Direct` | 路由模式 |
| `Fanout` | 发布/订阅模式 |
| `Topic` | 匹配订阅模式 |
| `Delayed` | 延迟交换机（需安装插件） |

### ReceivedHandler 消息处理结果

| 值 | 说明 |
|----|------|
| `Commit` | 提交，从队列中移除消息 |
| `RollBack` | 回滚，不重新入队（若配置 DLX 则进入死信队列） |
| `Resubmit` | 重新投递，消息重新入队，延迟按指数退避递增 |

### RecieveResult 消息上下文

| 属性 | 说明 |
|------|------|
| `Body` | 消息体 |
| `MessageId` | 消息编号 |
| `Exchange` | 来源交换机 |
| `Queue` | 来源队列 |
| `Route` | 路由键 |
| `Redelivered` | 是否重复投递 |
| `Headers` | 消息头 |
| `Commit()` | 确认消息 |
| `RollBack(requeue)` | 回滚消息，`requeue=true` 重新入队 |

### BaseRabbitConsumer 可配置属性

| 属性 | 默认值 | 说明 |
|------|--------|------|
| `ResubmitTimeSpan` | 1000 ms | 重试基础延迟 |
| `MaxResubmitTimeSpan` | 60000 ms | 重试最大延迟 |
| `MaxRetryCount` | 3 | 最大重试次数，超限后进入 DLX |

### BaseRabbit 连接配置

| 属性 | 默认值 | 说明 |
|------|--------|------|
| `Port` | 5672 | AMQP 端口 |
| `UserName` | guest | 账号 |
| `Password` | guest | 密码 |
| `VirtualHost` | / | 虚拟主机 |
| `MaxReconnectAttempts` | 3 | 最大重连次数 |
| `ReconnectDelay` | 1000 ms | 重连基础延迟 |

---

## 十、使用 IRabbitConsumerListener 与 BaseRabbitConsumer 的对比

| | BaseRabbitConsumer | IRabbitConsumerListener |
|---|---|---|
| 使用方式 | 继承基类，重写 `ReceivedAsync` | 实现接口，实现 `ConsumeAsync` |
| 自动 Ack/Nack | 根据返回值自动处理 | 需手动调用 `Commit()` / `RollBack()` |
| 异常捕捉 | 自动捕捉，调用 `HandleExceptionAsync` | 需自行 try-catch |
| 指数退避重试 | 内置 | 需自行实现 |
| 死信队列对接 | 内置（重试超限自动进入 DLX） | 需自行处理 |
| 适用场景 | 绝大多数业务场景 | 需要完全自定义消息处理流程的场景 |

---

## 开源协议

本项目基于 [MIT License](LICENSE) 开源。

```
MIT License

Copyright (c) 2026 lycoris

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
