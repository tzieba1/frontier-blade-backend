using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public abstract class BaseModel
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonSerializer(typeof(DateTimeOffsetSerializer))]
    public DateTimeOffset CreatedAt { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required Guid CreatedBy { get; set; }

    [BsonSerializer(typeof(DateTimeOffsetSerializer))]
    public DateTimeOffset UpdatedAt { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required Guid UpdatedBy { get; set; }

    // Constructor to set default values for CreatedAt and UpdatedAt
    protected BaseModel()
    {
        Id = Guid.Empty;
        DateTimeOffset now = DateTimeOffset.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
    }
}