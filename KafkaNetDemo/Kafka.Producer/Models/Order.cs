namespace Kafka.Producer.Models;

public class Order
{
    public int Id { get; set; }
    public string Product { get; set; } = string.Empty;
    public double Price { get; set; }
    public long CreatedAtTicks { get; set; }
}
