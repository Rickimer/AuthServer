using AuthServer.BLL.DTO.Todo;
using AuthServer.BLL.DTO.User;
using AuthServer.Shared.Todo;
using AutoMapper;

namespace AuthServer.Shared
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
            CreateMap<UserAuthDto, UserPatchDto>()
                .ReverseMap();

            CreateMap<UserAuthDto, UserPostDto>()
                .ReverseMap();

            CreateMap<UserGridBllDto, UserGridDto>()
                .ReverseMap();

            CreateMap<BllCreateTodoDto, CreateTodoDto>()
                .ReverseMap();

            CreateMap<BllUpdateTodoDto, UpdateTodoDto>()
                .ReverseMap();

            CreateMap<BLLTodoDeleteDto, DeleteTodoDto>()
                .ReverseMap();

            CreateMap<BllGetTodosDto, GetTodosDto>()
                .ReverseMap();

            CreateMap<BllTodoDto, TodoDto>()
                .ReverseMap();
        }
    }
}
