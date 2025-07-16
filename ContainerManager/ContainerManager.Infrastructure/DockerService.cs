using ContainerManager.Domain.Entities;
using ContainerManager.Domain.Interfaces;
using ContainerManager.Domain.ValueObjects;
using ContainerManager.Infrastructure.Docker.Mappers;
using Docker.DotNet;
using Docker.DotNet.Models;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;


namespace ContainerManager.Infrastructure.Docker;

public class DockerService : IContainerService
{
    private readonly DockerClient _client;

    public DockerService(IConfiguration configuration)
    {
        var dockerHost = configuration["Docker:Host"]
                         ?? "unix:///var/run/docker.sock"; 

        _client = new DockerClientConfiguration(new Uri(dockerHost)).CreateClient();
    }

    public async Task<bool> ResumeContainerAsync(string containerId)
    {
        try
        {
            return await _client.Containers.StartContainerAsync(containerId, null);
        }
        catch(Exception ex) 
        {
            Console.WriteLine($"❌ Failed to start container {containerId}: {ex.Message}");
            return false;
        }
    }


    public async Task<bool> StopContainerAsync(string containerId)
    {
        try
        {
            return await _client.Containers.StopContainerAsync(containerId, new ContainerStopParameters());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to stop container {containerId}: {ex.Message}");
            return false;
        }
    }


    public async Task<List<ContainerInfo>> ListContainersAsync(string imageFilter = null)
    {
        try
        {
            var containers = await _client.Containers.ListContainersAsync(new ContainersListParameters
            {
                All = true,
                Size = true
            });

            return containers
                .Where(c => imageFilter == null || c.Image == imageFilter)
                .Select(c => new ContainerInfo
                {
                    Id = c.ID,
                    Name = c.Names.FirstOrDefault()?.Trim('/'),
                    Image = c.Image,
                    ImageId = c.ImageID,
                    Status = c.Status,
                    State = c.State,
                    SizeRw = c.SizeRw,
                    SizeRootFs = c.SizeRootFs,
                    Ports = c.Ports.Select(p => $"{p.PublicPort}->{p.PrivatePort}/{p.Type}").ToList(),
                    Mounts = c.Mounts.Select(m => $"{m.Source} -> {m.Destination}").ToList(),
                    Created = c.Created.ToLocalTime()
                })
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to list containers: {ex.Message}");
            return new List<ContainerInfo>(); // Or consider rethrowing
        }
    }


    public async Task<ContainerStats> GetContainerStatsAsync(string containerId)
    {
        ContainerStatsResponse? dockerStats = null;

        var progress = new Progress<ContainerStatsResponse>(stats =>
        {
            dockerStats = stats;
        });

        await _client.Containers.GetContainerStatsAsync(
            containerId,
            new ContainerStatsParameters { Stream = false },
            progress,
            CancellationToken.None
        );

        if (dockerStats == null)
            throw new Exception($"❌ Failed to fetch stats for container {containerId}");

        var cpuStats = dockerStats.CPUStats;
        var memStats = dockerStats.MemoryStats;
        var networks = dockerStats.Networks ?? new Dictionary<string, NetworkStats>();

        return new ContainerStats
        {
            MemoryUsage = memStats.Usage,
            MemoryLimit = memStats.Limit,
            CpuTotalUsage = cpuStats.CPUUsage?.TotalUsage ?? 0,
            CpuSystemUsage = cpuStats.SystemUsage,
            CpuCores = cpuStats.OnlineCPUs,
            NetworkRxBytes = networks.Values.Aggregate(0UL, (total, n) => total + n.RxBytes),
            NetworkTxBytes = networks.Values.Aggregate(0UL, (total, n) => total + n.TxBytes),
        };
    }

    public async Task<string> StartContainerAsync(ContainerInstance instance)
    {
        var containerName = instance.Name;
        int port = instance.Port == 0 ? GetFreeUdpPort() : instance.Port;
        int containerPort = instance.ContainerPort == 0 ? GetFreeUdpPort() : instance.ContainerPort;
        string protocol = instance.IsUdp ? "udp" : "tcp";
        int ramMb = instance.Ram?.ValueInMb ?? 512;
        long memBytes = ramMb * 1024L * 1024L;
        double cpuCores = instance.Cpu?.Cores ?? 0.5;
        long cpuNano = (long)(cpuCores * 1_000_000_000);
        RestartPolicyKind policyKind = instance.RestartPolicy.ToDockerKind();

        var volumeName = $"{containerName}-data";

        var response = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = instance.Image,
            Name = containerName,
            Tty = true,
            Env = instance.EnvironmentVariables.Select(kv => $"{kv.Key}={kv.Value}").ToList(),
            HostConfig = new HostConfig
            {
                Mounts = new List<Mount>
                {
                    new Mount
                    {
                        Type = "volume",
                        Source = volumeName,
                        Target = "/data"
                    }
                },
                //Binds = new List<string>
                //{
                //    $"{hostVolumePath}:/data"
                //},
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    [containerPort+"/"+protocol] = new List<PortBinding> { new PortBinding { HostPort = port.ToString() } }
                },
                Memory = memBytes,
                MemorySwap = memBytes,
                NanoCPUs = cpuNano,
                AutoRemove = instance.AutoRemove,
                RestartPolicy = new RestartPolicy { Name = policyKind }
            }
        });

        await _client.Containers.StartContainerAsync(response.ID, null);

        return response.ID;
    }


    public async Task<bool> RemoveContainerAsync(string containerId)
    {
        try
        {
            var container = await _client.Containers.InspectContainerAsync(containerId);

            string containerName = container.Name?.TrimStart('/'); 

            await _client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters
            {
                Force = true,
                RemoveVolumes = true
            });

            var volumes = await _client.Volumes.ListAsync();
            if (volumes.Volumes.Any(v => v.Name == $"{containerName}-data"))
            {
                await _client.Volumes.RemoveAsync($"{containerName}-data", force: true);
                Console.WriteLine($"🗑️ Deleted volume: {containerName}-data");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to remove container {containerId}: {ex.Message}");
            return false;
        }
    }

    private static int GetFreeUdpPort()
    {
        using var udp = new UdpClient(0);
        return ((IPEndPoint)udp.Client.LocalEndPoint!).Port;
    }
}
