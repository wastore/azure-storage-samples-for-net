using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.ChangeFeed;
using Azure.Storage.Blobs.ChangeFeed.Models;
using Azure;
using System.IO;
using System.Text;
using System.Linq;
using ChangefeedSample;

namespace ChangeFeedSample
{
    public static class ChangefeedSample
    {
        // Storage account connection string
        static string connectionString = Constants.connectionString;

        // Example predicates to filter events from Changefeed
        static string containerName = Constants.containerName;
        static string blobName = Constants.blobName;
        static string eventType = Constants.eventType;
        static Predicate<BlobChangeFeedEvent> containerCheck = (BlobChangeFeedEvent changeFeedEvent) => { return changeFeedEvent.Subject.Contains("/containers/" + containerName + "/"); };
        static Predicate<BlobChangeFeedEvent> blobCheck = (BlobChangeFeedEvent changeFeedEvent) => { return changeFeedEvent.Subject.Contains("/blobs/" + blobName); };
        static Predicate<BlobChangeFeedEvent> eventTypeCheck = (BlobChangeFeedEvent changeFeedEvent) => { return changeFeedEvent.EventType == eventType; };

        /* 
         * This program outputs events from a storage account's changefeed, which may be filtered. 
         * The program iterates through the changefeed using a cursor, which will be saved in a container of the specified storage account. 
         * Before starting the program, delete the cursor from previous runs from the storage account, if it exists
         * To run without filtering, remove if statement in Run and GetCursor
         * To change trigger interval, change the CRON string in Run
         */
        [FunctionName("ChangeFeedSample")]
        public static void Run([TimerTrigger("0 */30 * * * *")]TimerInfo myTimer, ILogger log)
        {
            // Create client for Changefeed
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobChangeFeedClient changeFeedClient = blobServiceClient.GetChangeFeedClient();
            try
            {
                BlobContainerClient containerClient = blobServiceClient.CreateBlobContainer("cursorstoragecontainer");
            }
            catch
            {
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("cursorstoragecontainer");
            }
            BlobClient blobClient = new BlobClient(connectionString, "cursorstoragecontainer","cursorBlob");                      
            
            // Create and get cursor for Changefeed
            string continuationToken = GetCursor(blobClient,changeFeedClient,log);                            

            // Loop through the events in Changefeed with the continuationToken
            foreach (Page<BlobChangeFeedEvent> page in changeFeedClient.GetChanges(continuationToken: continuationToken).AsPages())
            {
                    foreach (BlobChangeFeedEvent changeFeedEvent in page.Values)
                    {
                        if (containerCheck.Invoke(changeFeedEvent) && eventTypeCheck.Invoke(changeFeedEvent) && blobCheck.Invoke(changeFeedEvent))
                        {
                            log.LogInformation($"event: {changeFeedEvent.EventType} at {changeFeedEvent.Subject.Replace("/blobServices/default","")} on {changeFeedEvent.EventTime}");
                        }                    
                    }
                    continuationToken = page.ContinuationToken;
            }

            // Upload new continuationToken to blob
            var upStream = new MemoryStream(Encoding.ASCII.GetBytes(continuationToken));
            blobClient.Upload(upStream, true);
        }

        // Gets a cursor from container or creates a new cursor if it does not exist
        private static string GetCursor(BlobClient blobClient, BlobChangeFeedClient changeFeedClient, ILogger log)
        {
            string continuationToken = null; 

            if (blobClient.Exists())
            {
                // If the continuationToken exists in blob, download and use it
                var stream = new MemoryStream();
                blobClient.DownloadTo(stream);
                continuationToken = Encoding.UTF8.GetString(stream.ToArray());
            }
            else
            {
                // If the continuationToken does not exist in the blob, get the continuationToken from the first item
                Page<BlobChangeFeedEvent> page = changeFeedClient.GetChanges().AsPages(pageSizeHint: 1).First<Page<BlobChangeFeedEvent>>();
                BlobChangeFeedEvent changeFeedEvent = page.Values.First<BlobChangeFeedEvent>();
                if (containerCheck.Invoke(changeFeedEvent) && eventTypeCheck.Invoke(changeFeedEvent) && blobCheck.Invoke(changeFeedEvent))
                {
                    log.LogInformation($"event: {changeFeedEvent.EventType} at {changeFeedEvent.Subject.Replace("/blobServices/default", "")} on {changeFeedEvent.EventTime}");
                }                              
                continuationToken = page.ContinuationToken;
            }

            return continuationToken;
        }
    }
}
