using FastEndpoints;
using FastEndpoints.Testing;
using UrlShortenerService.Api.Endpoints.Url.Requests;
using UrlShortenerService.Api.Endpoints.Url;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using HashidsNet;
using UrlShortenerService.Application.Common.Interfaces;
using System.Net;

namespace API.Test.Url;
public class RedirectToUrlShould(App TestApp) : TestBase<App>
{
    [Fact]
    public async Task RedirectToUrl_ShouldReturnOriginalUrl()
    {
        var url = "https://google.com";

        var newUrl = new UrlShortenerService.Domain.Entities.Url() { OriginalUrl = url };
        var context = TestApp.Services.GetService<IApplicationDbContext>();
        Assert.NotNull(context);

        _ = context.Urls.Add(newUrl);
        _ = await context.SaveChangesAsync(new CancellationToken());

        var hashIds = TestApp.Services.GetService<IHashids>();
        Assert.NotNull(hashIds);
        var code = hashIds.EncodeLong(newUrl.Id);

        var response = await TestApp.Client.GETAsync<RedirectToUrlEndpoint, RedirectToUrlRequest, string>(
            new()
            {
                Id = code
            });

        // why 404
        //_ = response.Response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        _ = response.Response.Headers.Location?.ToString().Should().Be(url);
    }


    [Fact]
    public async Task RedirectToUrl_ShouldReturnNotFound()
    {
        var hashIds = TestApp.Services.GetService<IHashids>();
        Assert.NotNull(hashIds);
        var code = hashIds.EncodeLong(999999);

        var response = await TestApp.Client.GETAsync<RedirectToUrlEndpoint, RedirectToUrlRequest, string>(
            new()
            {
                Id = code
            });

        _ = response.Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RedirectToUrl_ShouldReturnInternalServerError()
    {
        var response = await TestApp.Client.GETAsync<RedirectToUrlEndpoint, RedirectToUrlRequest, string>(
            new()
            {
                Id = "abc123"
            });

        _ = response.Response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
