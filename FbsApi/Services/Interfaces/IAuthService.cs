
using FbsApi.Data;
using FbsApi.Data.Models.DataTransferObjects.Auth;
using FbsApi.Data.Models.DataTransferObjects.User;

namespace FbsApi.Services.Interfaces
{
  public interface IAuthService
  {
    /// <summary>
    /// Represents AuthController endpoint 'DELETE: /api/Auth/Register' defined in AuthenticationService.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<Response<Guid>> Register(RegisterUserDto user);

    /// <summary>
    /// Represents AuthController endpoint 'POST: /api/Auth/Login' defined in AuthController.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<Response<RefreshTokenDto>> Login(LoginUserDto user);

    /// <summary>
    /// Represents AuthController endpoint 'POST: /api/Auth/Logout' defined in AuthController.
    /// </summary>
    /// <param name="tokens"></param>
    /// <returns></returns>
    Task<Response<Guid>> Logout(AuthTokensDto tokens);

    /// <summary>
    /// Represents AuthController endpoint 'POST: /api/Auth/ConfirmEmail?userId={string}&code={string}' defined in AuthController.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    Task<Response<RefreshTokenDto>> ConfirmEmail(string userId, string code);

    /// <summary>
    /// Represents AuthController endpoint 'POST: /api/Auth/RefreshToken' defined in AuthController.
    /// </summary>
    /// <param name="tokens"></param>
    /// <returns></returns>
    Task<Response<RefreshTokenDto>> RefreshToken(AuthTokensDto tokens);

    /// <summary>
    /// Helper used with RefreshToken method to take steps and validate a JWT and its refresh token before generating new authentication tokens.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Response<RefreshTokenDto>> ValidateAndGenerateAuthenticationTokens(AuthTokensDto token);

    /// <summary>
    /// Helper used with ConfirmEmail, Login, and RefreshToken to generate a new JWT with claims for a confirmed user and a refresh token.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<Response<RefreshTokenDto>> GenerateJwt(User user);

    /// <summary>
    /// Helper used to retrieve a bearer's 'userId' claim from the JWT provided within the Authorization header.
    /// </summary>
    /// <returns></returns>
    Guid GetUserId();
  }
}