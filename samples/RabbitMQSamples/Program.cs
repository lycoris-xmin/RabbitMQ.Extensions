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
    // ip��ַ
    opt.Hosts = new string[] { "yout rabbitmq service ip" };
    // �˿ں� ������Ĭ�ϣ�5672
    opt.Port = 5672;
    // �˺�
    opt.UserName = "your user name";
    // ����
    opt.Password = "your password";
    // ����� ������Ĭ��Ϊ��/
    opt.VirtualHost = "/";
    // �Ƿ�־û� ������Ĭ�ϣ�true
    opt.Durable = true;
    // �Ƿ��Զ�ɾ�� ������Ĭ�ϣ�false
    opt.AutoDelete = true;
});

mqBuilder.AddRabbitProducer(opt =>
{
    // ������������ Ĭ�ϣ�5
    opt.InitializeCount = 5;
    // ����������
    opt.Exchange = "exchange.your.exchangename";
    // ����������
    opt.Type = RabbitExchangeType.Delayed;
    // �ӳٶ��У���Ϣ�ӳٷ�������
    opt.DelayTime = 5;
    // ·�ɶ���
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
    // �Ƿ��Զ��ύ Ĭ�ϣ�false
    opt.AutoAck = false;
    // ÿ�η�����Ϣ���� Ĭ�ϣ�2
    opt.FetchCount = 2;
    // ����������
    opt.Type = RabbitExchangeType.Delayed;
    // ·�ɶ���
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