using MediatR;

namespace ContainerManager.Application.Commands;

public record StartContainerCommand(string ContainerId) : IRequest<bool>;

