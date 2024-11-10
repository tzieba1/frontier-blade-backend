using AutoMapper;
using FbsApi.Data.Models;
using FbsApi.Data.Models.DataTransferObjects.Auth;
using FbsApi.Data.Models.DataTransferObjects.User;

namespace FbsApi.Data
{
  public class AutoMapperProfile : Profile
  {
    public AutoMapperProfile()
    {
      // Auth
      CreateMap<RefreshToken, RefreshTokenDto>();

      // User
      CreateMap<User, GetUserDto>();
      CreateMap<RegisterUserDto, User>();
      CreateMap<LoginUserDto, User>();
    }
  }
}