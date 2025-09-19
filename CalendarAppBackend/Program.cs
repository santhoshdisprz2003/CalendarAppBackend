using CalendarAppBackend.Data;
using CalendarAppBackend.Repositories;
using CalendarAppBackend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Writers;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repositories & Services for DI
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
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

app.UseCors("AllowReactApp");
app.MapControllers();
app.Run();
