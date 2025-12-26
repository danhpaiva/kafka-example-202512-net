namespace Kafka.Consumer.Models;

public record Order(int Id, string Product, decimal Price, DateTime CreatedAt);
