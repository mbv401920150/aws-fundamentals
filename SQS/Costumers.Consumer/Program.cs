using Amazon.SQS;
using Costumers.Consumer;
using QueueSettings = Costumers.Consumer.QueueSettings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<QueueSettings>(builder.Configuration.GetSection(QueueSettings.Key));
builder.Services.AddSingleton<IAmazonSQS, AmazonSQSClient>();
builder.Services.AddHostedService<QueueConsumerService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var app = builder.Build();

app.Run();