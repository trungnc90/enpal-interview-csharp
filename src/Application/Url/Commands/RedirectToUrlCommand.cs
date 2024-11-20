using FluentValidation;
using HashidsNet;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Application.Common.Exceptions;
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

    public RedirectToUrlCommandHandler(IApplicationDbContext context, IHashids hashids)
    {
        _context = context;
        _hashids = hashids;
    }

    public async Task<string> Handle(RedirectToUrlCommand request, CancellationToken cancellationToken)
    {

        var decode = _hashids.Decode(request.Id);
        var idx = int.Parse(string.Concat(decode));

        if (int.TryParse(string.Concat(decode), out int id))
        {
            var url = await _context.Urls.FirstOrDefaultAsync(u => u.Id == id);
            if (url is null)
                throw new NotFoundException($"The short url with Id ({request.Id}) was not found");

            return url.OriginalUrl;
        }
        else
        {
            throw new ArgumentException($"The short url Id ({request.Id}) is invalid");
        }
    }
}
