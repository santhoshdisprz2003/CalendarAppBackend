using CalendarAppBackend.Data;
using CalendarAppBackend.Repositories;
using CalendarAppBackend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//  Register DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//  Register Repositories & Services for DI
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

//  Add Controllers
builder.Services.AddControllers();

//  Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//  Configure CORS for React app
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

//  Enable Swagger for all environments
app.UseSwagger();
app.UseSwaggerUI(c =>   
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CalendarApp API v1");
    c.RoutePrefix = "swagger"; // Swagger UI at /swagger
});

//  Enable CORS
app.UseCors("AllowReactApp");

//  Map controllers (API endpoints)
app.MapControllers();

//  Run the app
app.Run();
