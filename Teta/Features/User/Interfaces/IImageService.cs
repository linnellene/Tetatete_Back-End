namespace TetaBackend.Features.User.Interfaces;

public interface IImageService
{
    public Task<List<string>> UploadImage(List<IFormFile> images);
}