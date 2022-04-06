using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;

namespace ThorstenHans.ImageTagger.Functions
{
    public static class UploadImage
    {
        [FunctionName("UploadImage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "images")] HttpRequest req,
            [Blob("images/{rand-guid}.jpg", FileAccess.Write, Connection = "ImagesStorageAccount")] BlobClient image,
            ILogger log)
        {
            log.LogInformation("Received request to upload a new image");

            var formData = await req.ReadFormAsync();
            if (formData == null ||
                formData.Files == null ||
                formData.Files.Count == 0 ||
                !formData.Files[0].FileName.EndsWith(".jpg"))
            {
                log.LogDebug("Received bad request from the client. Responding with 400.");
                return new BadRequestResult();
            }
            using (var stream = formData.Files[0].OpenReadStream())
            {
                await image.UploadAsync(stream);
            }
            log.LogInformation($"Uploaded image {formData.Files[0].FileName}. Stored as {image.Name} in Azure Blob Storage.");
            return new OkObjectResult(new { filename = image.Name });
        }
    }
}
