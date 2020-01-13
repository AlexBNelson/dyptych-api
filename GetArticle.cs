using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;


namespace articleservice
{
    public static class GetArticle
    {
        [FunctionName("GetArticle")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "article/{id}")] HttpRequest req,
            ILogger log, ExecutionContext context, string id)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var blobConnectionString = config.GetConnectionString("BlobConnectionString");

            BlobClient client = GetBlobClient(id, blobConnectionString);

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        private static BlobClient GetBlobClient(string id, string connectionString)
        {

            var blobName = "articles";

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            var blobContainerClient = blobServiceClient.GetBlobContainerClient(blobName);

            // Get a reference to a blob
            return blobContainerClient.GetBlobClient($"{id}.json");
        }
    }
}
