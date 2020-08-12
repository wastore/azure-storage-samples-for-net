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
        // Storage account connection string
        static string connectionString = ConfigurationManager.AppSettings["connectionString"];
        // Name of created container
        static string containerName = "test-changefeed-container";

        // Creates container and blobs within a storage account to populate changefeed and add example events
        static void Main(string[] args)
        {
            count = 0;
            // Create clients
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            try
            {
                BlobContainerClient blobContainerClient = blobServiceClient.CreateBlobContainer(containerName);
            }
            catch
            {
                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            }
            
            // Create Timer
            Timer timer = null;
            Console.WriteLine("Press Space to start the timer. Press Enter to stop the timer. Press any other key to exit/not run the timer");
            if(Console.ReadKey().Key == ConsoleKey.Spacebar)
            {
                Console.WriteLine("Starting timer...");
                // Timer calls CreateEvents function every 30 minutes, to edit the interval change the last value in the call to the number of milliseconds
                timer = new Timer(new TimerCallback(CreateEvents), null, 0, 1800000);
                while (Console.ReadKey().Key != ConsoleKey.Enter)
                {
                    if (Console.ReadKey().Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine("Stopping timer...");
                        timer.Change(
                            Timeout.Infinite,
                            Timeout.Infinite);
                        break;
                    }
                }
                Console.WriteLine("Timer Stopped");
            }
            else
            {
                Console.WriteLine("\nTimer was not run, program exited");
            }
        }

        // Creates and uploads 3 blobs to populate Changefeed
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
