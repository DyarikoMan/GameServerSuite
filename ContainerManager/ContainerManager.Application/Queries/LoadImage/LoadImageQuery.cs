using MediatR;

namespace ContainerManager.Application.Queries;

public record LoadImageQuery(string TarPath) : IRequest<string?>;