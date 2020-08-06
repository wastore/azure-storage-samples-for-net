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

namespace ChangeFeedSample
{
    public static class ChangefeedSample
    {
        //Example predicate to filter events from Changefeed
        static string containerName = "test-changefeed-container";
        static Predicate<BlobChangeFeedEvent> containerCheck = (BlobChangeFeedEvent changeFeedEvent) => { return changeFeedEvent.Subject.Contains("/containers/" + containerName + "/"); };

        /* This program outputs events from a storage account's changefeed, which may be filtered. 
         * The program iterates through the changefeed using a cursor, which will be saved in a container of the specified storage account. 
         * Before starting the program, delete the cursor from previous runs from the storage account, if it exists
         */
        [FunctionName("ChangeFeedSample")]
        public static void Run([TimerTrigger("0 */30 * * * *")]TimerInfo myTimer, ILogger log)
        {
            string connectionString = "CONNECTION_STRING";          
                        
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobChangeFeedClient changeFeedClient = blobServiceClient.GetChangeFeedClient();
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("storingcursorcontainer");
            BlobClient blobClient = new BlobClient(connectionString, "cursorcontainer","storingcursorblob");                      
                      
            string continuationToken = GetCursor(blobClient,changeFeedClient,log);                            

            //Loop through the events in Changefeed with the continuationToken
            foreach (Page<BlobChangeFeedEvent> page in changeFeedClient.GetChanges(continuationToken: continuationToken).AsPages())
            {
                    foreach (BlobChangeFeedEvent changeFeedEvent in page.Values)
                    {
                        if (containerCheck.Invoke(changeFeedEvent))
                        {
                            log.LogInformation($"event: {changeFeedEvent.EventType} at {changeFeedEvent.Subject.Replace("/blobServices/default","")} on {changeFeedEvent.EventTime}");
                        }                    
                    }
                    continuationToken = page.ContinuationToken;
            }

            //Upload new continuationToken to blob
            var upStream = new MemoryStream(Encoding.ASCII.GetBytes(continuationToken));
            blobClient.Upload(upStream, true);
        }

        private static string GetCursor(BlobClient blobClient, BlobChangeFeedClient changeFeedClient, ILogger log)
        {
            string continuationToken = null; 

            if (blobClient.Exists())
            {
                //If the continuationToken exists in blob, download and use it
                var stream = new MemoryStream();
                blobClient.DownloadTo(stream);
                continuationToken = Encoding.UTF8.GetString(stream.ToArray());
            }
            else
            {
                //If the continuationToken does not exist in the blob, get the continuationToken from the first item
                Page<BlobChangeFeedEvent> page = changeFeedClient.GetChanges().AsPages(pageSizeHint: 1).First<Page<BlobChangeFeedEvent>>();
                BlobChangeFeedEvent changeFeedEvent = page.Values.First<BlobChangeFeedEvent>();
                if (containerCheck.Invoke(changeFeedEvent))
                {
                    log.LogInformation($"event: {changeFeedEvent.EventType} at {changeFeedEvent.Subject.Replace("/blobServices/default", "")} on {changeFeedEvent.EventTime}");
                }                              
                continuationToken = page.ContinuationToken;
            }

            return continuationToken;
        }
    }
}
