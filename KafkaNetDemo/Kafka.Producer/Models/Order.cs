namespace Kafka.Producer.Models;

public class Order
{
    public int Id { get; set; }
    public string Product { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }

    // Construtor vazio é essencial para o Deserializer do JSON
    public Order() { }

    // Construtor de conveniência para o Producer
    public Order(int id, string product, decimal price, DateTime createdAt)
    {
        Id = id;
        Product = product;
        Price = price;
        CreatedAt = createdAt;
    }
}
