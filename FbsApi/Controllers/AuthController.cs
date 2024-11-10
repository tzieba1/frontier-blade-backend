using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using FbsApi.Services.Interfaces;
using FbsApi.Data;
using FbsApi.Data.Models.DataTransferObjects.User;
using FbsApi.Data.Models.DataTransferObjects.Auth;

namespace FbsApi.Controllers
{ 
  [Authorize]
  [Route("api/[controller]")] // api/authentication
  [ApiController]
  public class AuthController : ControllerBase
  {
    private readonly IAuthService _authService;

    public AuthController(
      IAuthService authService)
    {
      _authService = authService; // Authentication Service does most of the heavy lifting.
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("Register")]
    public async Task<ActionResult<Response<Guid>>> Register(RegisterUserDto user)
    {
      Response<Guid> registerResponse = await _authService.Register(user);
      return registerResponse.Status switch
      {
        HttpStatusCode.BadRequest => BadRequest(registerResponse),
        HttpStatusCode.OK => Ok(registerResponse),
        _ => StatusCode((int)HttpStatusCode.InternalServerError,(registerResponse))
      };
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("ConfirmEmail")]
    public async Task<Response<RefreshTokenDto>> ConfirmEmail([FromQuery]string userId, [FromQuery] string code)
    {
      return await _authService.ConfirmEmail(userId, code);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("Login")]
    public async Task<ActionResult<Response<RefreshTokenDto>>> Login(LoginUserDto user)
    {
      Response<RefreshTokenDto> loginResponse = await _authService.Login(user);
      return loginResponse.Status switch
      {
        HttpStatusCode.OK => Ok(loginResponse),
        HttpStatusCode.NotFound => NotFound(loginResponse),
        HttpStatusCode.BadRequest => BadRequest(loginResponse),
        HttpStatusCode.Created => StatusCode((int)HttpStatusCode.Created, loginResponse),
        _ => StatusCode((int)HttpStatusCode.InternalServerError, (loginResponse))
      };
    }

    [HttpPost]
    [Route("Logout")]
    public async Task<ActionResult<Response<Guid>>> Logout(AuthTokensDto tokens)
    {
      Response<Guid> logoutResponse = await _authService.Logout(tokens);
      return logoutResponse.Status switch
      {
        HttpStatusCode.OK => Ok(logoutResponse),
        HttpStatusCode.NotFound => NotFound(logoutResponse),
        _ => StatusCode((int)HttpStatusCode.InternalServerError, (logoutResponse))
      };
    }

    [HttpPost]
    [Route("RefreshToken")]
    public async Task<ActionResult<Response<RefreshTokenDto>>> RefreshToken(AuthTokensDto tokens)
    {
      var refreshTokenResponse = await _authService.RefreshToken(tokens);
      return refreshTokenResponse.Status switch
      {
        HttpStatusCode.OK => Ok(refreshTokenResponse),
        HttpStatusCode.NotFound => NotFound(refreshTokenResponse),
        HttpStatusCode.BadRequest => BadRequest(refreshTokenResponse),
        HttpStatusCode.Created => StatusCode((int)HttpStatusCode.Created, refreshTokenResponse),
        HttpStatusCode.Forbidden => StatusCode((int)HttpStatusCode.Forbidden, refreshTokenResponse),
        HttpStatusCode.Gone => StatusCode((int)HttpStatusCode.Gone, refreshTokenResponse),
        HttpStatusCode.Conflict => StatusCode((int)HttpStatusCode.Conflict, refreshTokenResponse),
        _ => StatusCode((int)HttpStatusCode.InternalServerError, (refreshTokenResponse))
      };
    }
  }
}