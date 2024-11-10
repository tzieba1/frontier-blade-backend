using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FbsApi.Data.Models
{
  public class RefreshToken: BaseModel
  {
    required public string Token { get; set; }
    [BsonRepresentation(BsonType.String)]
    required public Guid JwtId { get; set; }
    required public bool IsUsed { get; set; }
    required public bool IsRevoked { get; set; }
    [BsonSerializer(typeof(DateTimeOffsetSerializer))]
    required public DateTimeOffset ExpiresAt { get; set; }
    required public User User { get; set; } 
  }
}