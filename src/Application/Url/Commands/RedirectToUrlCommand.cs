using System.Text;
using FluentValidation;
using HashidsNet;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using UrlShortenerService.Application.Common.Interfaces;

namespace UrlShortenerService.Application.Url.Commands;

public record RedirectToUrlCommand : IRequest<string>
{
    public string Id { get; init; } = default!;
}

public class RedirectToUrlCommandValidator : AbstractValidator<RedirectToUrlCommand>
{
    public RedirectToUrlCommandValidator()
    {
        _ = RuleFor(v => v.Id)
          .NotEmpty()
          .WithMessage("Id is required.");
    }
}

public class RedirectToUrlCommandHandler : IRequestHandler<RedirectToUrlCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IHashids _hashids;
    private readonly IDistributedCache _cache;

    public RedirectToUrlCommandHandler(IApplicationDbContext context, IHashids hashids, IDistributedCache cache)
    {
        _context = context;
        _hashids = hashids;
        _cache = cache;
    }

    public async Task<string> Handle(RedirectToUrlCommand request, CancellationToken cancellationToken)
    {
        var isSuccess = _hashids.TryDecodeSingleLong(request.Id, out long id);
        if (!isSuccess)
            throw new ArgumentException($"The short url Id ({request.Id}) is invalid");

        // mock: use GetAsync() instead of GetStringAsync
        var cachedUrl = await _cache.GetAsync(id.ToString(), cancellationToken);
        if (!(cachedUrl == null || cachedUrl.Length == 0))
        {
            return Encoding.UTF8.GetString(cachedUrl);
        }

        var url = await _context.Urls.FirstOrDefaultAsync(u => u.Id == id);
        if (url is null)
            return string.Empty;

        await _cache.SetAsync(
            id.ToString(),
            Encoding.UTF8.GetBytes(url.OriginalUrl),
            new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            });
        return url.OriginalUrl;
    }
}
