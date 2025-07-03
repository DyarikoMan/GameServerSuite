namespace ContainerManager.Domain.ValueObjects;

public class ContainerStats
{
    public ulong MemoryUsage { get; init; }
    public ulong MemoryLimit { get; init; }
    public double MemoryPercent => MemoryLimit > 0 ? (double)MemoryUsage / MemoryLimit * 100 : 0;

    public ulong CpuTotalUsage { get; init; }
    public ulong CpuSystemUsage { get; init; }
    public uint CpuCores { get; init; }

    public double CpuPercent
    {
        get
        {
            if (CpuSystemUsage == 0 || CpuCores == 0) return 0;
            return (double)CpuTotalUsage / CpuSystemUsage * CpuCores * 100;
        }
    }

    public ulong NetworkRxBytes { get; init; }
    public ulong NetworkTxBytes { get; init; }
}
