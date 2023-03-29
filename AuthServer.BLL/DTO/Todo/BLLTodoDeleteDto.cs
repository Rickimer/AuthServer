using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.BLL.DTO.Todo
{
    public class BLLTodoDeleteDto
    {
        public ulong Id { get; set; }
        public ulong UserId { get; set; }
    }
}
