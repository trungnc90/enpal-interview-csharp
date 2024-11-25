using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using FluentAssertions;
using HashidsNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UrlShortenerService.Api.Endpoints.Url;
using UrlShortenerService.Api.Endpoints.Url.Requests;
using UrlShortenerService.Application.Common.Interfaces;

namespace API.Test.Url;

public class CreateShortUrlShould(App TestApp) : TestBase<App>
{
    [Fact]
    public async Task CreateShortUrl_ShouldReturnShortenUrl()
    {
        var url = "https://example.com";

        var (rsp, res) = await TestApp.Client.POSTAsync<CreateShortUrlEndpoint, CreateShortUrlRequest, string>(
            new()
            {
                Url = url
            });

        var context = TestApp.Services.GetService<IApplicationDbContext>();
        Assert.NotNull(context);

        var originalUrl = await context.Urls.SingleOrDefaultAsync(u => u.OriginalUrl == url);
        Assert.NotNull(originalUrl);

        var hashIds = TestApp.Services.GetService<IHashids>();
        Assert.NotNull(hashIds);

        var code = hashIds.EncodeLong(originalUrl.Id);

        _ = rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        _ = res.Should().Be(code);
    }

    [Fact]
    public async Task CreateShortUrl_ShouldReturnBadRequest()
    {
        var (rsp, res) = await TestApp.Client.POSTAsync<CreateShortUrlEndpoint, CreateShortUrlRequest, string>(
            new()
            {
                Url = "example.com"
            });

        _ = rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
