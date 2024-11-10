
using FbsApi.Data.Models.DataTransferObjects.User;

namespace FbsApi.Data.Models.DataTransferObjects.Auth
{
  public class RefreshTokenDto
  {
    required public string Jwt { get; set; }
    required public string RefreshJwt { get; set; }
    required public bool IsUsed { get; set; }
    required public bool IsRevoked { get; set; }
    required public DateTimeOffset CreatedAt { get; set; }
    required public DateTimeOffset ExpiresAt { get; set; }
    required public GetUserDto User { get; set; }
  }
}