using Confluent.Kafka;
using Kafka.Producer.Models;
using System.Net;
using System.Text.Json;

var config = new ProducerConfig
{
    BootstrapServers = "localhost:9092",
    ClientId = Dns.GetHostName(),
    // Tenta reconectar automaticamente por até 5 minutos se o broker cair
    MessageTimeoutMs = 5000,
    RequestTimeoutMs = 5000
};

// <string, string> sendo Chave e Valor. Usar chaves ajuda no particionamento.
using var producer = new ProducerBuilder<string, string>(config).Build();

Console.WriteLine("--- Producer de Alta Disponibilidade ---");

while (true)
{
    Console.WriteLine("\nDigite o nome do produto (ou 'sair'):");
    var produto = Console.ReadLine();
    if (produto?.ToLower() == "sair") break;

    var order = new Order(new Random().Next(100, 999), produto!, 99.90m, DateTime.Now);
    var json = JsonSerializer.Serialize(order);

    try
    {
        // ProduceAsync garante que não travamos a thread principal
        var deliveryReport = await producer.ProduceAsync("vendas-pedidos", new Message<string, string>
        {
            Key = order.Id.ToString(),
            Value = json
        });

        Console.WriteLine($"Entregue: Partição {deliveryReport.Partition}, Offset {deliveryReport.Offset}");
    }
    catch (ProduceException<string, string> e)
    {
        // Se o Kafka estiver fora, ele cairá aqui após o timeout
        Console.WriteLine($"Erro de Envio: {e.Error.Reason}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro inesperado: {ex.Message}");
    }
}