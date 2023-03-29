using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagesQueueService.Shared
{
    public class SendActivationDto
    {
        public string ToEmail { get; set; } = string.Empty;
        public string ConfirmUrl { get; set; } = string.Empty;
    }
}
