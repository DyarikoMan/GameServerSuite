using ContainerManager.Application.Dtos;
using MediatR;

namespace ContainerManager.Application.Commands;

public record StartContainerCommand(StartContainerRequest request) : IRequest<string>;

