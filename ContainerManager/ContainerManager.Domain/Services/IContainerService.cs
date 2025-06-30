using ContainerManager.Domain.Entities;

namespace ContainerManager.Domain.Services;

public interface IContainerService
{
    Task<string> StartContainerAsync(ContainerInstance container);
}
