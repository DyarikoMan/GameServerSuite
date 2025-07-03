using ContainerManager.Domain.Entities;
using ContainerManager.Domain.Interfaces;
using MediatR;

namespace ContainerManager.Application.Commands;

public class StartContainerHandler : IRequestHandler<StartContainerCommand, string>
{
    private readonly IContainerService _containerService;

    public StartContainerHandler(IContainerService containerService)
    {
        _containerService = containerService;
    }

    public async Task<string> Handle(StartContainerCommand command, CancellationToken cancellationToken)
    {
        var instance = new ContainerInstance(
            name: command.request.Name,
            port: command.request.Port,
            ram: new RamSize(command.request.RamMb),
            cpu: new CpuCount(command.request.Cpu),
            restartPolicy: new RestartPolicyValue(command.request.RestartPolicy),
            autoRemove: command.request.AutoRemove,
            image: command.request.Image
        );
        return await _containerService.StartContainerAsync(instance);
    }

}
