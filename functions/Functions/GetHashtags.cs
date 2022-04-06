using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using System.Linq;
using System.Globalization;
using System;

namespace ThorstenHans.ImageTagger.Functions
{
    public static class GetHashtags
    {
        [FunctionName("GetHashtags")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "hashtags/{fileName}")] HttpRequest req,
            [Blob("images/{fileName}", FileAccess.Read, Connection = "ImagesStorageAccount")] BlobClient found,
            string fileName,
            ILogger log)
        {
            log.LogInformation($"Looking for hashtags ({fileName})");

            if (found == null)
            {
                log.LogDebug($"Image ({fileName}) not found. Will return with 404");
                return new NotFoundResult();
            }

            var p = await found.GetPropertiesAsync();
            return new OkObjectResult(p.Value.Metadata.Values.Select(tag => $"#{CleanUp(tag)}"));
        }

        private static string CleanUp(string v)
        {
            if (string.IsNullOrWhiteSpace(v)) return string.Empty;
            var info = CultureInfo.CurrentCulture.TextInfo;
            return info.ToTitleCase(v).Replace(" ", string.Empty).Replace("-", string.Empty);

        }
    }
}
