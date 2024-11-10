using MongoDB.Bson.Serialization;

public class DateTimeOffsetSerializer : IBsonSerializer<DateTimeOffset>
{
  public Type ValueType => typeof(DateTimeOffset);

  public DateTimeOffset Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
  {
    var bsonDate = context.Reader.ReadDateTime();
    return DateTimeOffset.FromUnixTimeMilliseconds(bsonDate);
  }

  public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTimeOffset value)
  {
    context.Writer.WriteDateTime(value.ToUnixTimeMilliseconds());
  }

  object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
  {
    // Read the date as a BSON long (which is the BSON representation for a DateTime in milliseconds)
    var bsonDate = context.Reader.ReadDateTime();
    return DateTimeOffset.FromUnixTimeMilliseconds(bsonDate);
  }

  public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
  {
    if (value is DateTimeOffset dateTimeOffset)
    {
      // Write as a BSON date in milliseconds since the Unix epoch
      context.Writer.WriteDateTime(dateTimeOffset.ToUnixTimeMilliseconds());
    }
    else
    {
      throw new ArgumentException("Expected DateTimeOffset value.");
    }
  }
}