using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;

namespace RaceDayApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly string _containerName = "event-images";

        public ImagesController(IConfiguration configuration)
        {
            // Reads the Azure Blob connection string from appsettings.json
            _connectionString = configuration.GetConnectionString("AzureBlobStorage");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

                if (string.IsNullOrEmpty(_connectionString))
                    return StatusCode(500, "Azure Blob Storage connection string is missing.");

                var blobServiceClient = new BlobServiceClient(_connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

                await blobContainerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var blobClient = blobContainerClient.GetBlobClient(fileName);

                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }

                return Ok(new { ImageUrl = blobClient.Uri.ToString() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}