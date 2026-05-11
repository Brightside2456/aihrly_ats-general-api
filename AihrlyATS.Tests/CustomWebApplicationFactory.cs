using AihrlyATSGeneralAPI.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AihrlyATS.Tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.AddDbContext<AihrlyDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AihrlyDbContext>();
            db.Database.EnsureCreated();
            
            if (!db.TeamMembers.Any())
            {
                db.TeamMembers.AddRange(
                    new AihrlyATSGeneralAPI.Models.Entities.TeamMember { Name = "Alice Recruiter", Email = "alice@aihrly.com", Role = "recruiter" },
                    new AihrlyATSGeneralAPI.Models.Entities.TeamMember { Name = "Bob Manager", Email = "bob@aihrly.com", Role = "hiring_manager" }
                );
                db.SaveChanges();
            }
        });
    }
}
