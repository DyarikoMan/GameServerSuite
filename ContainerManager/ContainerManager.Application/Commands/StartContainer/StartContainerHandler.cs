using MediatR;
using ContainerManager.Domain.Interfaces;

namespace ContainerManager.Application.Commands;

public class StartContainerHandler : IRequestHandler<StartContainerCommand, bool>
{
    private readonly IContainerService _containerService;

    public StartContainerHandler(IContainerService containerService)
    {
        _containerService = containerService;
    }

    public async Task<bool> Handle(StartContainerCommand request, CancellationToken cancellationToken)
    {
        return await _containerService.StartContainerAsync(request.ContainerId);
    }

}
