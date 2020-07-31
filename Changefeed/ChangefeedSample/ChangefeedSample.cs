using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.ChangeFeed;
using Azure;
using System.IO;
using System.Text;

namespace changefeedjd
{
    public static class ChangefeedSample
    {
        /* This program outputs events from a storage account's changefeed, which may be filtered. 
         * The program iterates through the changefeed using a cursor, which will be saved in a container of the specified storage account. 
         * Before starting the program, delete the cursor from previous runs from the storage account, if it exists
         */
        [FunctionName("Function1")]
        public static async void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            string connectionString = "CONNECTION_STRING";
            string containerName = "example";

            //Predicate to filter events from Changefeed
            Predicate<BlobChangeFeedEvent> containerCheck = (BlobChangeFeedEvent changeFeedEvent) => { return changeFeedEvent.Subject.Contains("/containers/" + containerName + "/"); };
                        
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobChangeFeedClient changeFeedClient = blobServiceClient.GetChangeFeedClient();
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("storingcursorcontainer");
            BlobClient blobClient = new BlobClient(connectionString, "cursorcontainer","storingcursorblob");                      
                      
            string continuationToken = null;

            if(blobClient.Exists())
            {
                //If the continuationToken exists in blob, download and use it
                var stream = new MemoryStream();
                blobClient.DownloadTo(stream);
                continuationToken = Encoding.UTF8.GetString(stream.ToArray());
            }
            
            else
            {
                //If the continuationToken does not exist in the blob, get the continuationToken from the first item
                await foreach (Page<BlobChangeFeedEvent> page in changeFeedClient.GetChangesAsync().AsPages(pageSizeHint: 1))
                {
                    foreach (BlobChangeFeedEvent changeFeedEvent in page.Values)
                    {
                        log.LogInformation($"event: {changeFeedEvent.EventType} at {changeFeedEvent.Subject.Replace("/blobServices/default", "")} on {changeFeedEvent.EventTime}");
                    }
                    continuationToken = page.ContinuationToken;
                    break;
                }
            }
            
            //Loop through the events in Changefeed with the continuationToken
            await foreach (Page<BlobChangeFeedEvent> page in changeFeedClient.GetChangesAsync(continuationToken: continuationToken).AsPages(pageSizeHint: 1))
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
    }
}
