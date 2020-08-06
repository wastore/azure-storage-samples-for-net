using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Text;
using System.Configuration;
using System.Threading;

namespace ExampleEventCreator
{
    class ExampleEventCreator
    {
        static int count;
        static string connectionString = ConfigurationManager.AppSettings["connectionString"];
        static string containerName = "test-changefeed-container";

        //Creates container and blobs within a storage account to populate changefeed and add example events
        static void Main(string[] args)
        {
            count = 0;
            
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient blobContainerClient = blobServiceClient.CreateBlobContainer(containerName);

            Timer timer = null;
            Console.WriteLine("Press Space to start the timer. Press Enter to stop the timer.");
            while (true)
            {
                if(Console.ReadKey().Key == ConsoleKey.Spacebar)
                {
                    Console.WriteLine("Starting timer...");
                    timer = new Timer(new TimerCallback(CreateEvents), null, 0, 1800000);
                }
                else if(Console.ReadKey().Key == ConsoleKey.Enter)
                {
                    Console.WriteLine("Stopping timer...");
                    timer.Change(
                        Timeout.Infinite,
                        Timeout.Infinite);
                    break;
                }            
            }

                     
        }

        static void CreateEvents(Object stateinfo)
        {
            for (int i = 0; i < 3; i++)
            {
                var upStream = new MemoryStream(Encoding.ASCII.GetBytes("Lorem Ipsum"));
                BlobClient blobClient = new BlobClient(connectionString, containerName, "exampleblob" + count.ToString());
                blobClient.Upload(upStream, true);
                count++;
            }

            Console.WriteLine("Created example events");
        }
    }
}
