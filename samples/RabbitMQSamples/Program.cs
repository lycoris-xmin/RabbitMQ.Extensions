using Lycoris.RabbitMQ.Extensions;
using Lycoris.RabbitMQ.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using RabbitMQSample;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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
});

mqBuilder.AddRabbitProducer(opt =>
{
    // 保留发布者数 默认：5
    opt.InitializeCount = 5;
    // 交换机名称
    opt.Exchange = "exchange.your.exchangename";
    // 交换机类型
    opt.Type = RabbitExchangeType.Delayed;
    // 延迟队列，消息延迟发布秒数
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

    opt.AddRabbitProducer<IRabbitProducerService, RabbitProducerService>();

}).AddRabbitConsumer(opt =>
{
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
            Route = "route.your.routename",
            Queue = "queue.your.queuename"
        },
        new RouteQueue()
        {
            Route = "route.your.routename2",
            Queue = "queue.your.queuename2"
        }
    };

    opt.AddConsumer<TestConsumer>("exchange.your.exchangename", "queue.your.queuename");
    opt.AddConsumer<TestConsumer2>("exchange.your.exchangename", "queue.your.queuename2");
});

var app = builder.Build();

// Configure the HTTP request pipeline.

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", ([FromServices] IRabbitProducerService producer) =>
{
    producer.Test();

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return forecast;
});

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}