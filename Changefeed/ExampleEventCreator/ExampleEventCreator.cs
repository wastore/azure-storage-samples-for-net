using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Text;
using System.Configuration;

namespace ExampleEventCreator
{
    class ExampleEventCreator
    {
        //Creates container and blobs within a storage account to populate changefeed and add example events
        static void Main(string[] args)
        {
            string connectionString = ConfigurationManager.AppSettings["connectionString"]; ;
            string containerName = "test-changefeed-container";
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient blobContainerClient = blobServiceClient.CreateBlobContainer(containerName);               
            
            for(int i = 0; i < 5; i++)
            {
                var upStream = new MemoryStream(Encoding.ASCII.GetBytes("blob"));
                BlobClient blobClient = new BlobClient(connectionString, containerName, "exampleblob" + i.ToString());
                blobClient.Upload(upStream,true);
            }
            Console.WriteLine("Finished creating example events");
        }
    }
}
