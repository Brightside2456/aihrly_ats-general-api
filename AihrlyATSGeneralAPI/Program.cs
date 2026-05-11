using AihrlyATSGeneralAPI.Data;
using AihrlyATSGeneralAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AihrlyDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddScoped<AihrlyATSGeneralAPI.Services.IPipelineService, AihrlyATSGeneralAPI.Services.PipelineService>();

builder.Services.AddSingleton<AihrlyATSGeneralAPI.Services.INotificationQueue, AihrlyATSGeneralAPI.Services.NotificationQueue>();
builder.Services.AddHostedService<AihrlyATSGeneralAPI.Services.NotificationWorker>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.EnvironmentName != "Testing")
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AihrlyDbContext>();
        db.Database.EnsureCreated();
        
        if (!db.TeamMembers.Any())
        {
            db.TeamMembers.AddRange(
                new TeamMember { Name = "Alice Recruiter", Email = "alice@aihrly.com", Role = "recruiter" },
                new TeamMember { Name = "Bob Manager", Email = "bob@aihrly.com", Role = "hiring_manager" }
            );
            db.SaveChanges();
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program { }
