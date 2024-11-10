
using System.ComponentModel.DataAnnotations;

namespace FbsApi.Data.Models.DataTransferObjects.User
{
  public class LoginUserDto
  {
    [EmailAddress]
    required public string Email { get; set; }
    required public string Password { get; set; }
  }
}