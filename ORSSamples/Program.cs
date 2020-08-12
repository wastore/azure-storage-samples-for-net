using System;
using Azure;
using Azure.Storage.Blobs;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using Azure.Storage.Blobs.Models;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Azure.Storage.Blobs.Specialized;

namespace ORS
{
    class Program
    {
        static void Main(string[] args)
        {
            // Getting variables from config file
            String sourceConnectionString = ConfigurationManager.AppSettings["sourceConnectionString"];
            String destConnectionString = ConfigurationManager.AppSettings["destConnectionString"];
            String sourceContainerName = ConfigurationManager.AppSettings["sourceContainerName"];
            String destContainerName = ConfigurationManager.AppSettings["destContainerName"];
            String blobName = ConfigurationManager.AppSettings["blobName"];
            String blobContent1 = "Hello World!";
            String blobContent2 = "Lorem ipsum";
            int numberBlobs = 1000;

            // Setting up individual clients for source and destination storage accounts
            BlobServiceClient destServiceClient = new BlobServiceClient(destConnectionString);
            BlobContainerClient sourceContainerClient = new BlobContainerClient(sourceConnectionString, sourceContainerName);
            BlobContainerClient destContainerClient = new BlobContainerClient(destConnectionString, destContainerName);

            // Demonstrates ORS features. Archiving using batch currently does not work. 
            // multipleBlobUpdater(sourceContainerClient, destContainerClient, blobName, blobContent1, numberBlobs);
            blobUpdater(sourceContainerClient, destContainerClient, blobName, blobContent1);
            blobUpdater(sourceContainerClient, destContainerClient, blobName, blobContent2);          
            archiveContainerFiles(destContainerClient, destConnectionString, destContainerName);
            // archiveContainerFilesUsingBatch(destServiceClient, destContainerClient);
        }

        /*
         * Uploads 1000 blobs, then checks status of replication in interval of 1 min. Outputs status as a percentage of blobs completed replication
         */
        public static void multipleBlobUpdater(BlobContainerClient sourceContainerClient, BlobContainerClient destContainerClient, String blobName, String blobContent, int numberBlobs)
        {
            // Uploading blobs
            Console.WriteLine("Demonstrating tracking percentage of blobs completed");
            List<string> blobNames = new List<string>();
            Console.WriteLine("Uploading " + numberBlobs + " blobs");
            for (int i = 0; i < numberBlobs; i++)
            {
                String newBlobName = i + blobName;
                BlobClient sourceBlob = sourceContainerClient.GetBlobClient(newBlobName);
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(blobContent));
                sourceBlob.Upload(stream, true);
                blobNames.Add(newBlobName);
            }

            // Check if replication in dest container is finished in interval of 5 min
            Console.WriteLine("Checking to see if replication finished");
            while (blobNames.Count > 0)
            {
                Thread.Sleep(60000);
                List<string> repIncomplete = new List<string>();
                foreach (String currBlobName in blobNames)
                {
                    bool status = checkReplicationStatus(sourceContainerClient, currBlobName);
                    if (!status)
                    {
                        repIncomplete.Add(currBlobName);
                    }
                }
                blobNames = repIncomplete;
                Console.WriteLine("Percentage completed: " + ((numberBlobs - blobNames.Count) / numberBlobs * 100 + "%"));
                Console.WriteLine((numberBlobs - blobNames.Count) + " out of " + numberBlobs + " blobs have completed");
            }
        }

        /*
         * Checks replication status of given blob in given container
         */
        public static bool checkReplicationStatus(BlobContainerClient sourceContainerClient, String blobName)
        {
            BlobClient sourceBlob = sourceContainerClient.GetBlobClient(blobName);
            Response<BlobProperties> source_response = sourceBlob.GetProperties();
            IList<ObjectReplicationPolicy> policyList = source_response.Value.ObjectReplicationSourceProperties;

            // If policyList is null, then replication still in progress
            if (policyList != null)
            {
                foreach (ObjectReplicationPolicy policy in policyList)
                {
                    foreach (ObjectReplicationRule rule in policy.Rules)
                    {
                        if (rule.ReplicationStatus.ToString() != "Complete")
                        {
                            Console.WriteLine("Replication failed for " + blobName);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        /*
         * Uploads a single blob (or updates it in the case where blob already exists in container), checks replication completion, 
         * then demonstrates that source and destination blobs have identical contents
         */
        public static void blobUpdater(BlobContainerClient sourceContainerClient, BlobContainerClient destContainerClient, String blobName, String blobContent)
        {
            // Uploading blobs
            Console.WriteLine("Demonstrating replication of blob in source has same contents in destination container");
            BlobClient sourceBlob = sourceContainerClient.GetBlobClient(blobName);
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(blobContent));
            sourceBlob.Upload(stream, true);
            Console.WriteLine("Added blob " + blobName + " containing content: " + blobContent);
            
            // Check if replication in dest container is finished in interval of 5 min
            Console.WriteLine("Checking to see if replication finished");
            bool replicationSuccess = false;
            bool replicationStatusFlag = true;
            while (replicationStatusFlag)
            {
                Thread.Sleep(300000);
                Response<BlobProperties> source_response = sourceBlob.GetProperties();
                IList<ObjectReplicationPolicy> policyList = source_response.Value.ObjectReplicationSourceProperties;

                // If policyList is null, then replication still in progress
                if (policyList != null)
                {
                    foreach (ObjectReplicationPolicy policy in policyList)
                    {
                        foreach (ObjectReplicationRule rule in policy.Rules)
                        {
                            if (rule.ReplicationStatus.ToString() != "Complete")
                            {
                                Console.WriteLine("Blob replication failed");
                                // Environment.Exit(1);
                                return;
                            }
                            // Comparing source and dest blobs
                            Console.WriteLine("Blob successfully replicated");
                            MemoryStream sourceStream = new MemoryStream();
                            sourceBlob.DownloadTo(sourceStream);
                            String sourceContent = Encoding.UTF8.GetString(sourceStream.ToArray());
                            Console.WriteLine("Source Blob Content: " + sourceContent);

                            MemoryStream destStream = new MemoryStream();
                            BlobClient destBlob = destContainerClient.GetBlobClient(blobName);
                            destBlob.DownloadTo(destStream);
                            String destContent = Encoding.UTF8.GetString(destStream.ToArray());
                            Console.WriteLine("Destination Blob Content: " + destContent);
                            replicationStatusFlag = false;
                        }
                    }
                }
            }
        }

        // Iterates through all blobs in destination container and changes access tier to archive
        public static void archiveContainerFiles(BlobContainerClient containerClient, String connectionString, String containerName)
        {
            foreach (BlobItem blob in containerClient.GetBlobs())
            {
                Process process = Process.Start("CMD.exe", "/C az storage blob set-tier --connection-string " + connectionString + " --container-name " + containerName + " --name \"" + blob.Name +"\" --tier Archive");
                process.WaitForExit();
            }
        }

        // Changes all blobs in destination container to archive access tier using blob batch
        public static void archiveContainerFilesUsingBatch(BlobServiceClient serviceClient, BlobContainerClient containerClient)
        {
            // Getting list of blob uris
            Pageable<BlobItem> allBlobs = containerClient.GetBlobs();
            Uri[] blobList = new Uri[allBlobs.Count()];
            int count = 0;
            foreach (BlobItem blob in allBlobs)
            {
                BlobClient blobClient = containerClient.GetBlobClient(blob.Name);
                blobList[count] = blobClient.Uri;
                count++;
            }

            // Creating batch client then setting access tier for all blobs
            BlobBatchClient batchClient = serviceClient.GetBlobBatchClient();
            batchClient.SetBlobsAccessTier(blobList, AccessTier.Archive);
        }
    }
}
