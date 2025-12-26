using Kafka.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<KafkaWorker>();

var host = builder.Build();
host.Run();
