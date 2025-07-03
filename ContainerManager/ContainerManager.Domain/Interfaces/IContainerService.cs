using ContainerManager.Domain.Entities;
using ContainerManager.Domain.ValueObjects;

namespace ContainerManager.Domain.Interfaces;

public interface IContainerService
{
      Task<bool> ResumeContainerAsync(string containerId);
      Task<bool> StopContainerAsync(string containerId);
      Task<List<ContainerInfo>> ListContainersAsync(string imageFilter);
      Task<ContainerStats> GetContainerStatsAsync(string containerId);
      Task<string?> LoadImageAsync(string tarPath);
      Task<string> StartContainerAsync(ContainerInstance instance);
      Task<bool> RemoveContainerAsync(string containerId);

}
