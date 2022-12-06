using AuthServer.DTO;
using AuthServer.DTO.User;
using AuthServer.Models;
using AutoMapper;

namespace AuthServer.Shared
{
    public class AppMappingProfile : Profile
	{
		public AppMappingProfile()
		{
			CreateMap<User, UserPatchDto>()
				.ReverseMap();

			CreateMap<User, UserPostDto>()
				.ReverseMap();

			CreateMap<User, UserDto>()
				.ReverseMap();

			CreateMap<Todo, TodoDto>()
				.ReverseMap();
		}
	}
}
