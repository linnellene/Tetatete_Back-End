using System.Net;
using Amazon;
using Amazon.Runtime;
using TetaBackend.Features.User.Interfaces;
using Amazon.S3;
using Amazon.S3.Model;

namespace TetaBackend.Features.User.Services;

public class ImageService : IImageService
{
    private readonly IAmazonS3 _amazonS3Client;
    private readonly string _bucketName;
    private const int MaxImagesCount = 6;

    public ImageService(IConfiguration configuration)
    {
        var accessKey = configuration.GetSection("Amazon:S3:AccessKey").Value;
        var secretKey = configuration.GetSection("Amazon:S3:SecretKey").Value;
        var bucketName = configuration.GetSection("Amazon:S3:BucketName").Value;

        if (accessKey is null || secretKey is null || bucketName is null)
        {
            throw new ArgumentException("Invalid configuration for S3.");
        }

        var credentials = new BasicAWSCredentials(accessKey, secretKey);

        _amazonS3Client = new AmazonS3Client(credentials, RegionEndpoint.USWest1);
        _bucketName = bucketName;
    }

    public async Task<List<string>> UploadImage(List<IFormFile> images)
    {
        if (images.Count is 0 or > MaxImagesCount)
        {
            throw new InvalidDataException("Min images count is 0 and max count is 6.");
        }

        var result = new List<string>();

        foreach (var image in images)
        {
            if (image.Length == 0)
            {
                throw new InvalidDataException("No image was provided.");
            }

            var keyName = Guid.NewGuid() + Path.GetExtension(image.FileName);

            await using var stream = image.OpenReadStream();

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = keyName,
                InputStream = stream,
                ContentType = image.ContentType,
                CannedACL = S3CannedACL.PublicRead,
            };

            var response = await _amazonS3Client.PutObjectAsync(request);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Error uploading image to S3.");
            }

            result.Add($"https://{_bucketName}.s3.amazonaws.com/{keyName}");
        }

        return result;
    }
}