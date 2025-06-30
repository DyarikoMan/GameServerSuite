namespace ContainerManager.Domain.Entities;

public class ContainerInstance
{
    public string Name { get; }
    public string Image { get; }
    public RamSize Ram { get; }
    public CpuCount Cpu { get; }
    public RestartPolicyValue RestartPolicy { get; }
    public bool AutoRemove { get; }

    public ContainerInstance(
        string name,
        string image,
        RamSize ram,
        CpuCount cpu,
        RestartPolicyValue restartPolicy,
        bool autoRemove = true)
    {
        Name = name;
        Image = image;
        Ram = ram;
        Cpu = cpu;
        RestartPolicy = restartPolicy;
        AutoRemove = autoRemove;
    }
}
