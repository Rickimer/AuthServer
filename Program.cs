using AuthServer;
using AuthServer.Models;
using AuthServer.Models.Repository;
using AuthServer.Services;
using AuthServer.Services.Auth;
using AuthServer.Services.Logger;
using AuthServer.Services.Middleware;
using AuthServer.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment.EnvironmentName;
if (env == "Development")    
    builder.Configuration.AddUserSecrets("AuthServer-Dev");
else    
    builder.Configuration.AddUserSecrets("AuthServer-noDev");

var connectionStringUsers = builder.Configuration.GetConnectionString("UserDB");
builder.Services.AddDbContext<UserDbContext>(options => options.UseSqlite(connectionStringUsers));
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<UserDbContext>();

builder.Services.AddScoped<TodoRepository>();
builder.Services.AddScoped(typeof(IRepository<Traffic>), typeof(TrafficRepository));

builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JWTSettings"));
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear(); 

var secretKey = builder.Configuration.GetSection("JWTSettings:SecretKey").Value;
var issuer = builder.Configuration.GetSection("JWTSettings:Issuer").Value;
var audience = builder.Configuration.GetSection("JWTSettings:Audience").Value;
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
    builder => {        
        builder.AllowCredentials();
        builder.AllowAnyHeader();
        builder.AllowAnyMethod();
        builder.WithOrigins("http://localhost:3000", "https://rickimer.site:8080");  
    })
    );

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer( options =>
    {        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            IssuerSigningKey = signingKey,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = "roles",
            NameClaimType = "username",
        };
    }
 );

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/api/Auth/ReLogin";
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Logging.AddFile(Path.Combine(Directory.GetCurrentDirectory(), "logger.txt"));

builder.Services.AddAutoMapper(typeof(AppMappingProfile));

builder.WebHost    
    .ConfigureKestrel((context, serverOptions) =>
    {
        serverOptions.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http1AndHttp2);
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("CorsPolicy");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
var options = optionsBuilder
            .UseSqlite(connectionStringUsers)
            .Options;

app.UseMyMiddleware();

app.Logger.LogInformation($"Start AuthServer! {DateTime.Now}");
app.Run();