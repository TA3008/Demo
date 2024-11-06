using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Demo.Application.Services;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Demo.Infrastructure.File
{
    public class StorageAccount : IFileService
    {
        private readonly IConfiguration _configuration;
        private Lazy<string> _connectionString;
        public StorageAccount(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = new Lazy<string>(() => _configuration.GetSection("AzureStorage").Value);
        }

        public string Upsert(string rootFolder, string filePath, string base64FileContent)
        {
            BlobContainerClient container = new BlobContainerClient(_connectionString.Value, rootFolder.ToLower());

            if (!container.Exists())
            {
                container.Create();
                container.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
            }

            byte[] bytes = Convert.FromBase64String(base64FileContent);

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                var blobClient = container.GetBlobClient(filePath);
                blobClient.Upload(ms, true);
                return blobClient.Uri.AbsoluteUri;
            }
        }

        public string UpsertImage(string rootFolder, string filePath, Stream fileStream)
        {
            BlobContainerClient container = new BlobContainerClient(_connectionString.Value, rootFolder.ToLower());

            if (!container.Exists())
            {
                container.Create();
                container.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
            }
            var blobClient = container.GetBlobClient(filePath);
            var imageStream = SaveJpeg(fileStream, 30);
            imageStream.Position = 0;
            blobClient.Upload(imageStream, true);
            return blobClient.Uri.AbsoluteUri;
        }

        public void Delete(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            path = path.Replace("https://", null).Replace("http://", null);
            var containerName = path.Split('/')[1];
            var blobName = path.Replace(path.Split('/')[0] + "/" + containerName, null);

            BlobContainerClient container = new BlobContainerClient(_connectionString.Value, containerName);
            if (container.Exists())
            {
                var blobClient = container.GetBlobClient(blobName);
                if (blobClient.Exists())
                    blobClient.Delete();
            }
        }

        /// <summary> 
        /// Saves an image as a jpeg image, with the given quality 
        /// </summary> 
        /// <param name="path"> Path to which the image would be saved. </param> 
        /// <param name="quality"> An integer from 0 to 100, with 100 being the highest quality. </param> 
        public static Stream SaveJpeg(Stream stream, int quality)
        {
            // Load the image from the stream using ImageSharp
            using (var image = Image.Load<Rgba32>(stream))
            {
                // Create a JPEG encoder with the specified quality
                var encoder = new JpegEncoder { Quality = quality };

                // Save the image to a memory stream in JPEG format
                var outStream = new MemoryStream();
                image.Save(outStream, encoder); // Encode and save the image as a JPEG
                outStream.Position = 0; // Reset the stream position for further use
                return outStream;
            }
        }

        public string ResizeImageJpeg(Stream stream, int w, int h, string rootFolder, string filePath)
        {
            // Load the image from the stream using ImageSharp
            using (var image = Image.Load<Rgba32>(stream))
            {
                // Resize the image to the desired dimensions
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(w, h),
                    Mode = ResizeMode.Max // You can adjust the resizing mode as per your need
                }));

                // Save the resized image to a memory stream in JPEG format
                using (var ms = new MemoryStream())
                {
                    var encoder = new JpegEncoder { Quality = 75 }; // Set the JPEG quality level (adjust as needed)
                    image.Save(ms, encoder); // Encode and save the image as a JPEG
                    ms.Position = 0; // Reset the stream position for further use
                    return UpsertImage(rootFolder, filePath, ms); // Upload the resized image to Azure Blob Storage
                }
            }
        }
    }
}