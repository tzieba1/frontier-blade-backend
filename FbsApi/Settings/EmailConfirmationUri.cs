namespace FbsApi.Settings
{
  public class EmailConfirmationUri
  {
    required public string Scheme { get; set; }
    required public string Host { get; set; }
    required public int Port { get; set; }
    required public string Path { get; set; }
  }
}