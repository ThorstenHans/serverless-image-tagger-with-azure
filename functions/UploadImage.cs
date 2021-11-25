using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;

namespace ThorstenHans.XmasTagger
{
    public static class UploadImage
    {
        [FunctionName("UploadImage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "images")] HttpRequest req,
            [Blob("images/{rand-guid}.jpg", FileAccess.Write, Connection = "ImagesStorageAccount")] BlobClient image,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var formData = await req.ReadFormAsync();
            if (formData == null ||
                formData.Files == null ||
                formData.Files.Count == 0 ||
                !formData.Files[0].FileName.EndsWith(".jpg"))
            {
                return new BadRequestResult();
            }
            using (var stream = formData.Files[0].OpenReadStream())
            {
                await image.UploadAsync(stream);
            }
            return new OkObjectResult(new { filename = image.Name });
        }
    }
}
