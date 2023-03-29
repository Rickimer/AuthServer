using AuthServer.BLL.Auth;
using AuthServer.BLL.DTO.Todo;
using AuthServer.BLL.Shared;
using AuthServer.BLL.Todo;
using AuthServer.DAL;
using AuthServer.DAL.Data.Models;
using AuthServer.DAL.Data.Repository;
using AuthServer.Shared;
using MessagesQueueService.RabbitMQ;
using MessagesQueueService.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NLog.Web;
using RPC;
using RPC.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using TodoClient;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment.EnvironmentName;
if (env == "Development")
    builder.Configuration.AddUserSecrets("AuthServer-Dev");
else
    builder.Configuration.AddUserSecrets("AuthServer-noDev");

// Add services to the container.
builder.Services.AddAutoMapper(typeof(AppMappingProfile));
builder.Services.AddAutoMapper(typeof(AppMappingBLLProfile));
builder.Services.AddAutoMapper(typeof(RPCAutoMapperProfile));

builder.Services.AddScoped(typeof(IRepository<Traffic>), typeof(TrafficRepository));
builder.Services.AddScoped(typeof(IRepository<UserServiceProfile>), typeof(UserServiceProfileRepository));
builder.Services.AddScoped(typeof(IRepository<UserProfile>), typeof(UserProfileRepository));
builder.Services.AddScoped(typeof(IRepository<RefreshToken>), typeof(RefreshTokenRepository));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IMQServices, RabbitMQService>();
builder.Services.AddScoped<IOAuth2Service, OAuth2Service>();

builder.Services.AddScoped<ITodoBllService, TodoBllService>();
builder.Services.AddScoped<ITodoRPCService, TodoRPCService>();

builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JWTSettings"));
builder.Services.Configure<Tenants>(builder.Configuration.GetSection("Tenants"));
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

var secretKey = builder.Configuration.GetSection("JWTSettings:SecretKey").Value;
if (secretKey == null)
    throw new Exception("Error load Setings");

var issuer = builder.Configuration.GetSection("JWTSettings:Issuer").Value;
var audience = builder.Configuration.GetSection("JWTSettings:Audience").Value;
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

builder.WebHost.ConfigureLogging(
        logging =>
        {            
            logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
        }
    ).UseNLog();

var connectionStringUsers = builder.Configuration.GetConnectionString("UserDB");
builder.Services.AddDbContext<UserDbContext>(options =>
{
    options.UseSqlite(connectionStringUsers);
    options.ConfigureWarnings(opt =>
    {
        opt.Ignore(CoreEventId.ForeignKeyAttributesOnBothPropertiesWarning);
    });
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<UserDbContext>();

var authServerToken = builder.Configuration.GetSection("Tenants:AuthServerToken").Value;

builder.Services.AddGrpcClient<TodoRPC.TodoRPCClient>(o => //+creds, settings, fault handling
{
    o.Address = new Uri("https://localhost:7141");
})
.AddCallCredentials((context, metadata) =>
{
    metadata.Add("TodoClient", $"AuthServer,{authServerToken}");
    return Task.CompletedTask;
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
    builder =>
    {
        builder.AllowCredentials();
        builder.AllowAnyHeader();
        builder.AllowAnyMethod();
        builder.WithOrigins("http://localhost:3000", "https://rickimer.site:8080", "https://localhost:3000", "https://localhost:5001");
    })
    );

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
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
});

builder.Services.AddMemoryCache();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/api/Auth/ReLogin";
});
builder.Services.AddControllers();

var app = builder.Build();
app.UseCors("CorsPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
