using ImageManager.Domain.Interfaces;
using Docker.DotNet;
using Docker.DotNet.Models;
using SharpCompress.Archives.Tar;
using System.Text.Json;

namespace ImageManager.Infrastructure
{
    public class ImageService : IImageService
    {
        private readonly DockerClient _client;

        public ImageService()
        {
            _client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
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
    }
}
