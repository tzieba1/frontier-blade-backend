namespace FbsApi.Settings
{
  public class JwtSettings
  {
    required public string Audience { get; set; }
    required public string Issuer { get; set; }
    required public string Key { get; set; }
    required public int ExpirySeconds { get; set; }
  }
}