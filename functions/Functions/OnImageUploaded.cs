using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System;

namespace ThorstenHans.ImageTagger.Functions
{
    public class OnImageUploaded
    {
        protected ImageTaggerConfig Config { get; }
        protected ILogger<OnImageUploaded> Log { get; }
        public OnImageUploaded(IOptions<ImageTaggerConfig> options,
            ILogger<OnImageUploaded> log)
        {
            Config = options.Value;
            Log = log;
        }

        [FunctionName("OnImageUploaded")]
        public async Task Run([BlobTrigger("images/{name}", Connection = "ImagesStorageAccount")] BlobClient image, string name)
        {
            Log.LogInformation($"OnImageUploaded invoked for {image.Name}");
            var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(Config.SubscriptionKey))
            {
                Endpoint = Config.Endpoint
            };

            var features = new List<VisualFeatureTypes?> { VisualFeatureTypes.Tags };

            var result = await client.AnalyzeImageWithHttpMessagesAsync(image.Uri.ToString(), features);

            Log.LogDebug($"Received response from Azure Computer Vision (StatusCode {result.Response.StatusCode})");

            var tags = result.Body.Tags
                .Select((tag, index) => new { index = $"tag_{index}", tag })
                .ToDictionary(x => x.index, x => x.tag.Name);

            await image.SetMetadataAsync(tags);
            Log.LogDebug($"Hashtags for image '{image.Name}' stored in blob metadata ({tags.Count} tags)");
        }
    }
}
