using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MadameCoco.Audit.Worker.Entities;

public class OrderLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    public string OrderId { get; set; } = default!;

    public string CustomerId { get; set; } = default!;

    public string EventType { get; set; } = default!;

    public string ProductId { get; set; } = null!;
    public string ProductName { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreatedAt { get; set; }
}