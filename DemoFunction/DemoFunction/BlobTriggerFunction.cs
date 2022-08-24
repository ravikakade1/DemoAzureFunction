using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DemoFunction
{
    public class BlobTriggerFunction
    {
        [FunctionName("BlobTriggerFunction")]
        public void Run([BlobTrigger("samplecontainer/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var json = Convert(myBlob);
            PostJSON(json);

        }

        public static string Convert(Stream blob)
        {
            var csv = new CsvReader(new StreamReader(blob), CultureInfo.InvariantCulture);
            csv.Read();
            csv.ReadHeader();
            var csvRecords = csv.GetRecords<object>().ToList();

            return JsonConvert.SerializeObject(csvRecords);
        }

        public static async Task PostJSON(string json)
        {
            var request = (HttpWebRequest)WebRequest.Create("<Your Web App URL>");

            var data = Encoding.ASCII.GetBytes(json);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
        }

    }
}
