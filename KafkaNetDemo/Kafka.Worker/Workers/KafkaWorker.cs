using Confluent.Kafka;

namespace Kafka.Worker.Workers
{
    public class KafkaWorker : BackgroundService
    {
        private readonly ILogger<KafkaWorker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IConsumer<string, string> _consumer;
        private readonly IProducer<string, string> _producer;
        private readonly string _mainTopic;
        private readonly string _dlqTopic;

        public KafkaWorker(ILogger<KafkaWorker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            var bootstrap = _configuration["KafkaConfig:BootstrapServers"];

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrap,
                GroupId = _configuration["KafkaConfig:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false //Controlamos o commit manualmente
            };

            var producerConfig = new ProducerConfig { BootstrapServers = bootstrap };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            _producer = new ProducerBuilder<string, string>(producerConfig).Build();

            _mainTopic = _configuration["KafkaConfig:Topic"]!;
            _dlqTopic = _configuration["KafkaConfig:DLQTopic"]!;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_mainTopic);

            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? result = null;
                try
                {
                    result = _consumer.Consume(stoppingToken);
                    if (result == null) continue;

                    // LÓGICA DE NEGÓCIO (Simulação de erro)
                    ProcessOrder(result.Message.Value);

                    // Se chegou aqui, deu certo. Fazemos o Commit.
                    _consumer.Commit(result);
                    _logger.LogInformation("Mensagem processada e commitada: {offset}", result.Offset);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Falha crítica. Movendo para DLQ: {ex.Message}");

                    if (result != null)
                    {
                        await MoveToDeadLetterQueue(result);
                        _consumer.Commit(result); // Commitamos na principal para não ler de novo
                    }
                }
            }
        }

        private void ProcessOrder(string json)
        {
            // Simulação: Se o produto for "erro", forçamos uma exceção
            if (json.Contains("erro"))
                throw new Exception("Simulação de falha no processamento!");

            _logger.LogInformation("Processando: {json}", json);
        }

        private async Task MoveToDeadLetterQueue(ConsumeResult<string, string> result)
        {
            var dlqMessage = new Message<string, string>
            {
                Key = result.Message.Key,
                Value = result.Message.Value,
                Headers = new Headers { { "Error", System.Text.Encoding.UTF8.GetBytes("Falha no processamento") } }
            };

            await _producer.ProduceAsync(_dlqTopic, dlqMessage);
            _logger.LogWarning("Mensagem enviada para a DLQ: {topic}", _dlqTopic);
        }

        public override void Dispose()
        {
            _consumer.Close();
            _producer.Dispose();
            base.Dispose();
        }
    }
}