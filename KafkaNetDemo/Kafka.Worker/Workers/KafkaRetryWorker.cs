using Confluent.Kafka;

namespace Kafka.Worker.Workers;

public class KafkaRetryWorker : BackgroundService
{
    private readonly ILogger<KafkaRetryWorker> _logger;
    private readonly IConsumer<string, string> _dlqConsumer;
    private readonly IProducer<string, string> _mainProducer;
    private readonly string _dlqTopic = "vendas-pedidos-erros";
    private readonly string _mainTopic = "vendas-pedidos";

    public KafkaRetryWorker(ILogger<KafkaRetryWorker> logger, IConfiguration configuration)
    {
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["KafkaConfig:BootstrapServers"],
            GroupId = "dlq-monitor-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _dlqConsumer = new ConsumerBuilder<string, string>(config).Build();
        _mainProducer = new ProducerBuilder<string, string>(new ProducerConfig { BootstrapServers = config.BootstrapServers }).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _dlqConsumer.Subscribe(_dlqTopic);
        _logger.LogInformation("Monitor de DLQ iniciado. Aguardando mensagens de erro...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _dlqConsumer.Consume(stoppingToken);
                if (result == null) continue;

                _logger.LogWarning("Mensagem de erro detectada. Aguardando 30s para reprocessar...");

                // Não bloqueie o processamento se tiver muitas mensagens.
                // Aqui simplificamos com um Delay, mas em prod usaríamos timestamps.
                await Task.Delay(30000, stoppingToken);

                _logger.LogInformation("Tentando reprocessar pedido: {key}", result.Message.Key);

                // Devolvemos a mensagem para o tópico principal para o Worker original tentar de novo
                await _mainProducer.ProduceAsync(_mainTopic, new Message<string, string>
                {
                    Key = result.Message.Key,
                    Value = result.Message.Value,
                    Headers = new Headers { { "retry-count", BitConverter.GetBytes(1) } }
                });

                _dlqConsumer.Commit(result);
                _logger.LogInformation("Mensagem devolvida ao tópico principal.");
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao tentar reprocessar DLQ.");
            }
        }
    }
}
