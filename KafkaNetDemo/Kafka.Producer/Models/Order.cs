namespace Kafka.Producer.Models;

public class Order
{
    public int Id { get; set; }
    public string Product { get; set; }
    public double Price { get; set; }
    public long CreatedAt { get; set; }
}
