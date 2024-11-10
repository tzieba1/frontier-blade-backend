using System.Net;

namespace FbsApi.Data
{
  public class Response<T>
  {
    public T? Data { get; set; }
    public bool Success { get; set; } = true;
    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
    public ICollection<string> Messages { get; set; } = new List<string>();
  }
}