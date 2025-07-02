using MediatR;

namespace ContainerManager.Application.Commands;

public record StopContainerCommand(string ContainerId) : IRequest<bool>;