using System.Drawing;

namespace TetaBackend.Features.User.Interfaces;

public interface IImageService
{
    Task<string> UploadImage(IFormFile image);
}