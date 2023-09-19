using Lycoris.RabbitMQ.Extensions;
using Lycoris.RabbitMQ.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using RabbitMQSample;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRabbitMQExtensions(builder =>
{
    // 注册基础配置，如果有使用到多个，请多次注册
    builder.AddRabbitMQOption("DefaultRabbitMQOption", opt =>
    {
        // ip地址
        opt.Hosts = new string[] { "yout rabbitmq service ip" };
        // 端口号 不设置默认：5672
        opt.Port = 5667;
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
    });

    // 注册生产者(如果你有使用生产者)
    builder.AddRabbitProducer("DefaultProducer", opt =>
    {
        // 继承基础配置，如果不继承，需要单独再处理
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
            }
        };
    });

    // 注册生产者(如果你有使用生产者)
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

    //
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
    .AddListener<TestConsumer>("exchange.your.exchangename", "queue.your.queuename");

    // TestConsumer3
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

    builder.UseLycorisLogger();
});

// 注册生产者
builder.Services.AddRabbitProducer("DefaultProducer1", opt =>
{
    // 继承基础配置，如果不继承，需要单独再处理
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
                Route = "route.your.routename2",
                Queue = "queue.your.queuename2"
            }
    };
});

// 注册消费者
builder.Services.AddRabbitConsumer(opt =>
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
.AddListener<TestConsumer2>("exchange.your.exchangename", "queue.your.queuename2");

var app = builder.Build();

// Configure the HTTP request pipeline.

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", ([FromServices] IRabbitProducerFactory factory) =>
{
    var producer = factory.Create("DefaultProducer");
    producer.Publish("route.your.routename", "this is push TestConsumer");
    producer.Publish("route.your.routename2", "this is push TestConsumer2");

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    var delay = factory.Create("DefaultDelayProducer");

    delay.Publish("route.your.routename3", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

    return forecast;
});

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}