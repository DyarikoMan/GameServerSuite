using ImageManager.Domain.Interfaces;
using MediatR;

namespace ImageManager.Application.Queries;

public class LoadImageHandler : IRequestHandler<LoadImageQuery, string?>
{
    private readonly IImageService _imageService;

    public LoadImageHandler(IImageService imageService)
    {
        _imageService = imageService;
    }

    public async Task<string?> Handle(LoadImageQuery request, CancellationToken cancellationToken)
    {
        return await _imageService.LoadImageAsync(request.TarPath);
    }
}
