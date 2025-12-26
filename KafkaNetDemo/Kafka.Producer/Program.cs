using Confluent.Kafka;
using Kafka.Producer.Models;
using System.Net;
using System.Text.Json;

var config = new ProducerConfig { 
    BootstrapServers = "localhost:9092", 
    ClientId = Dns.GetHostName() };

using var producer = new ProducerBuilder<string, string>(config).Build();

Console.WriteLine("--- Gerador de Pedidos (Producer) ---");

for (int i = 1; i <= 5; i++)
{
    var order = new Order(i, $"Produto {i}", i * 15.5m, DateTime.Now);
    var json = JsonSerializer.Serialize(order);

    var message = new Message<string, string>
    {
        Key = order.Id.ToString(), // Chave é importante para garantir ordem no Kafka
        Value = json
    };

    var result = await producer.ProduceAsync("vendas-pedidos", message);
    Console.WriteLine($"[Enviado] Pedido {order.Id} - Offset: {result.Offset}");

    await Task.Delay(1000); // Simula um intervalo entre vendas
}