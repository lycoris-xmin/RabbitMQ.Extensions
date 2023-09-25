

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
    // 
    opt.DisableRabbitConsumerHostedListen = true;
});
```

#### 生产者注册
```csharp
mqBuilder.AddRabbitProducer(opt =>
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

### **二、创建消费者**

#### **消费者有两种创建方式**
#### **1. 继承扩展封装好的基类：`RabbitConsumerListener`，做了基础的异常捕捉。**

**基类包含属性：**
- **`Context`：当前消费者接收到的上下文实体**
- **`Exchange`：当前消费者交换机名称**
- **`Route`：当前消费者路由**
- **`ResubmitTimeSpan`：重新发布时间间隔(单位:毫秒,默认1000毫秒)**
- **`Task<ReceivedHandler> HandleExceptionAsync(Exception exception)`：全局异常拦截，没有重写的情况下，默认扩展返回的是回滚MQ消息**

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