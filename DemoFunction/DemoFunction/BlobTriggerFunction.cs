using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DemoFunction
{
    public class BlobTriggerFunction
    {
        [FunctionName("BlobTriggerFunction")]
        public void Run([BlobTrigger("samplecontainer/{name}.xlsx", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}
