using ContainerManager.Domain.Entities;
using ContainerManager.Domain.Interfaces;
using ContainerManager.Domain.ValueObjects;

namespace ContainerManager.Infrastructure.Docker
{
    public class FakeDockerService : IContainerService
    {
        public Task<bool> ResumeContainerAsync(string containerId)
        {
            if (containerId == "fail")
            {
                Console.WriteLine("❌ [FakeDockerService] Simulated failure");
                return Task.FromResult(false);
            }

            Console.WriteLine($"✅ [FakeDockerService] Would start container {containerId}");
            return Task.FromResult(true);
        }

        public Task<bool> StopContainerAsync(string containerId)
        {
            if (containerId == "fail")
            {
                Console.WriteLine("❌ [FakeDockerService] Simulated failure to stop container");
                return Task.FromResult(false);
            }

            Console.WriteLine($"✅ [FakeDockerService] Would stop container: {containerId}");
            return Task.FromResult(true);
        }

        public Task<List<ContainerInfo>> ListContainersAsync(string imageFilter = null)
        {
            var containers = new List<ContainerInfo>
            {
                new ContainerInfo
                {
                    Id = "abc123",
                    Name = "sm64-server",
                    Image = "sm64coopdx_server",
                    ImageId = "sha256:123abc",
                    Status = "Up 5 minutes",
                    State = "running",
                    SizeRw = 1024,
                    SizeRootFs = 50000,
                    Ports = new List<string> { "64->7777/udp" },
                    Mounts = new List<string> { "/home/bou33ou/mods -> /app/mods" },
                    Created = DateTime.Now.AddMinutes(-5)
                },
                new ContainerInfo
                {
                    Id = "def456",
                    Name = "minecraft-server",
                    Image = "minecraft_server",
                    ImageId = "sha256:456def",
                    Status = "Exited (0)",
                    State = "exited",
                    SizeRw = 2048,
                    SizeRootFs = 150000,
                    Ports = new List<string> { "25565->25565/tcp" },
                    Mounts = new List<string> { "/home/bou33ou/world -> /data" },
                    Created = DateTime.Now.AddHours(-2)
                }
            };

            if (!string.IsNullOrEmpty(imageFilter))
                containers = containers.Where(c => c.Image == imageFilter).ToList();

            Console.WriteLine($"📦 [FakeDockerService] Returning {containers.Count} fake containers (filter: {imageFilter ?? "none"})");
            return Task.FromResult(containers);
        }

        public async Task<ContainerStats> GetContainerStatsAsync(string containerId)
        {
            // Simulate delay
            await Task.Delay(100);

            Console.WriteLine($"📊 [FakeDockerService] Returning stats for container: {containerId}");

            return new ContainerStats
            {
                MemoryUsage = 150 * 1024 * 1024,        // 150 MB
                MemoryLimit = 512 * 1024 * 1024,        // 512 MB
                CpuTotalUsage = 1_000_000_000,          // 1s of CPU
                CpuSystemUsage = 2_000_000_000,         // 2s of system CPU
                CpuCores = 2,
                NetworkRxBytes = 5_000_000,             // 5 MB received
                NetworkTxBytes = 3_000_000              // 3 MB sent
            };
        }

        public Task<string?> LoadImageAsync(string tarPath)
        {
            if (tarPath.EndsWith("sm64coopdx.tar"))
                return Task.FromResult<string?>("sm64coopdx_server:latest");

            if (tarPath.EndsWith("minecraft.tar"))
                return Task.FromResult<string?>("minecraft_server:latest");

            return Task.FromResult<string?>(null);
        }

        public Task<string> StartContainerAsync(ContainerInstance instance)
        {
            string fakeContainerId = $"started-container-{Guid.NewGuid()}";
            return Task.FromResult(fakeContainerId);
        }
    }
}
