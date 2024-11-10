
namespace FbsApi.Data.Models.DataTransferObjects.Auth
{
  public class AuthTokensDto
  {
    required public string Jwt { get; set; }
    required public string RefreshJwt { get; set; }
  }
}