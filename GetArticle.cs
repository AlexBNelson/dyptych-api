using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;


namespace articleservice
{
    public static class GetArticle
    {
        [FunctionName("GetArticle")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "article/{id}")] HttpRequest req,
            ILogger log, ExecutionContext context, string id)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var blobConnectionString = config.GetConnectionString("ArticleConnectionString");

            var blobName = "articles";

            BlobClient client = GetBlobClient(id, blobConnectionString, blobName);

            BlobDownloadInfo download = await client.DownloadAsync();

            MemoryStream stream = new MemoryStream();

            stream.Position = 0;//resetting stream's position to 0

            var serializer = new JsonSerializer();

            FormattedArticle result;

            using (var sr = new StreamReader(download.Content))
            {
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                   result = serializer.Deserialize<FormattedArticle>(jsonTextReader);
                }
            }

            return (ActionResult)new JsonResult(result);
        }

        [FunctionName("GetThumbnail")]
        public static async Task<IActionResult> GetThumbnail(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "thumbnail/{id}")] HttpRequest req,
            ILogger log, ExecutionContext context, string id)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var blobConnectionString = config.GetConnectionString("ThumbnailConnectionString");

            var blobName = "articlethumbnails";

            BlobClient client = GetBlobClient(id, blobConnectionString, blobName);

            BlobDownloadInfo download = await client.DownloadAsync();

            MemoryStream stream = new MemoryStream();

            stream.Position = 0;//resetting stream's position to 0

            var serializer = new JsonSerializer();

            Thumbnail result;

            using (var sr = new StreamReader(download.Content))
            {
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                   result = serializer.Deserialize<Thumbnail>(jsonTextReader);
                }
            }

            return (ActionResult)new JsonResult(result);
        }


        private static BlobClient GetBlobClient(string id, string connectionString, string blobName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            var blobContainerClient = blobServiceClient.GetBlobContainerClient(blobName);

            // Get a reference to a blob
            return blobContainerClient.GetBlobClient($"{id}.json");
        }
    }
}
