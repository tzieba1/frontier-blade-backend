using System.ComponentModel.DataAnnotations;

namespace FbsApi.Data.Models.DataTransferObjects.User
{
  public class RegisterUserDto
  {
    [EmailAddress]
    required public string Email { get; set; }
    required public string FirstName { get; set; }
    required public string LastName { get; set; }
    required public string Password { get; set; }
  }
}