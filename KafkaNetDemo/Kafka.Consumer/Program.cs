using Confluent.Kafka;
using Kafka.Consumer.Models;
using System.Text.Json;

var config = new ConsumerConfig
{
    BootstrapServers = "localhost:9092",
    GroupId = "vendas-processor-group",
    AutoOffsetReset = AutoOffsetReset.Earliest,
    // Evita que o consumer trave tentando ler mensagens que não existem ainda
    EnableAutoCommit = true
};

using var consumer = new ConsumerBuilder<string, string>(config).Build();
consumer.Subscribe("vendas-pedidos");

// Token para fechamento limpo (Graceful Shutdown)
CancellationTokenSource cts = new();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

Console.WriteLine("--- Consumer Resiliente Aguardando ---");

try
{
    while (!cts.IsCancellationRequested)
    {
        try
        {
            // Timeout de 1 segundo para o Consume não travar o loop para sempre
            var result = consumer.Consume(cts.Token);

            if (result == null) continue;

            // Deserialização segura
            var order = JsonSerializer.Deserialize<Order>(result.Message.Value);

            if (order == null) throw new Exception("Dados nulos após deserialização.");

            Console.WriteLine($"Processado: Pedido #{order.Id} - Item: {order.Product}");
        }
        catch (ConsumeException e)
        {
            // Erros de infraestrutura (Kafka caiu, rede, etc)
            Console.WriteLine($"Erro de Consumo: {e.Error.Reason}");
            await Task.Delay(2000); // Espera um pouco antes de tentar de novo
        }
        catch (JsonException e)
        {
            // Erro de Poison Pill (Mensagem inválida no tópico)
            Console.WriteLine($"Mensagem inválida ignorada: {e.Message}");
        }
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Desligando consumidor...");
}
finally
{
    consumer.Close(); // OBRIGATÓRIO: Avisa o Kafka que este consumidor saiu do grupo
}