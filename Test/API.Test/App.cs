using FastEndpoints.Testing;
using HashidsNet;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using UrlShortenerService.Application.Common.Interfaces;
using UrlShortenerService.Infrastructure.Persistence.Interceptors;
using UrlShortenerService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace API.Test;
public class App : AppFixture<Program>
{
    protected override Task SetupAsync()
    {
        // place one-time setup for the fixture here
        return Task.CompletedTask;
    }

    protected override void ConfigureApp(IWebHostBuilder a)
    {
        // do host builder configuration here
    }

    protected override void ConfigureServices(IServiceCollection s)
    {
        // do test service registration here
        _ = s.AddSingleton<IHashids>(
        new Hashids(
              salt: "my-secret-key",
              minHashLength: 6,
              alphabet: "abcdefghijklmnopqrstuvwxyz1234567890"
        ));

        _ = s.AddScoped<AuditableEntitySaveChangesInterceptor>();
        _ = s.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("UrlShortenerServiceDb"));

        _ = s.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        _ = s.AddScoped<ApplicationDbContextInitialiser>();
    }

    protected override Task TearDownAsync()
    {
        // do cleanups here
        return Task.CompletedTask;
    }
}
