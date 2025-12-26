using Kafka.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<KafkaWorker>();

var host = builder.Build();
host.Run();
