using MediatR;
using ContainerManager.Domain.Interfaces;

namespace ContainerManager.Application.Commands;

public class ResumeContainerHandler : IRequestHandler<ResumeContainerCommand, bool>
{
    private readonly IContainerService _containerService;

    public ResumeContainerHandler(IContainerService containerService)
    {
        _containerService = containerService;
    }

    public async Task<bool> Handle(ResumeContainerCommand request, CancellationToken cancellationToken)
    {
        return await _containerService.ResumeContainerAsync(request.ContainerId);
    }

}
