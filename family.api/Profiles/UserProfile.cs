using AutoMapper;
using family.api.Dtos;
using family.api.Models;

namespace family.api.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserRegisterDto, User>();
    }
}