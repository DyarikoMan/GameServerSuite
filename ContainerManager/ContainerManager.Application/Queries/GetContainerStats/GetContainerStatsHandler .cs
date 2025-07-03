using ContainerManager.Application.Queries.GetContainerStats;
using ContainerManager.Domain.Interfaces;
using ContainerManager.Domain.ValueObjects;
using MediatR;

public class GetContainerStatsHandler : IRequestHandler<GetContainerStatsQuery, ContainerStats>
{
    private readonly IContainerService _service;

    public GetContainerStatsHandler(IContainerService service)
    {
        _service = service;
    }

    public async Task<ContainerStats> Handle(GetContainerStatsQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetContainerStatsAsync(request.ContainerId);
    }
}
