using MediatR;

namespace ImageManager.Application.Queries;

public record LoadImageQuery(string TarPath) : IRequest<string?>;