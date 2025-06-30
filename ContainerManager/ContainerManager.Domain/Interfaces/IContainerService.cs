using ContainerManager.Domain.Entities;

namespace ContainerManager.Domain.Interfaces;

public interface IContainerService
{
      Task<string> StartContainerAsync(ContainerInstance container);
}
