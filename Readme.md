

### 扩展支持延迟队列(`rabbitmq_delayed_message_exchange`插件)
**[插件仓库地址：rabbitmq_delayed_message_exchange](https://github.com/rabbitmq/rabbitmq-delayed-message-exchange)**

### 安装方式

```shell
// .net cli
dotnet add package Lycoris.RabbitMQ.Extensions
// package manager
Install-Package Lycoris.RabbitMQ.Extensions
```


### **一、注册扩展**

#### 基础连接信息注册
```csharp
var mqBuilder = builder.Services.AddRabbitMQExtensions(opt =>
{
    // ip地址
    opt.Hosts = new string[] { "yout rabbitmq service ip" };
    // 端口号 不设置默认：5672
    opt.Port = 5672;
    // 账号
    opt.UserName = "your user name";
    // 密码
    opt.Password = "your password";
    // 虚拟机 不设置默认为：/
    opt.VirtualHost = "/";
    // 是否持久化 不设置默认：true
    opt.Durable = true;
    // 是否自动删除 不设置默认：false
    opt.AutoDelete = true;
    // 禁止消费者自动启动监听
    // 禁用后，需要在你需要使用消费者服务前，主动调用 IRabbitConsumerFactory.ManualStartListenAsync() 启用已添加的消费者服务监听
    // 详见 四、手动处理消费者服务
    opt.DisableRabbitConsumerHostedListen = true;
});
```

#### 生产者注册
```csharp
// 使用生产者工厂模式注册
mqBuilder.AddRabbitProducer("DefaultProducer", opt =>
{
    // 保留发布者数 默认：5
    opt.InitializeCount = 5;
    // 交换机名称
    opt.Exchange = "exchange.your.exchangename";
    // 交换机类型
    opt.Type = RabbitExchangeType.Direct;
    // 路由队列
    opt.RouteQueues = new RouteQueue[]
    {
        new RouteQueue()
        {
            Route = "route.your.routename",
            Queue = "queue.your.queuename"
        }
    };
});

// 使用生产者服务模式注册
// 单实现模式
mqBuilder.AddRabbitProducer<RabbitProducerService>(opt =>
{
    // 保留发布者数 默认：5
    opt.InitializeCount = 5;
    // 交换机名称
    opt.Exchange = "exchange.your.exchangename";
    // 交换机类型
    opt.Type = RabbitExchangeType.Direct;
    // 路由队列
    opt.RouteQueues = new RouteQueue[]
    {
        new RouteQueue()
        {
            Route = "route.your.routename",
            Queue = "queue.your.queuename"
        },
        new RouteQueue()
        {
            Route = "route.your.routename2",
            Queue = "queue.your.queuename2"
        }
    };
});

// 接口、实现类模式
mqBuilder.AddRabbitProducer<IRabbitProducerService, RabbitProducerService>(opt =>
{
    // 保留发布者数 默认：5
    opt.InitializeCount = 5;
    // 交换机名称
    opt.Exchange = "exchange.your.exchangename";
    // 交换机类型 延迟队列
    opt.Type = RabbitExchangeType.Delayed;
    // 延迟秒数
    opt.DelayTime = 5;
    // 路由队列
    opt.RouteQueues = new RouteQueue[]
    {
        new RouteQueue()
        {
            Route = "route.your.routename",
            Queue = "queue.your.queuename"
        },
        new RouteQueue()
        {
            Route = "route.your.routename2",
            Queue = "queue.your.queuename2"
        }
    };
});
```

**使用生产这服务模式注册时，不管是接口实现类还是单实现类均需要继承 `BaseRabbitProducerService` 基类，并实现构造函数**
**服务生命周期固定为`Scoped`**

**`BaseRabbitProducerService` 基类包含两个属性：**
- **`RabbitProducerFactory`：生产者创建工厂**
- **`Producer`：当前注册的生产者实例**

**示例**
```csharp
public class RabbitProducerService : BaseRabbitProducerService, IRabbitProducerService
{
    public RabbitProducerService(IRabbitProducerFactory rabbitProducerFactory) : base(rabbitProducerFactory)
    {
    }
}
```

#### 消费者注册
```csharp
mqBuilder.AddRabbitConsumer(opt =>
{
    // 是否自动提交 默认：false
    opt.AutoAck = false;
    // 每次发送消息条数 默认：2
    opt.FetchCount = 2;
    // 交换机类型
    opt.Type = RabbitExchangeType.Direct;
    // 路由队列
    opt.RouteQueues = new RouteQueue[]
    {
        new RouteQueue()
        {
            Route = "route.your.routename",
            Queue = "queue.your.queuename"
        },
        new RouteQueue()
        {
            Route = "route.your.routename2",
            Queue = "queue.your.queuename2"
        }
    };

    // 消费者创建详见第二部分 创建消费者
    // 添加消费者监听
    // 普通模式
    opt.AddListener<TestConsumer>("queue.your.queuename");
    // 订阅模式、路由模式、Topic模式、延迟队列模式
    // exchange.your.exchangename 为对应的生产者交换机
    opt.AddListener<TestConsumer1>("exchange.your.exchangename", "queue.your.queuename");
});
```


### **二、生产者使用示例**

#### 使用生产者工厂模式发送示例
```csharp
public class Demo
{
    private readonly IRabbitProducerFactory _factory;
    private readonly IRabbitConsumerFactory _consumerFactory;
    public class Demo(IRabbitProducerFactory factory, IRabbitConsumerFactory consumerFactory)
    {
        _factory = factory;
        _consumerFactory = consumerFactory;
    }

    public void PublishTest()
    {
        // 获取生产者
        var producer = factory.Create("DefaultProducer");
        // 发送消息
        producer.Publish("route.your.routename", "this is push TestConsumer");
    }

    public async Task ManualTest()
    {
        // 如果在配置中设置了 DisableRabbitConsumerHostedListen 为 true
        // 需要在合适的地方 启动消费者监听，此时消费者才能正常的接收MQ消息
        // 此处仅作为使用示例

        // 启动所有的消费者监听
        await _consumerFactory.ManualStartListenAsync();
        // 启动指定的消费者监听
        await _consumerFactory.ManualStartListenAsync("exchange.your.exchangename", "queue.your.queuename");
        // 停止所有的消费者监听
        await _consumerFactory.ManualStopListenAsync();
        // 停止指定的消费者监听
        await _consumerFactory.ManualStopListenAsync("exchange.your.exchangename", "queue.your.queuename");
    }
}
```

#### 使用生产者服务模式发送示例
```csharp
public class RabbitProducerService : BaseRabbitProducerService, IRabbitProducerService
{
    public RabbitProducerService(IRabbitProducerFactory rabbitProducerFactory) : base(rabbitProducerFactory)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public void Test()
    {
        // 直接使用生产者进行发送
        this.Producer.Publish("route.your.routename", "this is push TestConsumer");
        this.Producer.Publish("route.your.routename2", "this is push TestConsumer2");
    }
}
```


### **三、消费者使用示例**

#### **消费者有两种创建方式**
#### **1. 继承扩展封装好的基类：`BaseRabbitConsumerListener`，做了基础的异常捕捉。**

**基类包含属性：**
- **`Context`：当前消费者接收到的上下文实体**
- **`Exchange`：当前消费者交换机名称**
- **`Route`：当前消费者路由**
- **`ResubmitTimeSpan`：重新发布时间间隔(单位:毫秒,默认1000毫秒)**
- **`Task<ReceivedHandler> HandleExceptionAsync(Exception exception)`：全局异常拦截，没有重写的情况下，默认扩展返回的是回滚MQ消息**

```csharp
public class TestConsumer : BaseRabbitConsumerListener
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    protected override Task<ReceivedHandler> ReceivedAsync(string body)
    {
        Console.WriteLine($"TestConsumer ==> {body}");
        return Task.FromResult(ReceivedHandler.Commit);
    }

    // 不重写，默认返回的是回滚MQ消息
    protected override Task<ReceivedHandler> HandleExceptionAsync(Exception exception)
    {
        // 处理你未捕获到的异常
    }
}
```



#### **2. 使用 `IRabbitConsumerListener` 接口自己实现**
```csharp
public abstract class TestConsumer : IRabbitConsumerListener
{
     /// <summary>
     /// 消费消息
     /// </summary>
     /// <param name="recieveResult"></param>
     /// <returns></returns>
     public async  Task ConsumeAsync(RecieveResult recieveResult)
     {
	 	  // 提交
          recieveResult.Commit();
		  // 回滚
		  //recieveResult.RollBack();
		  // 重新发布
		  //recieveResult.RollBack(true);
     }
}
```

### **四、手动处理消费者服务**

**禁用后，需要在你需要使用消费者服务前，主动调用 IRabbitConsumerFactory.ManualStartListenAsync() 启用已添加的消费者服务监听**

```csharp
public class ApplicationStartService : IHostedService
{
    private readonly IRabbitConsumerFactory _consumerFactory;
    public ApplicationStartService(IRabbitConsumerFactory consumerFactory)
    {
        _consumerFactory = consumerFactory;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // do something
        // 做一些基础操作，比如数据库迁移、中间件热机、其他前置处理

        // 启动消费者监听
        await _consumerFactory.ManualStartListenAsync();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        // do something 

        await _consumerFactory.ManualStopListenAsync(cancellationToken);
    }
}
```