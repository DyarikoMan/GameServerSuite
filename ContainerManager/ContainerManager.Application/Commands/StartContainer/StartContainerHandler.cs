using MediatR;
using ContainerManager.Domain.Entities;
using ContainerManager.Domain.Interfaces;

namespace ContainerManager.Application.Commands;

public class StartContainerHandler : IRequestHandler<StartContainerCommand, string>
{
    private readonly IContainerService _containerService;

    public StartContainerHandler(IContainerService containerService)
    {
        _containerService = containerService;
    }

    public async Task<string> Handle(StartContainerCommand request, CancellationToken cancellationToken)
    {
        var container = new ContainerInstance(
            name: request.Name,
            image: request.Image,
            ram: new RamSize(request.RamMb),
            cpu: new CpuCount(request.CpuCores),
            restartPolicy: new RestartPolicyValue(request.RestartPolicy),
            autoRemove: request.AutoRemove
        );

        return await _containerService.StartContainerAsync(container);
    }

}
