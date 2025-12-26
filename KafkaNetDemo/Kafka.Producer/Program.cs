using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Kafka.Producer.Models;
using System.Net;

// 1. Configurações de Conexão
var bootstrapServers = "localhost:9092";
var schemaRegistryUrl = "localhost:8081";

var producerConfig = new ProducerConfig
{
    BootstrapServers = bootstrapServers,
    ClientId = Dns.GetHostName(),
    MessageTimeoutMs = 5000,
    RequestTimeoutMs = 5000
};

var schemaRegistryConfig = new SchemaRegistryConfig
{
    Url = schemaRegistryUrl
};

// 2. Construção dos Clientes
// O CachedSchemaRegistryClient armazena os esquemas localmente para não consultar o servidor toda hora
using var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

using var producer = new ProducerBuilder<string, Order>(producerConfig)
    .SetValueSerializer(new AvroSerializer<Order>(schemaRegistry)) // Serializador Avro injetado aqui
    .Build();

Console.WriteLine("--- Producer Avro com Schema Registry ---");

while (true)
{
    Console.WriteLine("\nDigite o nome do produto (ou 'sair'):");
    var produto = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(produto) || produto.ToLower() == "sair") break;

    // Criando o objeto respeitando o contrato Avro
    var order = new Order
    {
        Id = new Random().Next(100, 999),
        Product = produto,
        Price = 99.90,
        CreatedAtTicks = DateTime.UtcNow.Ticks
    };

    try
    {
        // Envio Assíncrono para o tópico Avro
        // O AvroSerializer verificará se o esquema da classe 'Order' já existe no Registry
        var deliveryReport = await producer.ProduceAsync("vendas-pedidos-avro", new Message<string, Order>
        {
            Key = order.Id.ToString(),
            Value = order
        });

        Console.WriteLine($"[Avro] Sucesso! Partição: {deliveryReport.Partition} | Offset: {deliveryReport.Offset}");
    }
    catch (ProduceException<string, Order> e)
    {
        // Erro específico do Kafka ou falha na validação do esquema (Contrato violado)
        Console.WriteLine($"Erro de Envio/Esquema: {e.Error.Reason}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro inesperado: {ex.Message}");
    }
}