using CalendarAppBackend.Data;
using CalendarAppBackend.Repositories;
using CalendarAppBackend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Writers;

using CalendarAppBackend.Helpers;           // For JwtSettings & JwtTokenGenerator
using Microsoft.IdentityModel.Tokens;       // For TokenValidationParameters & SymmetricSecurityKey
using System.Text;                          // For Encoding


var builder = WebApplication.CreateBuilder(args);

// Register DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT configuration is missing");

// Register as singleton
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton(new JwtTokenGenerator(jwtSettings));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });

builder.Services.AddAuthorization();



// Register Repositories & Services for DI
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// Add Controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CalendarApp API",
        Version = "v1"
    });

   c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field. Example: Bearer {your token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

//  Generate YAML file automatically on startup
using (var scope = app.Services.CreateScope())
{
    var provider = scope.ServiceProvider.GetRequiredService<ISwaggerProvider>();
    var swaggerDoc = provider.GetSwagger("v1");

    var yamlPath = Path.Combine(app.Environment.ContentRootPath, "swagger", "swagger.yaml");
    Directory.CreateDirectory(Path.GetDirectoryName(yamlPath)!);

    using var streamWriter = new StreamWriter(yamlPath);
    var yamlWriter = new OpenApiYamlWriter(streamWriter);
    swaggerDoc.SerializeAsV3(yamlWriter);
    streamWriter.Flush(); // ensure file is written
}

// ✅ Always enable Swagger (dev + prod)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CalendarApp API v1");
    c.RoutePrefix = "swagger"; // open at /swagger
});


app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowReactApp");
app.MapControllers();
app.Run();
