namespace ContainerManager.Domain.Entities;

public class ContainerInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public string ImageId { get; set; }
    public string Status { get; set; }
    public string State { get; set; }
    public long? SizeRw { get; set; }
    public long? SizeRootFs { get; set; }
    public List<string> Ports { get; set; }
    public List<string> Mounts { get; set; }
    public DateTime Created { get; set; }
}
