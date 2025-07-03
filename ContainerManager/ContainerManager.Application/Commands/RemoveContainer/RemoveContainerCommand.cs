using MediatR;

namespace ContainerManager.Application.Commands;

public record RemoveContainerCommand(string ContainerId) : IRequest<bool>;