using HashidsNet;
using Moq;
using UrlShortenerService.Application.Common.Interfaces;
using Moq.EntityFrameworkCore;

namespace API.Test.UnitTest;
public class CreateShortUrlCommand
{

    [Fact]
    public async Task CreateShortUrlCommand_InputIsInDb_ReturnString()
    {
        // Arrange
        var data = new List<UrlShortenerService.Domain.Entities.Url>
        {
            new UrlShortenerService.Domain.Entities.Url { Id = 1, OriginalUrl = "https://example.com/" },
            new UrlShortenerService.Domain.Entities.Url { Id = 2, OriginalUrl = "https://example.org/" }
        };

        var appDbMock = new Mock<IApplicationDbContext>();
        _ = appDbMock.Setup(c => c.Urls).ReturnsDbSet(data);

        var hashId = "abc123";
        var hashIdsMock = new Mock<IHashids>();
        _ = hashIdsMock
            .Setup(h => h.EncodeLong(It.IsAny<long>()))
            .Returns(hashId);

        var handler = new UrlShortenerService.Application.Url.Commands.CreateShortUrlCommandHandler(appDbMock.Object, hashIdsMock.Object);
        var request = new UrlShortenerService.Application.Url.Commands.CreateShortUrlCommand()
        {
            Url = "https://example.com/"
        };

        // act
        var response = await handler.Handle(request, new CancellationToken());

        // assert
        Assert.NotNull(response);
        _ = Assert.IsType<string>(response);
        Assert.Equal(response, hashId);
    }

    [Fact]
    public async Task CreateShortUrlCommand_InputIsNotInDb_ReturnString()
    {
        // Arrange
        var data = new List<UrlShortenerService.Domain.Entities.Url>
        {
            new UrlShortenerService.Domain.Entities.Url { Id = 1, OriginalUrl = "https://example.com/" },
            new UrlShortenerService.Domain.Entities.Url { Id = 2, OriginalUrl = "https://example.org/" }
        };

        var appDbMock = new Mock<IApplicationDbContext>();
        _ = appDbMock.Setup(c => c.Urls).ReturnsDbSet(data);

        var hashId = "abc123";
        var hashIdsMock = new Mock<IHashids>();
        _ = hashIdsMock
            .Setup(h => h.EncodeLong(It.IsAny<long>()))
            .Returns(hashId);

        var handler = new UrlShortenerService.Application.Url.Commands.CreateShortUrlCommandHandler(appDbMock.Object, hashIdsMock.Object);
        var request = new UrlShortenerService.Application.Url.Commands.CreateShortUrlCommand()
        {
            Url = "https://example.net/"
        };

        // act
        var response = await handler.Handle(request, new CancellationToken());

        // assert
        Assert.NotNull(response);
        _ = Assert.IsType<string>(response);
        Assert.Equal(response, hashId);
    }
}
