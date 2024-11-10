using Microsoft.IdentityModel.Tokens;
using FbsApi.Settings;
using FbsApi.Data.Models.DataTransferObjects.User;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using FbsApi.Services.Interfaces;
using FbsApi.Data.Models;
using AutoMapper;
using FbsApi.Data.Models.DataTransferObjects.Auth;
using FbsApi.Data;
using Microsoft.AspNetCore.WebUtilities;

namespace FbsApi.Services
{
  public class AuthService : IAuthService
  {
    private readonly RefreshTokenRepository _refreshTokenRepository;
    private readonly UserRepository _userRepository;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtSettings _jwtSettings;
    private readonly EmailConfirmationUri _emailConfirmationUri;
    private readonly TokenValidationParameters _tokenValidationParams;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailSender _gmailSender;
    private readonly ILogger<AuthService> _logger;
    private readonly IMapper _mapper;

    public AuthService(UserRepository userRepository,
                      RefreshTokenRepository refreshTokenRepository,
                      PasswordHasher passwordHasher,
                      IConfiguration config,
                      TokenValidationParameters tokenValidationParams,
                      IHttpContextAccessor httpContextAccessor,
                      IEmailSender gmailSender,
                      ILogger<AuthService> logger,
                      IMapper mapper
      )
    {
      _userRepository = userRepository;
      _refreshTokenRepository = refreshTokenRepository;
      _passwordHasher = passwordHasher;
      _jwtSettings = config.GetSection("Jwt").Get<JwtSettings>() ?? throw new ArgumentNullException("JwtSettings");
      _emailConfirmationUri = config.GetSection("EmailConfirmationUri").Get<EmailConfirmationUri>() ?? throw new ArgumentNullException("EmailConfirmationUri");
      _tokenValidationParams = tokenValidationParams;
      _httpContextAccessor = httpContextAccessor;
      _gmailSender = gmailSender;
      _logger = logger;
      _mapper = mapper;
    }

    public async Task<Response<RefreshTokenDto>> Login(LoginUserDto user)
    {
      try
      {
        var findResult = await _userRepository.FindByEmailAsync(user.Email);
        if (findResult == null || findResult.Data == null)
        {
          return new Response<RefreshTokenDto>()
          {
            Messages = new List<string>() { "User not found." },
            Status = HttpStatusCode.NotFound,
            Success = false
          };
        }
        User existingUser = findResult.Data;

        // // Check user has not confirmed their email
        // if (!existingUser.EmailConfirmed)
        // {
        //   return new ServiceResponse<RefreshTokenDto>()
        //   {
        //     Messages = new List<string>() { "Email has not been confirmed." },
        //     Status = HttpStatusCode.NotFound,
        //     Success = false
        //   };
        // }

        // Verify password
        if (!_passwordHasher.VerifyPassword(existingUser.PasswordHash, user.Password))
        {
          return new Response<RefreshTokenDto>()
          {
            Messages = new List<string>() { "Invalid password" },
            Status = HttpStatusCode.BadRequest,
            Success = false
          };
        }

        return await GenerateJwt(existingUser);
      }
      catch (Exception ex)
      {
        return new Response<RefreshTokenDto>()
        {
          Messages = new List<string>() { ex.Message },
          Status = HttpStatusCode.InternalServerError,
          Success = false
        };
      }
    }

    public async Task<Response<Guid>> Logout(AuthTokensDto tokens)
    {
      Response<Guid> serviceResponse = new();

      // Attempt to find refresh token of user to logout so it can be revoked
      RefreshToken refreshToken = await _refreshTokenRepository.FindByTokenAsync(tokens.RefreshJwt);

      // Check that refresh token was not found.
      if (refreshToken == null)
      {
        serviceResponse.Messages.Add("Refresh token not found.");
        serviceResponse.Status = HttpStatusCode.NotFound;
        serviceResponse.Success = false;
        return serviceResponse;
      }

      // Revoke refresh token
      refreshToken.IsRevoked = true;
      await _refreshTokenRepository.UpdateAsync(refreshToken.Id, refreshToken);

      // Return success message
      serviceResponse.Messages.Add($"Successfully logged out user with id {refreshToken.User.Id}");
      serviceResponse.Data = refreshToken.User.Id;
      return serviceResponse;
    }

    public async Task<Response<Guid>> Register(RegisterUserDto user)
    {
      try
      {
        var emailResult = await _userRepository.FindByEmailAsync(user.Email);
        // Check email already registered
        if (emailResult != null && emailResult.Data != null)
        {
          return new Response<Guid>()
          {
            Messages = new List<string>() { "Email already in use." },
            Status = HttpStatusCode.BadRequest,
            Success = false
          };
        }

        // Await registration and check for success
        var newUser = new User
        {
          Email = user.Email,
          EmailConfirmed = false,
          FirstName = user.FirstName,
          LastName = user.LastName,
          CreatedBy = Guid.Empty,
          CreatedAt = DateTimeOffset.UtcNow,
          UpdatedBy = Guid.Empty,
          UpdatedAt = DateTimeOffset.UtcNow,
          Roles = [Role.Employee],
          PasswordHash = _passwordHasher.HashPassword(user.Password)
        };

        var result = await _userRepository.CreateAsync(newUser);
        if (!result.Success || result.Data == null)  // Registration failed
        {
          return new Response<Guid>()
          {
            Messages = result.Errors,
            Status = HttpStatusCode.InternalServerError,
            Success = false
          };
        }

        // Update user with their Id as CreatedBy and UpdatedBy
        var updatedUser = result.Data;
        updatedUser.CreatedBy = updatedUser.Id;
        updatedUser.UpdatedBy = updatedUser.Id;
        var updatedResult = await _userRepository.UpdateAsync(updatedUser.Id, updatedUser);

        _logger.LogInformation($"User \"{newUser.Email}\" created a new account.");

        var findResult = await _userRepository.FindByEmailAsync(user.Email);
        if (findResult == null || findResult.Data == null)
        {
          return new Response<Guid>()
          {
            Messages = new List<string>() { "Error retrieving registered user." },
            Status = HttpStatusCode.InternalServerError,
            Success = false
          };
        }

        _logger.LogInformation($"User \"{newUser.Email}\" created a new account.");

        // Send email confirmation
        var urlBuilder = new UriBuilder()
        {
          Scheme = _emailConfirmationUri.Scheme,
          Host = _emailConfirmationUri.Host,
          Port = _emailConfirmationUri.Port,
          Path = _emailConfirmationUri.Path,
          Query = $"?userId={updatedUser.Id}&code={GenerateEmailConfirmationToken(newUser)}"
        };
        await _gmailSender.SendEmailAsync(updatedUser.Email, "Confirm your email",
    $"Please confirm your account by <a href='{urlBuilder}'>clicking here</a>.");
        return new Response<Guid>()
        {
          Messages = new List<string>() { $"Email sent to '{updatedUser.Email}' with a coded confirmation link (check spam folder)." }
        };

      }
      catch (Exception ex)
      {
        return new Response<Guid>()
        {
          Messages = new List<string>() { ex.Message },
          Status = HttpStatusCode.InternalServerError,
          Success = false
        };
      }
    }

    public async Task<Response<RefreshTokenDto>> ConfirmEmail(string userId, string code)
    {
      try
      {
        if (userId == null || code == null)
        {
          return new Response<RefreshTokenDto>()
          {
            Messages = new List<string>() { "Invalid email confirmation request." },
            Success = false,
            Status = HttpStatusCode.BadRequest
          };
        }

        var user = (await _userRepository.GetByIdAsync(new Guid(userId))).Data;
        if (user == null)
        {
          return new Response<RefreshTokenDto>()
          {
            Messages = new List<string>() { $"Unable to load user with ID '{userId}'." },
            Success = false,
            Status = HttpStatusCode.NotFound
          };
        }

        // Validate email confirmation token
        if (!ValidateEmailConfirmationToken(code, out _, out _))
        {
          return new Response<RefreshTokenDto>()
          {
            Messages = new List<string>() { "Invalid email confirmation token." },
            Success = false,
            Status = HttpStatusCode.BadRequest
          };
        }

        // Update user's EmailConfirmed property to true
        user.EmailConfirmed = true;
        var result = await _userRepository.UpdateAsync(user.Id, user);
        var response = await GenerateJwt(user);
        response.Messages.Add(result.Success ? "Thank you for confirming your email." : "Error confirming your email.");
        return response;
      }
      catch (Exception ex)
      {
        return new Response<RefreshTokenDto>()
        {
          Messages = new List<string>() { ex.Message },
          Status = HttpStatusCode.InternalServerError,
          Success = false
        };
      }
    }

    public async Task<Response<RefreshTokenDto>> RefreshToken(AuthTokensDto tokens)
    {
      try
      {
        Response<RefreshTokenDto> result = await ValidateAndGenerateAuthenticationTokens(tokens);
        if (result == null)
        {
          return new Response<RefreshTokenDto>()
          {
            Messages = new List<string>() { "Invalid tokens." },
            Success = false,
            Status = HttpStatusCode.BadRequest
          };
        }
        return result;
      }
      catch (Exception ex)
      {
        return new Response<RefreshTokenDto>()
        {
          Messages = new List<string>() { ex.Message },
          Status = HttpStatusCode.InternalServerError,
          Success = false
        };
      }
    }

    /// <summary>
    /// Use a configuration private key to generate a JWT token with a SecurityTokenDescriptor.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<Response<RefreshTokenDto>> GenerateJwt(User user)
    {
      try
      {
        JwtSecurityTokenHandler jwtTokenHandler = new();
        byte[] key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

        // Establish claims for JWTs.
        List<System.Security.Claims.Claim> claims = new()
        {
          new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
          new System.Security.Claims.Claim(JwtRegisteredClaimNames.Email, user.Email),             // Email claim of user generating Jwt
          new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, user.FirstName),           // Sub claim identifies principal Subject of the JWT
          new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Enables JWT refresh token functionality
        };

        // Add role claims from the user’s roles
        foreach (var role in user.Roles)
        {
          claims.Add(new System.Security.Claims.Claim(ClaimTypes.Role, role.ToString()));
        }

        // SecurityTokenDescriptor works to register claims to a JwtPayload (e.g. Subject -> sub, Expires -> exp)
        SecurityTokenDescriptor tokenDescriptor = new()
        {
          Subject = new ClaimsIdentity(claims),
          Expires = DateTime.UtcNow.AddSeconds(_jwtSettings.ExpirySeconds), // use ~5-10 mins in production
          SigningCredentials = new SigningCredentials(
            _tokenValidationParams.IssuerSigningKey,
            SecurityAlgorithms.HmacSha256Signature
          ),
          Audience = _jwtSettings.Audience,
          Issuer = _jwtSettings.Issuer
        };

        // Use security descriptor to generate JWT token
        SecurityToken token = jwtTokenHandler.CreateToken(tokenDescriptor);

        // Refreshes the JWT token
        var refreshToken = new RefreshToken()
        {
          JwtId = new Guid(token.Id),
          IsUsed = false,
          IsRevoked = false,
          User = user,
          ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),  // Should be longer than JWT expiry, unless requiring a new token every time
          Token = RandomString(35) + Guid.NewGuid(),
          CreatedBy = Guid.Empty,
          UpdatedBy = Guid.Empty,
          UpdatedAt = DateTimeOffset.UtcNow,
          CreatedAt = DateTimeOffset.UtcNow
        };

        // Save refreshToken to database
        var result = await _refreshTokenRepository.CreateAsync(refreshToken);
        RefreshTokenDto tokenDto = _mapper.Map<RefreshTokenDto>(result.Data);
        tokenDto.Jwt = jwtTokenHandler.WriteToken(token);
        tokenDto.RefreshJwt = refreshToken.Token;
        tokenDto.User.Roles = user.Roles;

        return new Response<RefreshTokenDto>()
        {
          Data = tokenDto,
          Status = HttpStatusCode.Created,
          Messages = new List<string>() { "Successfully generated new tokens." }
        };
      }
      catch (Exception ex)
      {
        return new Response<RefreshTokenDto>()
        {
          Messages = new List<string>() { ex.Message },
          Status = HttpStatusCode.InternalServerError,
          Success = false
        };
      }
    }

    /// <summary>
    /// Helper for validating tokens that are being generated during token refresh.
    /// </summary>
    /// <param name="token">Token from request body being refreshed.</param>
    /// <returns>Jwt with Token and RefreshToken properties</returns>
    public async Task<Response<RefreshTokenDto>> ValidateAndGenerateAuthenticationTokens(AuthTokensDto token)
    {
      JwtSecurityTokenHandler jwtTokenHandler = new();  // Handles verification of a Jwt
      try
      {
        // 1. Validate JWT token format to pass on for encryption algorithm validation
        ClaimsPrincipal tokenCandidate = jwtTokenHandler.ValidateToken(token.Jwt, _tokenValidationParams, out SecurityToken validatedToken);

        // 2. Validate encryption algorithm
        if (validatedToken is JwtSecurityToken jwtSecurityToken)
        {
          if (!jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
          {
            return new Response<RefreshTokenDto>()
            {
              Success = false,
              Messages = new List<string>() { "Unauthorized." },  // Do not mention algorithm used.
              Status = HttpStatusCode.Unauthorized
            };
          }
        }

        // Attempt to find stored RefreshToken entry in database with Token matching token.RefreshJwt
        var storedToken = await _refreshTokenRepository.FindByTokenAsync(token.RefreshJwt);


        // 3. Validate that stored RefreshToken exists
        if (storedToken == null)
        {
          return new Response<RefreshTokenDto>()
          {
            Success = false,
            Messages = new List<string>() { "Token does not exist." },
            Status = HttpStatusCode.Gone
          };
        }

        // 4. Validate that token claim Id from ClaimsPrincipal object (tokenCandidate) returned from 1st validation
        System.Security.Claims.Claim? jtiClaim = tokenCandidate.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti);
        if (jtiClaim == null)
        {
          return new Response<RefreshTokenDto>()
          {
            Success = false,
            Messages = new List<string>() { "Token does not contain JTI claim." },
            Status = HttpStatusCode.BadRequest
          };
        }
        string jti = jtiClaim.Value;
        if (storedToken.JwtId.ToString() != jti)
        {
          return new Response<RefreshTokenDto>()
          {
            Success = false,
            Messages = new List<string>() { "Token does not match." },
            Status = HttpStatusCode.Conflict
          };
        }

        // 5. Validate that stored RefreshToken has not been revoked
        if (storedToken.IsRevoked)
        {
          return new Response<RefreshTokenDto>()
          {
            Success = false,
            Messages = new List<string>() { "Token has been revoked." },
            Status = HttpStatusCode.Forbidden
          };
        }

        // 6. Validate that stored RefreshToken has not been used
        if (storedToken.IsUsed)
        {
          return new Response<RefreshTokenDto>()
          {
            Success = false,
            Messages = new List<string>() { "Token has been used." },
            Status = HttpStatusCode.Forbidden
          };
        }

        // 7. Validate expiry date is after now (no need to refresh)
        var expClaim = tokenCandidate.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp);
        if (expClaim == null)
        {
          return new Response<RefreshTokenDto>()
          {
            Success = false,
            Messages = new List<string>() { "Token does not contain Exp claim." },
            Status = HttpStatusCode.BadRequest
          };
        }
        long utcExpiresAt = long.Parse(expClaim.Value);
        DateTimeOffset ExpiresAt = UnixTimeStampToDateTime(utcExpiresAt);
        if (ExpiresAt > DateTime.UtcNow)
        {
          return new Response<RefreshTokenDto>()
          {
            Success = false,
            Messages = new List<string>() { "Token has not expired." },
            Status = HttpStatusCode.OK,
            Data = _mapper.Map<RefreshTokenDto>(storedToken)
          };
        }

        // 8. Update stored RefreshToken to be used
        storedToken.IsUsed = true;
        await _refreshTokenRepository.UpdateAsync(storedToken.Id, storedToken);

        // Generate and return new Jwt for the UserId associated to stored RefreshToken
        var user = (await _userRepository.GetByIdAsync(storedToken.User.Id)).Data;
        if (user == null)
        {
          return new Response<RefreshTokenDto>()
          {
            Success = false,
            Messages = new List<string>() { "User associated to token not found." },
            Status = HttpStatusCode.NotFound
          };
        }
        return await GenerateJwt(user);
      }
      catch (Exception ex)
      {
        if (ex.Message.Contains("Lifetime validation failed. The token is expired."))
        {
          return new Response<RefreshTokenDto>()
          {
            Success = false,
            Messages = new List<string>() { "Token has expired please login again." },
            Status = HttpStatusCode.Gone
          };
        }
        else
        {
          return new Response<RefreshTokenDto>()
          {
            Success = false,
            Messages = new List<string>() { "Something went wrong." },
            Status = HttpStatusCode.BadRequest
          };
        }
      }
    }

    /// <summary>
    /// Access current HTTP context to get authenticated user and retrieve their Id via custom UserId claim.
    /// </summary>
    /// <returns></returns>
    public Guid GetUserId()
    {
      var claims = (_httpContextAccessor
        .HttpContext?
        .User?
        .Identities?
        .FirstOrDefault()?
        .Claims?
        .ToList()) ??
          throw new InvalidOperationException("Unable to retrieve user claims.");
      var userIdClaim = claims.FirstOrDefault(c => c.Type == "UserId") ??
        throw new InvalidOperationException("UserId claim not found.");
      return Guid.Parse(userIdClaim.Value);
    }

    public string GenerateEmailConfirmationToken(User user)
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

      // Define claims for the token
      var claims = new[]
      {
            new System.Security.Claims.Claim("UserId", user.Id.ToString()),
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Email, user.Email)
        };

      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddHours(24), // Token validity
        SigningCredentials = new SigningCredentials(
              new SymmetricSecurityKey(key),
              SecurityAlgorithms.HmacSha256Signature)
      };

      var token = tokenHandler.CreateToken(tokenDescriptor);
      return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(tokenHandler.WriteToken(token)));
    }

    public bool ValidateEmailConfirmationToken(string token, out string userId, out string email)
    {
      userId = string.Empty;
      email = string.Empty;

      try
      {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

        token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

        var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(key),
          ValidateIssuer = false,
          ValidateAudience = false,
          ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        if (validatedToken is JwtSecurityToken jwtToken)
        {
          userId = principal.FindFirst("UserId")?.Value ?? string.Empty;
          email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value ?? string.Empty;
          return true;
        }
      }
      catch
      {
        // Handle validation failure
      }

      return false;
    }

    /// <summary>
    /// Convert UnixTimeStamp to DateTime.
    /// </summary>
    /// <param name="unixTimeStamp">UnixTimeStamp to bet converted.</param>
    /// <returns>DateTime</returns>
    private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
      var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
      return dateTimeVal.AddSeconds(unixTimeStamp).ToUniversalTime();
    }

    /// <summary>
    /// Use alpha-numeric characters to generate a random string parametrized by character length.
    /// </summary>
    /// <param name="length">Length of random string returned</param>
    /// <returns>Random alpha-numeric string</returns>
    private static string RandomString(int length)
    {
      Random random = new();
      string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      return new string(Enumerable.Repeat(chars, length).Select(x => x[random.Next(x.Length)]).ToArray());
    }
  }
}