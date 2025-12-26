using Confluent.Kafka;
using Kafka.Worker.Models;
using System.Text.Json;

namespace Kafka.Worker
{
    public class KafkaWorker : BackgroundService
    {
        private readonly ILogger<KafkaWorker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IConsumer<string, string> _consumer;
        private readonly string _topic;

        public KafkaWorker(ILogger<KafkaWorker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["KafkaConfig:BootstrapServers"],
                GroupId = _configuration["KafkaConfig:GroupId"],
                AutoOffsetReset = (AutoOffsetReset)Enum.Parse(typeof(AutoOffsetReset), _configuration["KafkaConfig:AutoOffsetReset"]!)
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _topic = _configuration["KafkaConfig:Topic"]!;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);
            _logger.LogInformation("Worker Kafka subscrito no tópico: {topic}", _topic);

            // O loop agora respeita o stoppingToken do .NET (quando o serviço para)
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Consume é bloqueante, mas passamos o token para cancelar se o app fechar
                    var result = _consumer.Consume(stoppingToken);

                    if (result != null)
                    {
                        var order = JsonSerializer.Deserialize<Order>(result.Message.Value);
                        _logger.LogInformation("Processando Pedido: {id} | {item}", order?.Id, order?.Product);

                        // Simula processamento pesado
                        await Task.Delay(500, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break; // Fechamento normal
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem do Kafka.");
                    await Task.Delay(2000, stoppingToken); // Backoff em caso de erro
                }
            }
        }

        public override void Dispose()
        {
            _consumer.Close();
            _consumer.Dispose();
            base.Dispose();
        }
    }
}
