using System.ComponentModel.DataAnnotations;

namespace FbsApi.Data.Models.DataTransferObjects.User
{
  public class GetUserDto
  {
    required public Guid Id { get; set; }
    [EmailAddress]
    required public string Email { get; set; }
    required public string FirstName { get; set; }
    required public string LastName { get; set; }
    required public IList<Role> Roles { get; set; }
  }
}