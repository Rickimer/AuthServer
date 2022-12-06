using AuthServer.Models;
using AuthServer.Models.Repository;

namespace AuthServer.Services.Middleware
{
    public class TraffiсStatsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;        

        public TraffiсStatsMiddleware(RequestDelegate next, ILoggerFactory logFactory            
            )
        {
            _next = next;
            _logger = logFactory.CreateLogger("TraffiсStatsMiddleware");            
        }

        public async Task Invoke(HttpContext httpContext, IRepository<Traffic> trafficRepository)
        {
            _logger.LogInformation("TraffiсStatsMiddleware executing..");
            var path = @"{httpContext.Request.Path} - {httpContext.Request.Method}";
            var ip = httpContext.Request.HttpContext.Connection.RemoteIpAddress?.ToString();

            var traffic = new Traffic {                 
                Path = path,
                DateEvent = DateTime.Now,
                IP = ip
            };

            await trafficRepository.AddAsync(traffic);
            await _next(httpContext); // calling next middleware
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MyMiddlewareExtensions
    {
        public static IApplicationBuilder UseMyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TraffiсStatsMiddleware>();
        }
    }
}
