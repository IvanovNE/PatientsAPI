using Microsoft.EntityFrameworkCore;
using PatientsAPI;
using PatientsAPI.Application;
using PatientsAPI.Infrastructure;
using System.Text.Json;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => options.Filters.Add<ApiExceptionFilterAttribute>())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddSwaggerDocumentation();
builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(80));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    Console.WriteLine("Applying migrations...");
    await dbContext.Database.MigrateAsync();
    Console.WriteLine("Migrations applied successfully!");
}

app.UseSwaggerDocumentation();
app.UseCors("AllowAll");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
