using System.Data.Common;
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
    //private readonly string _shortHeader = "https://localhost:7072/u/";

    public CreateShortUrlCommandHandler(IApplicationDbContext context, IHashids hashids)
    {
        _context = context;
        _hashids = hashids;
    }


    // Id as unique

    public async Task<string> Handle(CreateShortUrlCommand request, CancellationToken cancellationToken)
    {
        //try catch
        // SingleOrDefault??


        try
        {
            var url = await _context.Urls.FirstOrDefaultAsync(u => u.OriginalUrl.Equals(request.Url));
            if (url is not null)
                return _hashids.EncodeLong(url.Id);

            var newUrl = new Domain.Entities.Url() { OriginalUrl = request.Url };

            _ = _context.Urls.Add(newUrl);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return _hashids.EncodeLong(newUrl.Id);
        }
        catch (DbException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to add the url to the database", ex);
        }
    }
}
