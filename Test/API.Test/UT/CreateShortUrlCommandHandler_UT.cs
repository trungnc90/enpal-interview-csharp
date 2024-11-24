using Azure.Core;
using System.Threading;
using HashidsNet;
using Moq;
using UrlShortenerService.Application.Common.Interfaces;
using UrlShortenerService.Application.Url.Commands;
using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Domain.Entities;
using System.Linq;
using UrlShortenerService.Infrastructure.Persistence;
using System.Collections.Generic;

namespace API.Test.UT;
public class CreateShortUrlCommandHandler_UT
{

    private static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
    {
        var queryable = sourceList.AsQueryable();

        var dbSet = new Mock<DbSet<T>>();
        dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => sourceList.Add(s));

        return dbSet.Object;
    }

    [Fact]
    public async Task CreateShortUrlCommandHandler_Test1()
    {
        // arrange
        var mockDbSet = new Mock<DbSet<UrlShortenerService.Domain.Entities.Url>>();
       

        var data = new List<UrlShortenerService.Domain.Entities.Url>
        {
            new UrlShortenerService.Domain.Entities.Url { Id = 1, OriginalUrl = "https://example.com" },
            new UrlShortenerService.Domain.Entities.Url { Id = 2, OriginalUrl = "https://example.org" }
        }.AsQueryable();


        var appDbMock = new Mock<IApplicationDbContext>();

        var url = new UrlShortenerService.Domain.Entities.Url()
        {
            OriginalUrl = ""
        };

        _ = appDbMock.Setup(x => x.Urls).Returns(data);

        var hashId = "yWk0Ao";
        var hashIdsMock = new Mock<IHashids>();
        _ = hashIdsMock.Setup(h => h.Encode(It.IsAny<int>()))
                   .Returns(hashId);



        var shortUrlHandler = new CreateShortUrlCommandHandler(appDbMock.Object, hashIdsMock.Object);
        var request = new CreateShortUrlCommand()
        {
            Url = "httsp://google.com"
        };

        // act
        var response = await shortUrlHandler.Handle(request, new CancellationToken());

        // assert
        Assert.NotNull(response);
        _ = Assert.IsType<string>(response);


        
    }
}
