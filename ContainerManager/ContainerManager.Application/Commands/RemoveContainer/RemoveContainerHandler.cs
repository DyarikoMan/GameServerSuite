using MediatR;
using ContainerManager.Domain.Interfaces;

namespace ContainerManager.Application.Commands;

public class RemoveContainerHandler : IRequestHandler<RemoveContainerCommand, bool>
{
    private readonly IContainerService _containerService;

    public RemoveContainerHandler(IContainerService containerService)
    {
        _containerService = containerService;
    }

    public async Task<bool> Handle(RemoveContainerCommand request, CancellationToken cancellationToken)
    {
        return await _containerService.RemoveContainerAsync(request.ContainerId);
    }
}
