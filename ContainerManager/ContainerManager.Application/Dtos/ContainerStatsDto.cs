namespace ContainerManager.Application.Dtos;

public class ContainerStatsDto
{
    public ulong MemoryUsage { get; init; }
    public ulong MemoryLimit { get; init; }
    public ulong CpuTotalUsage { get; init; }
    public ulong CpuSystemUsage { get; init; }
    public uint CpuCores { get; init; }
    public ulong NetworkRxBytes { get; init; }
    public ulong NetworkTxBytes { get; init; }
}