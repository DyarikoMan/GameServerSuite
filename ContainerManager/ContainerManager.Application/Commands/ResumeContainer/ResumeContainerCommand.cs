using MediatR;

namespace ContainerManager.Application.Commands;

public record ResumeContainerCommand(string ContainerId) : IRequest<bool>;

