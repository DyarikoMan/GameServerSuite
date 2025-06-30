using MediatR;

namespace ContainerManager.Application.Commands;

public class StartContainerCommand : IRequest<string>
{
    public string Image { get; set; }
    public string? Name { get; set; }
    public int RamMb { get; set; } = 512;
    public double CpuCores { get; set; } = 1.0;
    public bool AutoRemove { get; set; } = true;
    public string RestartPolicy { get; set; } = "no";
}
