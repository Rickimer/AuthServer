using AuthServer.BLL.AppConst;
using AuthServer.BLL.DTO.Todo;
using AuthServer.BLL.DTO.User;
using AuthServer.DAL.Data.Enums;
using AuthServer.DAL.Data.Models;
using AutoMapper;
using RPC.Shared;

namespace AuthServer.BLL.Shared
{
	public class AppMappingBLLProfile : Profile
	{
		public AppMappingBLLProfile()
		{
			CreateMap<AuthSystemEnum, AuthSystemEnumDto>()
				.ReverseMap();

			CreateMap<ConsumeServiceEnum, ConsumeServiceEnumDto>()
				.ReverseMap();

			CreateMap<UserServiceProfile, UserServiceProfileDto>()
				.ReverseMap();

			CreateMap<UserProfile, UserProfileDto>()
				.ReverseMap();

			CreateMap<UserProfileDto, UserGithubRegisterDto>()
				.ReverseMap();

			CreateMap<BllCreateTodoDto, RPCCreateTodoDto>()
				.ReverseMap();

			CreateMap<BllUpdateTodoDto, RPCUpdateTodoDto>()
				.ReverseMap();

			CreateMap<BLLTodoDeleteDto, RPCDeleteTodoDto>()
				.ReverseMap();

			CreateMap<BllGetTodosDto, RPCGetTodosInputDto>()
				.ReverseMap();

			CreateMap<BllTodoDto, RPCTodoDto>()
				.ReverseMap();			
			/*CreateMap<User, UserDto>()
				.ReverseMap();			

			CreateMap<UserGithubRegisterDto, UserProfile>()
				.ForMember(e => e.Id, opt => opt.Ignore())
				.ForMember(e => e.ExternalAuthId, opt => opt.MapFrom(src => src.Id))
				;*/
		}
	}
}
