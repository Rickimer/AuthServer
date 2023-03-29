using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPC.Shared
{
    public class RPCAutoMapperProfile : Profile
    {
        public RPCAutoMapperProfile()
        {
            CreateMap<TodoClient.RPCCreateTodo, RPCCreateTodoDto>()
                .ReverseMap();

            CreateMap< TodoClient.RPCUpdateTodo, RPCUpdateTodoDto>()                
                .ReverseMap();

            CreateMap<TodoClient.RPCDeleteTodo, RPCDeleteTodoDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TodoId))
                .ReverseMap();

            CreateMap<TodoClient.RPCUser, RPCGetTodosInputDto>()
                .ReverseMap();

            CreateMap< TodoClient.RPCTodo, RPCTodoDto>()
                .ReverseMap();

            CreateMap<TodoClient.RPCTodoId, RpcCreateResultDto>()                
                .ReverseMap();            
        }
    }
}
