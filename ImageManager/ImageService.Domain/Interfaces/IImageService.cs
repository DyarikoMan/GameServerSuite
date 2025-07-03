namespace ImageManager.Domain.Interfaces;

public interface IImageService
{
    Task<string?> LoadImageAsync(string tarPath);
    List<string> ListAvailableTarImages();
}
