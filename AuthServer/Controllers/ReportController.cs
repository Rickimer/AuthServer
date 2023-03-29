using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : Controller
    {
        [HttpGet("Info")]
        public async Task<IActionResult> Get()
        {
            return Ok();
        }
    }
}