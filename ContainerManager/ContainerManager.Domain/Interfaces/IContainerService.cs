using ContainerManager.Domain.Entities;

namespace ContainerManager.Domain.Interfaces;

public interface IContainerService
{
      Task<bool> StartContainerAsync(string containerId);
      Task<bool> StopContainerAsync(string containerId);
      Task<List<ContainerInfo>> ListContainersAsync(string imageFilter = null);
}
