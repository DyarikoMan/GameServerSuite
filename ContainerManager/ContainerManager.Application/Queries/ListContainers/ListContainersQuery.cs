using ContainerManager.Domain.Entities;
using MediatR;

namespace ContainerManager.Application.Queries;

public record ListContainersQuery(string? ImageFilter = null) : IRequest<List<ContainerInfo>>;
