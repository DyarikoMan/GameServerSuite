using ContainerManager.Application.Dtos;
using ContainerManager.Domain.Entities;

namespace ContainerManager.Application.Mappers
{
    public static class StartImageRequestMapper
    {
        public static ContainerInstance ToDomain(this StartContainerRequest dto)
        {
            return new ContainerInstance(
                name: dto.Name,
                image: dto.Image,
                ram: new RamSize(dto.RamMb),
                cpu: new CpuCount(dto.Cpu),
                restartPolicy: new RestartPolicyValue(dto.RestartPolicy),
                autoRemove: dto.AutoRemove
            );
        }
    }

}
