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

namespace ThorstenHans.ImageTagger
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
            if ( found == null){
                return new NotFoundResult();
            }
            
            var p = await found.GetPropertiesAsync();
            
            return new OkObjectResult(p.Value.Metadata.Values.Select(tag=> $"#{ToPascalCase(tag)}"));
        }

        private static string ToPascalCase(string v){
            if (string.IsNullOrWhiteSpace(v)) return "";

            var info = CultureInfo.CurrentCulture.TextInfo;
            return info.ToTitleCase(v).Replace(" ", string.Empty);

        }
    }
}
