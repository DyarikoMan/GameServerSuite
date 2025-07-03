using ImageManager.Domain.Interfaces;
using Docker.DotNet;
namespace ImageManager.Infrastructure
{
    public class FakeImageService : IImageService
    {
        public List<string> ListAvailableTarImages()
        {
            // Return dummy TAR file names
            return new List<string>
            {
                "mario_server.tar",
                "quake_server.tar",
                "minecraft_server.tar"
            };
        }

        public Task<string?> LoadImageAsync(string tarPath)
        {
            // Simulate loading and return a fake image name
            var fakeImageName = tarPath.Contains("mario") ? "mario_server:latest"
                             : tarPath.Contains("quake") ? "quake_server:v1"
                             : tarPath.Contains("minecraft") ? "minecraft_server:v2"
                             : "default_game:latest";

            return Task.FromResult<string?>(fakeImageName);
        }
    }
}
