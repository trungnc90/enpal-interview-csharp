using FluentValidation;
using HashidsNet;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Application.Common.Interfaces;

namespace UrlShortenerService.Application.Url.Commands;

public record CreateShortUrlCommand : IRequest<string>
{
    public string Url { get; init; } = default!;
}

public class CreateShortUrlCommandValidator : AbstractValidator<CreateShortUrlCommand>
{
    public CreateShortUrlCommandValidator()
    {
        _ = RuleFor(v => v.Url)
          .NotEmpty()
          .WithMessage("Url is required.");
    }
}

public class CreateShortUrlCommandHandler : IRequestHandler<CreateShortUrlCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IHashids _hashids;
    private readonly string _shortHeader = "https://enpal.co/api/";

    public CreateShortUrlCommandHandler(IApplicationDbContext context, IHashids hashids)
    {
        _context = context;
        _hashids = hashids;
    }

    public async Task<string> Handle(CreateShortUrlCommand request, CancellationToken cancellationToken)
    {
        var url = await _context.Urls.FirstOrDefaultAsync(u => u.OriginalUrl.Equals(request.Url));
        if (url is null)
        {
            try
            {
                var newUrl = new Domain.Entities.Url() { OriginalUrl = request.Url };

                _ = _context.Urls.Add(newUrl);
                _ = await _context.SaveChangesAsync(cancellationToken);

                var code = _hashids.EncodeLong(newUrl.Id);
                return _shortHeader + code;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to add the url to the database", ex);
            }
        }
        else
        {
            return _shortHeader + _hashids.EncodeLong(url.Id);
        }
    }
}
