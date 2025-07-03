using ContainerManager.Domain.Interfaces;
using MediatR;

namespace ContainerManager.Application.Queries;

public class LoadImageHandler : IRequestHandler<LoadImageQuery, string?>
{
    private readonly IContainerService _containerService;

    public LoadImageHandler(IContainerService containerService)
    {
        _containerService = containerService;
    }

    public async Task<string?> Handle(LoadImageQuery request, CancellationToken cancellationToken)
    {
        return await _containerService.LoadImageAsync(request.TarPath);
    }
}
