using ImageManager.Domain.Interfaces;
using MediatR;

namespace ImageManager.Application.Queries;


public class ListTarImagesHandler : IRequestHandler<ListTarImagesQuery, List<string>>
{
    private readonly IImageService _imageService;

    public ListTarImagesHandler(IImageService imageService)
    {
        _imageService = imageService;
    }

    public Task<List<string>> Handle(ListTarImagesQuery request, CancellationToken cancellationToken)
    {
        var result = _imageService.ListAvailableTarImages();
        return Task.FromResult(result);
    }
}
