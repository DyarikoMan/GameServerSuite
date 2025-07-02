using ContainerManager.Application.Queries;
using ContainerManager.Domain.Entities;
using ContainerManager.Domain.Interfaces;
using MediatR;

namespace ContainerManager.Application.Handlers;

public class ListContainersQueryHandler : IRequestHandler<ListContainersQuery, List<ContainerInfo>>
{
    private readonly IContainerService _containerService;

    public ListContainersQueryHandler(IContainerService containerService)
    {
        _containerService = containerService;
    }

    public async Task<List<ContainerInfo>> Handle(ListContainersQuery request, CancellationToken cancellationToken)
    {
        return await _containerService.ListContainersAsync(request.ImageFilter);
    }
}
