var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// 🔥 IMPORTANT ORDER

app.UseCors("AllowAngular");  // MUST be before MapControllers

app.UseAuthorization();

app.MapControllers();

app.Run();
