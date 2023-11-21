using Heydesk.Entities.Models;
using Heydesk.Service.Hubs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(
          options => {
              options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
          });
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.WithOrigins(new string[] {
        "http://localhost:3000",
        "https://localhost:7271"
        })
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<HeydeskContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Heydesk"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.MapHub<ChatHub>("/ChatHub");

app.UseAuthorization();

app.MapControllers();

app.Run();
