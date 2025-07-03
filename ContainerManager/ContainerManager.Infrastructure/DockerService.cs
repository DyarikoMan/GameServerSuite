using ContainerManager.Domain.Entities;
using ContainerManager.Domain.Interfaces;
using ContainerManager.Domain.ValueObjects;
using ContainerManager.Infrastructure.Docker.Mappers;
using Docker.DotNet;
using Docker.DotNet.Models;
using SharpCompress.Archives.Tar;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;


namespace ContainerManager.Infrastructure.Docker;

public class DockerService : IContainerService
{
    private readonly DockerClient _client;

    public DockerService()
    {
        _client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
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
        int ramMb = instance.Ram?.ValueInMb ?? 512;
        long memBytes = ramMb * 1024L * 1024L;
        double cpuCores = instance.Cpu?.Cores ?? 0.5;
        long cpuNano = (long)(cpuCores * 1_000_000_000);
        RestartPolicyKind policyKind = instance.RestartPolicy.ToDockerKind();

        var response = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = instance.Image,
            Name = containerName,
            Tty = true,
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    ["7777/udp"] = new List<PortBinding> { new PortBinding { HostPort = port.ToString() } }
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

            await _client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters
            {
                Force = true,
                RemoveVolumes = true
            });

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to remove container {containerId}: {ex.Message}");
            return false;
        }
    }

    public List<string> ListAvailableTarImages()
    {
        var imageDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "Resources", "Images"
        ));

        if (!Directory.Exists(imageDir))
            return new List<string>();

        return [.. Directory.GetFiles(imageDir, "*.tar").Select(Path.GetFileName)];
    }


    public async Task<string?> LoadImageAsync(string tarPath)
    {
        // 1. Get the image name from manifest.json
        string? imageName = GetImageNameFromTar(tarPath);
        if (imageName == null)
            throw new Exception("Could not extract image name from TAR");

        // 2. Load the image
        try
        {
            using var stream = File.OpenRead(tarPath);
            await _client.Images.LoadImageAsync(
                new ImageLoadParameters { Quiet = true },
                stream,
                new Progress<JSONMessage>(),
                CancellationToken.None
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to load/read image: {ex.Message}");
        }

        return imageName;
    }

    private static string? GetImageNameFromTar(string tarPath)
    {
        using var stream = File.OpenRead(tarPath);
        using var archive = TarArchive.Open(stream);

        var manifestEntry = archive.Entries.FirstOrDefault(e => e.Key == "manifest.json");
        if (manifestEntry == null) return null;

        using var reader = new StreamReader(manifestEntry.OpenEntryStream());
        var json = reader.ReadToEnd();

        var doc = JsonDocument.Parse(json);
        var repoTags = doc.RootElement[0].GetProperty("RepoTags");

        return repoTags[0].GetString();
    }

    private static int GetFreeUdpPort()
    {
        using var udp = new UdpClient(0);
        return ((IPEndPoint)udp.Client.LocalEndPoint!).Port;
    }

}
