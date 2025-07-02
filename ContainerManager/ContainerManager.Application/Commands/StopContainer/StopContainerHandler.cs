using MediatR;
using ContainerManager.Domain.Interfaces;

namespace ContainerManager.Application.Commands;

public class StopContainerHandler : IRequestHandler<StopContainerCommand, bool>
{
    private readonly IContainerService _containerService;

    public StopContainerHandler(IContainerService containerService)
    {
        _containerService = containerService;
    }

    public async Task<bool> Handle(StopContainerCommand request, CancellationToken cancellationToken)
    {
        return await _containerService.StopContainerAsync(request.ContainerId);
    }
}
