namespace FbsApi.Services.Interfaces
{
  public interface IPasswordHasher
  {
      string HashPassword(string password, int? iterations = null);
      bool VerifyPassword(string hashedPassword, string providedPassword);
  }
}