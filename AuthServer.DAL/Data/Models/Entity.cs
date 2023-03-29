using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.DAL.Data.Models
{
    public class Entity : IEntity
    {
        public ulong Id { get; set; }
        public DateTime Created { get; set; }
    }
}
