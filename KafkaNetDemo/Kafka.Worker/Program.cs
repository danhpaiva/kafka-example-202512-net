using Kafka.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<KafkaWorker>();

builder.Services.AddHostedService<KafkaRetryWorker>();

var host = builder.Build();
host.Run();
