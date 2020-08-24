## Additional Setup and Instructions for Object Replication Service(ORS) Example
This sample will demonstrate ORS working for two linked service account containers, one that acts as the source container and one that
acts as the destination container. There are multiple features implemented in this sample. 

First, we have MultipleBlobUpdater where 1000 blobs are uploaded to the source container. Then, the replication status of all the blobs
are tracked, and for every minute until completion, the percentage of blobs that have completed replication is outputted. 

Another function called BlobUpdater will create a blob called blobExample.txt that will be uploaded to the source account's container, and 
once replication is complete, the source blob and destination blob's contents will be printed to demonstrate that 
they are identical. Then, the same blob will be updated with new contents and then compared after replication completes.

We also have ArchiveContainerFiles that changes the access tier of all blobs in the destination container to archive. ArchiveContainerFilesUsingBatch
does the same thing but instead uses blob batch. DeleteAllBlobsUsingBatch will delete all of the files in the destination container.

#### Setup
Requires installation of [.NET Core](https://dotnet.microsoft.com/download/dotnet-core),
[Visual Studio 2019](https://visualstudio.microsoft.com/downloads/) with the Azure development workload also installed, and 
an [Azure subscription](https://azure.microsoft.com/en-us/free/). 
[Two storage accounts must be created](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal) that 
can have changefeed and blob versioning enabled. The storage accounts' containers
[must be linked using ORS](https://docs.microsoft.com/en-us/azure/storage/blobs/object-replication-configure?tabs=azure-clil) so that 
all blobs created and updated are reflected in the destination container.

***Disclaimer*** If ORS is not enabled properly, the sample may hang. If more than 8 minutes have passed and there are no additional outputs, 
please check that ORS is properly configured.


#### Code Sample Specific Setup
Requires modification of app.config file. Must add values to variables listed below:
 * *sourceConnectionString* (for source storage account)
 * *destConnectionString* (for destination storage account)
 * *sourceContainerName* 
 * *destContainerName*

 The following do not have to be modified. 
 * *blobName*
 
#### Step-by-Step Instructions to Run Program
1. Follow setup instructions above. Make sure all necessary installations are done and storage accounts are made and linked
2. Build and run Program.cs

