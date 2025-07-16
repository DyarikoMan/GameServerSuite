using System.Runtime.Intrinsics.Arm;

namespace ContainerManager.Domain.Entities;

public class ContainerInstance
{
    public string Name { get; }
    public string Image { get; }
    public int ContainerPort { get; }
    public int Port { get; }
    public bool IsUdp { get; } 
    public RamSize Ram { get; }
    public CpuCount Cpu { get; }
    public RestartPolicyValue RestartPolicy { get; }
    public bool AutoRemove { get; }

    public Dictionary<string, string> EnvironmentVariables { get; } 

    public ContainerInstance(
        string name,
        string image,
        int port,
        int containerPort,
        bool isUdp,
        RamSize ram,
        CpuCount cpu,
        RestartPolicyValue restartPolicy,
        bool autoRemove,
        Dictionary<string, string> environmentVariables)
    {
        Name = name;
        Image = image;
        Port = port;
        ContainerPort = containerPort;
        IsUdp = isUdp;
        Ram = ram;
        Cpu = cpu;
        RestartPolicy = restartPolicy;
        AutoRemove = autoRemove;
        EnvironmentVariables = environmentVariables;
    }
}
