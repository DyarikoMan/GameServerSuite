using MediatR;

namespace ImageManager.Application.Queries;

public record ListTarImagesQuery : IRequest<List<string>>;