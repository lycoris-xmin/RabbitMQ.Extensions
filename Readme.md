## **RabbitMQ一些常用的功能简单扩展,方便易用**

### 集成了延迟队列(`rabbitmq_delayed_message_exchange`插件)，目前延迟插件仅支持`路由模式(Direct)`

### **一、注册扩展**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 添加MQ扩展
builder.Services.AddRabbitMQExtensions(builder =>
{
    // 注册基础配置，如果有使用到多个，请多次注册
	// DefaultRabbitMQOption 为配置key，后面生产者和消费者的使用会用到
    builder.AddRabbitMQOption("DefaultRabbitMQOption", opt =>
    {
        // ip地址
        opt.Hosts = new string[] { "your mq server host" };
        // 端口号 不设置默认：5672
        opt.Port = 5672;
        // 账号
        opt.UserName = "your username";
        // 密码
        opt.Password = "your password";
        // 虚拟机 不设置默认为：/
        opt.VirtualHost = "/";
        // 是否持久化 不设置默认：true
        opt.Durable = true;
        // 是否自动删除 不设置默认：false
        opt.AutoDelete = true;
    });

    // 注册生产者(如果你有使用生产者)
	// DefaultProducer 为生产者配置Key，获取生产者客户端时使用
    builder.AddRabbitProducer("DefaultProducer", opt =>
    {
        // 继承基础配置，如果不继承，需要单独配置其他配置项
        opt.UseRabbitOption("DefaultRabbitMQOption");
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

    // 注册生产者(延迟队列生产者)
	// DefaultProducer 为生产者配置Key，获取生产者客户端时使用
	// 注意：当前延迟队列默认仅支持 rabbitmq_delayed_message_exchange 插件，使用死信扩展的延迟队列，请自行开发
    builder.AddRabbitProducer("DefaultDelayProducer", opt =>
    {
        opt.UseRabbitOption("DefaultRabbitMQOption");
        // 保留发布者数 默认：5
        opt.InitializeCount = 5;
        // 交换机名称
        opt.Exchange = "exchange.your.delayexchangename";
        // 交换机类型
        opt.Type = RabbitExchangeType.Delayed;
        // 延迟时间
        opt.DelayTime = 5;
        // 路由队列
        opt.RouteQueues = new RouteQueue[]
        {
            new RouteQueue()
            {
                Route = "route.your.routename3",
                Queue = "queue.your.queuename3"
            }
        };
    });

    // 注册生产者(消费者)
    builder.AddRabbitConsumer(opt =>
    {
        // 继承基础配置，如果不继承，需要单独再处理
        opt.UseRabbitOption("DefaultRabbitMQOption");
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
    })
    .AddListener<TestConsumer>("exchange.your.exchangename", "queue.your.queuename")
    .AddListener<TestConsumer2>("exchange.your.exchangename", "queue.your.queuename2");

    // TestConsumer3
	// 注册生产者(延迟队列生产者)
    builder.AddRabbitConsumer(opt =>
    {
        // 继承基础配置，如果不继承，需要单独再处理
        opt.UseRabbitOption("DefaultRabbitMQOption");
        // 是否自动提交 默认：false
        opt.AutoAck = false;
        // 每次发送消息条数 默认：2
        opt.FetchCount = 2;
        // 交换机类型
        opt.Type = RabbitExchangeType.Delayed;
        // 路由队列
        opt.RouteQueues = new RouteQueue[]
        {
            new RouteQueue()
            {
                Route = "route.your.routename3",
                Queue = "queue.your.queuename3"
            }
        };
    })
    .AddListener<TestConsumer3>("exchange.your.delayexchangename", "queue.your.queuename3");

    // 默认情况，扩展在程序启动时会自动启动设置了监听的消费者
    // 如果不想自动启动，或者在启动前还有其他一些操作，则可以在当前位置禁用自动监听
    // 禁用自动启动监听后，需要使用 IRabbitConsumerFactory.ManualStartListenAsync() 或者  IRabbitConsumerFactory.ManualStartListenAsync(string exchange, string queue)进行启动监听服务
    builder.DisableRabbitConsumerHostedListen = false;
});


var app = builder.Build();

// Configure the HTTP request pipeline.

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// 使用示例
app.MapGet("/weatherforecast", ([FromServices] IRabbitProducerFactory factory) =>
{
	// 根据注册的生产者配置Key获取生产者客户端
    var producer = factory.Create("DefaultProducer");
	// 推送消息
    producer.Publish("route.your.routename", "this is push TestConsumer");
	// 推送消息
    producer.Publish("route.your.routename2", "this is push TestConsumer2");

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

	// 根据注册的生产者配置Key获取生产者客户端
    var delay = factory.Create("DefaultDelayProducer");
	// 推送消息
    delay.Publish("route.your.routename3", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

    return forecast;
});

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
```

### **二、创建消费者**
**消费者有两种创建方式**
- **1. 继承扩展封装好的基类：`RabbitConsumerListener`，做了基础的异常捕捉。**

**基类包含属性：**
- **`Context`：当前消费者接收到的上下文实体**
- **`Exchange`：当前消费者交换机名称**
- **`Route`：当前消费者路由**
- **`ResubmitTimeSpan`：重新发布时间间隔(单位:毫秒,默认1000毫秒)**

**基类包含重写方法：**
- **`Task<ReceivedHandler> HandleExceptionAsync(Exception exception)`：全局异常拦截，没有重写的情况下，默认扩展返回的是回滚MQ消息**
- 
```csharp
public class TestConsumer : RabbitConsumerListener
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
}
```

- **2. 使用接口自己实现**
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