using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CsvHelper;
using DemoFunction.Model;
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
            PostJSONToSql(json);

        }

        public static string Convert(Stream blob)
        {
            var csv = new CsvReader(new StreamReader(blob), CultureInfo.InvariantCulture);
            csv.Read();
            csv.ReadHeader();
            var csvRecords = csv.GetRecords<object>().ToList();

            return JsonConvert.SerializeObject(csvRecords);
        }

        public static async Task PostJSONToSql(string json)
        {

            var result = JsonConvert.DeserializeObject<List<UserModel>>(json);
            foreach (var item in result)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection("Server=tcp:demoazfunction.database.windows.net,1433;Initial Catalog=demofunction;Persist Security Info=False;User ID=demofuntionadmin;Password=Password@098;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
                    {
                        // Opening a connection
                        connection.Open();


                        // Prepare the SQL Query
                        var query = $"INSERT INTO [dbo].[demoFunction] " +
                            $"        ([FirstName]      " +
                            $"        ,[LastName]      " +
                            $"        ,[MobileNumber]   " +
                            $"        ,[City]    " +
                            $"        ,[Country])" +
                            $"    VALUES        (" +
                            $"       '{item.FirstName}'  " +
                            $"      ,'{item.LastName}'   " +
                            $"      ,{item.MobileNumber}" +
                            $"      ,'{item.City}'" +
                            $"      ,'{item.Country}'" +
                            ")";

                        // Prepare the SQL command and execute query
                        SqlCommand command = new SqlCommand(query, connection);

                        // Open the connection, execute and close connection
                        if (command.Connection.State == System.Data.ConnectionState.Open)
                        {
                            command.Connection.Close();
                        }
                        command.Connection.Open();
                        command.ExecuteNonQuery();

                    }
                }
                catch (Exception e)
                {
                    
                }
            }
           
        }

    }
}
