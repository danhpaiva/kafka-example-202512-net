using Confluent.Kafka;
using Kafka.Consumer.Models;
using System.Text.Json;

var config = new ConsumerConfig
{
    BootstrapServers = "localhost:9092",
    GroupId = "vendas-processor-group",
    AutoOffsetReset = AutoOffsetReset.Earliest
};

using var consumer = new ConsumerBuilder<string, string>(config).Build();
consumer.Subscribe("vendas-pedidos");

Console.WriteLine("--- Processador de Pedidos (Consumer) ---");

while (true)
{
    var result = consumer.Consume();
    var order = JsonSerializer.Deserialize<Order>(result.Message.Value);

    Console.WriteLine($"[Processando] ID: {order?.Id} | Produto: {order?.Product} | Total: R$ {order?.Price}");
}