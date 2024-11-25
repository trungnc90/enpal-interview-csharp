using System.Text;
using HashidsNet;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Moq.EntityFrameworkCore;
using UrlShortenerService.Application.Common.Interfaces;

namespace API.Test.UnitTest;
public class RedirectToUrlCommand
{
    [Fact]
    public async Task RedirectToUrlCommand_InputIsValidCodeAndInCache_ReturnString()
    {
        // Arrange
        var dbData = new List<UrlShortenerService.Domain.Entities.Url>
        {
            new UrlShortenerService.Domain.Entities.Url { Id = 1, OriginalUrl = "https://example.com/" },
            new UrlShortenerService.Domain.Entities.Url { Id = 2, OriginalUrl = "https://example.org/" }
        };

        var appDbMock = new Mock<IApplicationDbContext>();
        _ = appDbMock.Setup(c => c.Urls).ReturnsDbSet(dbData);

        var code = "abc123";
        long hashId = 1;
        var hashIdsMock = new Mock<IHashids>();
        _ = hashIdsMock.Setup(h => h.TryDecodeSingleLong(code, out hashId))
            .Returns(true);

        var value = "https://example.com";
        var cacheData = Encoding.UTF8.GetBytes(value);
        var cacheMock = new Mock<IDistributedCache>();
        _ = cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), new CancellationToken()))
            .ReturnsAsync(cacheData);

        var handler = new UrlShortenerService.Application.Url.Commands.RedirectToUrlCommandHandler(appDbMock.Object, hashIdsMock.Object, cacheMock.Object);
        var request = new UrlShortenerService.Application.Url.Commands.RedirectToUrlCommand()
        {
            Id = code
        };

        // act
        var response = await handler.Handle(request, new CancellationToken());

        // assert
        Assert.NotNull(response);
        _ = Assert.IsType<string>(response);
        Assert.Equal(response, value);
    }

    [Fact]
    public async Task RedirectToUrlCommand_InputIsValidCodeAndNotInCache_ReturnString()
    {
        // Arrange
        var dbData = new List<UrlShortenerService.Domain.Entities.Url>
        {
            new UrlShortenerService.Domain.Entities.Url { Id = 1, OriginalUrl = "https://example.com/" },
            new UrlShortenerService.Domain.Entities.Url { Id = 2, OriginalUrl = "https://example.org/" }
        };

        var appDbMock = new Mock<IApplicationDbContext>();
        _ = appDbMock.Setup(c => c.Urls).ReturnsDbSet(dbData);

        var code = "abc123";
        long hashId = 1;
        var hashIdsMock = new Mock<IHashids>();
        _ = hashIdsMock.Setup(h => h.TryDecodeSingleLong(code, out hashId))
            .Returns(true);

        var cacheMock = new Mock<IDistributedCache>();
        _ = cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), new CancellationToken()))
            .ReturnsAsync((byte[]?)null);

        var handler = new UrlShortenerService.Application.Url.Commands.RedirectToUrlCommandHandler(appDbMock.Object, hashIdsMock.Object, cacheMock.Object);
        var request = new UrlShortenerService.Application.Url.Commands.RedirectToUrlCommand()
        {
            Id = code
        };

        // act
        var response = await handler.Handle(request, new CancellationToken());

        // assert
        Assert.NotNull(response);
        _ = Assert.IsType<string>(response);
        Assert.Equal(response, dbData.ElementAt(0).OriginalUrl);
    }

    [Fact]
    public async Task RedirectToUrlCommand_InputIsValidCodeAndNotFoundInDb_ReturnString()
    {
        // Arrange
        var dbData = new List<UrlShortenerService.Domain.Entities.Url>
        {
            new UrlShortenerService.Domain.Entities.Url { Id = 1, OriginalUrl = "https://example.com/" },
            new UrlShortenerService.Domain.Entities.Url { Id = 2, OriginalUrl = "https://example.org/" }
        };

        var appDbMock = new Mock<IApplicationDbContext>();
        _ = appDbMock.Setup(c => c.Urls).ReturnsDbSet(dbData);

        var code = "abc123";
        long hashId = 999999;
        var hashIdsMock = new Mock<IHashids>();
        _ = hashIdsMock.Setup(h => h.TryDecodeSingleLong(code, out hashId))
            .Returns(true);

        var cacheMock = new Mock<IDistributedCache>();
        _ = cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), new CancellationToken()))
            .ReturnsAsync((byte[]?)null);

        var handler = new UrlShortenerService.Application.Url.Commands.RedirectToUrlCommandHandler(appDbMock.Object, hashIdsMock.Object, cacheMock.Object);
        var request = new UrlShortenerService.Application.Url.Commands.RedirectToUrlCommand()
        {
            Id = code
        };

        // act
        var response = await handler.Handle(request, new CancellationToken());

        // assert
        Assert.NotNull(response);
        _ = Assert.IsType<string>(response);
        Assert.Equal(response, string.Empty);
    }
}
