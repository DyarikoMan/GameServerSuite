using ContainerManager.Domain.ValueObjects;
using MediatR;

namespace ContainerManager.Application.Queries.GetContainerStats;

public record GetContainerStatsQuery(string ContainerId) : IRequest<ContainerStats>;
