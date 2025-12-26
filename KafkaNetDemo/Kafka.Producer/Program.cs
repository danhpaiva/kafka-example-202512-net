using Confluent.Kafka;
using Kafka.Producer.Models;
using System.Net;
using System.Text.Json;

var config = new ProducerConfig
{
    BootstrapServers = "localhost:9092",
    ClientId = Dns.GetHostName(),
    // Configurações de Resiliência
    Acks = Acks.All,                // Garante que todos os brokers confirmem o recebimento
    EnableIdempotence = true,        // Evita duplicatas se houver retry de rede
    MessageTimeoutMs = 5000
};

using var producer = new ProducerBuilder<string, string>(config).Build();

Console.WriteLine("--- Producer JSON de Alta Disponibilidade ---");

while (true)
{
    Console.WriteLine("\nDigite o nome do produto (ou 'sair'):");
    var produto = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(produto) || produto.ToLower() == "sair") break;

    var order = new Order(new Random().Next(100, 999), produto, 99.90m, DateTime.Now);
    var json = JsonSerializer.Serialize(order);

    try
    {
        var result = await producer.ProduceAsync("vendas-pedidos", new Message<string, string>
        {
            Key = order.Id.ToString(),
            Value = json
        });

        Console.WriteLine($"Enviado: Partição {result.Partition} | Offset {result.Offset}");
    }
    catch (ProduceException<string, string> e)
    {
        Console.WriteLine($"Erro de Conexão/Kafka: {e.Error.Reason}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro inesperado: {ex.Message}");
    }
}