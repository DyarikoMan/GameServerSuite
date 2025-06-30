using System.Net;
using System.Net.Sockets;
using Docker.DotNet;
using Docker.DotNet.Models;
using ContainerManager.Domain.Interfaces;
using ContainerManager.Domain.Entities;
using ContainerManager.Infrastructure.Docker.Mappers;


namespace ContainerManager.Infrastructure.Docker;

public class DockerService : IContainerService
{
    private readonly DockerClient _client;

    public DockerService()
    {
        _client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
    }

    public async Task<string> StartContainerAsync(ContainerInstance container)
    {
        var createParams = new CreateContainerParameters
        {
            Image = container.Image,
            Name = container.Name,
            Tty = true,

            HostConfig = new HostConfig
            {
                AutoRemove = container.AutoRemove,
                Memory = container.Ram.ValueInMb * 1024 * 1024,
                NanoCPUs = (long)(container.Cpu.Cores * 1_000_000_000),
                RestartPolicy = new RestartPolicy
                {
                    Name = container.RestartPolicy.ToDockerKind()
                }
            }
        };

        var response = await _client.Containers.CreateContainerAsync(createParams);

        bool started = await _client.Containers.StartContainerAsync(response.ID, null);
        if (!started)
            throw new Exception($"❌ فشل تشغيل الكونتينر: {container.Name}");

        return response.ID;
    }


    public async Task<bool> StopContainerAsync(string id)
    {
        return await _client.Containers.StopContainerAsync(id, new ContainerStopParameters());
    }

    public async Task<IEnumerable<object>> ListContainersAsync(bool all = true)
    {
        var containers = await _client.Containers.ListContainersAsync(new ContainersListParameters
        {
            All = all,
            Size = true
        });

        return containers
            .Where(c => c.Image == "sm64coopdx_server") // ✅ Filter only SM64 containers
            .Select(c => new
            {
                c.ID,
                Name = c.Names.FirstOrDefault()?.Trim('/'),
                c.Image,
                c.ImageID,
                c.Status,
                c.State,
                SizeRw = c.SizeRw,
                SizeRootFs = c.SizeRootFs,
                Ports = c.Ports.Select(p => $"{p.PublicPort}->{p.PrivatePort}/{p.Type}").ToList(),
                Mounts = c.Mounts.Select(m => $"{m.Source} -> {m.Destination}").ToList(),
                Created = c.Created.ToLocalTime()
            });
    }


    public async Task<ContainerStatsResponse> GetContainerStatsAsync(string containerId)
    {
        ContainerStatsResponse? latest = null;

        var progress = new Progress<ContainerStatsResponse>(stats =>
        {
            latest = stats;
        });

        await _client.Containers.GetContainerStatsAsync(
            containerId,
            new ContainerStatsParameters { Stream = false },
            progress,
            CancellationToken.None
        );

        return latest!;
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

   public async Task<string> LoadImageAndStartContainerAsync(
    string tarPath,
    string containerBaseDir,
    string? containerName = null,
    int? hostPort = null,
    int? ramMb = null,
    double? cpuCores = null,
    bool autoRemove = false,
    string restartPolicy = "unless-stopped" )
    {
        // 1. Load image from TAR file
        using var stream = File.OpenRead(tarPath);
        await _client.Images.LoadImageAsync(
            new ImageLoadParameters { Quiet = true },
            stream,
            new Progress<JSONMessage>(),
            CancellationToken.None
        );

        containerName ??= $"sm64_{Guid.NewGuid():N}".Substring(0, 12);

        int port = hostPort ?? GetFreeUdpPort();
        long memBytes = (ramMb ?? 512) * 1024L * 1024L;
        long cpuNano = (long)((cpuCores ?? 0.5) * 1_000_000_000);

        RestartPolicyKind policyKind = restartPolicy.ToLower() switch
        {
            "always" => RestartPolicyKind.Always,
            "no" => RestartPolicyKind.No,
            "unless-stopped" => RestartPolicyKind.UnlessStopped,
            _ => RestartPolicyKind.No
        };
        var response = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = "sm64coopdx_server",
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
                AutoRemove = autoRemove,
                RestartPolicy = new RestartPolicy { Name = policyKind },
                //RestartPolicy = new RestartPolicy { Name = RestartPolicyKind.UnlessStopped },
            }
        });

        await _client.Containers.StartContainerAsync(response.ID, null);

        return response.ID;
    }


    public async Task<bool> RemoveContainerAndCleanupAsync(string containerId)
    {
        try
        {
            var container = await _client.Containers.InspectContainerAsync(containerId);
            string containerName = container.Name.Trim('/');

            await _client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters
            {
                Force = true,
                RemoveVolumes = true
            });

            return true;
        }
        catch (DockerContainerNotFoundException)
        {
            return false;
        }
    }



    private static int GetFreeUdpPort()
    {
        using var udp = new UdpClient(0);
        return ((IPEndPoint)udp.Client.LocalEndPoint!).Port;
    }

}
