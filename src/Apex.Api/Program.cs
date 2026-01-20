using Apex.Api.Extensions;
using Apex.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register all command and query handlers automatically via reflection
builder.Services.AddApplicationHandlers();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
