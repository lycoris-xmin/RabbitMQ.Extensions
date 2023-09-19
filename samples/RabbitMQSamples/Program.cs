using Lycoris.RabbitMQ.Extensions;
using Lycoris.RabbitMQ.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using RabbitMQSample;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRabbitMQExtensions(builder =>
{
    // ע��������ã������ʹ�õ����������ע��
    builder.AddRabbitMQOption("DefaultRabbitMQOption", opt =>
    {
        // ip��ַ
        opt.Hosts = new string[] { "yout rabbitmq service ip" };
        // �˿ں� ������Ĭ�ϣ�5672
        opt.Port = 5667;
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

    // ע��������(�������ʹ��������)
    builder.AddRabbitProducer("DefaultProducer", opt =>
    {
        // �̳л������ã�������̳У���Ҫ�����ٴ���
        opt.UseRabbitOption("DefaultRabbitMQOption");
        // ������������ Ĭ�ϣ�5
        opt.InitializeCount = 5;
        // ����������
        opt.Exchange = "exchange.your.exchangename";
        // ����������
        opt.Type = RabbitExchangeType.Direct;
        // ·�ɶ���
        opt.RouteQueues = new RouteQueue[]
        {
            new RouteQueue()
            {
                Route = "route.your.routename",
                Queue = "queue.your.queuename"
            }
        };
    });

    // ע��������(�������ʹ��������)
    builder.AddRabbitProducer("DefaultDelayProducer", opt =>
    {
        opt.UseRabbitOption("DefaultRabbitMQOption");
        // ������������ Ĭ�ϣ�5
        opt.InitializeCount = 5;
        // ����������
        opt.Exchange = "exchange.your.delayexchangename";
        // ����������
        opt.Type = RabbitExchangeType.Delayed;
        // �ӳ�ʱ��
        opt.DelayTime = 5;
        // ·�ɶ���
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
        // �̳л������ã�������̳У���Ҫ�����ٴ���
        opt.UseRabbitOption("DefaultRabbitMQOption");
        // �Ƿ��Զ��ύ Ĭ�ϣ�false
        opt.AutoAck = false;
        // ÿ�η�����Ϣ���� Ĭ�ϣ�2
        opt.FetchCount = 2;
        // ����������
        opt.Type = RabbitExchangeType.Direct;
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
    })
    .AddListener<TestConsumer>("exchange.your.exchangename", "queue.your.queuename");

    // TestConsumer3
    builder.AddRabbitConsumer(opt =>
    {
        // �̳л������ã�������̳У���Ҫ�����ٴ���
        opt.UseRabbitOption("DefaultRabbitMQOption");
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
                Route = "route.your.routename3",
                Queue = "queue.your.queuename3"
            }
        };
    })
    .AddListener<TestConsumer3>("exchange.your.delayexchangename", "queue.your.queuename3");

    builder.UseLycorisLogger();
});

// ע��������
builder.Services.AddRabbitProducer("DefaultProducer1", opt =>
{
    // �̳л������ã�������̳У���Ҫ�����ٴ���
    opt.UseRabbitOption("DefaultRabbitMQOption");
    // ������������ Ĭ�ϣ�5
    opt.InitializeCount = 5;
    // ����������
    opt.Exchange = "exchange.your.exchangename";
    // ����������
    opt.Type = RabbitExchangeType.Direct;
    // ·�ɶ���
    opt.RouteQueues = new RouteQueue[]
    {
            new RouteQueue()
            {
                Route = "route.your.routename2",
                Queue = "queue.your.queuename2"
            }
    };
});

// ע��������
builder.Services.AddRabbitConsumer(opt =>
{
    // �̳л������ã�������̳У���Ҫ�����ٴ���
    opt.UseRabbitOption("DefaultRabbitMQOption");
    // �Ƿ��Զ��ύ Ĭ�ϣ�false
    opt.AutoAck = false;
    // ÿ�η�����Ϣ���� Ĭ�ϣ�2
    opt.FetchCount = 2;
    // ����������
    opt.Type = RabbitExchangeType.Direct;
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