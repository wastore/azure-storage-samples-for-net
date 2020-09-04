#Parallel Uploading Performance
Sample that tests which combination of upload options is the most time efficient in uploading a series of large blobs to a storage account.

## General Information
This sample consists of one program that uploads a series of 128MB blobs to a container using different combinations of
upload options including maximum concurrency, minimum upload threshold, maximum block size, and virtual machine settings.
The program tracks the time elapsed to upload the batch of blobs and then returns which upload settings combination was the 
most time efficient and which was the least time efficient.

## A Note
The code used to put the data into an excel sheet has at this point not been tested, as it was not compatible with the remote desktop that
this program was being tested on. Therefore, it is currently unknown whether or not that code will throw an error.

## Prerequisites
Requires installation of [.NET Core](https://dotnet.microsoft.com/download/dotnet-core) and [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest).
Requires an [Azure subscription](https://azure.microsoft.com/en-us/free/) and an 
[Azure storage account](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal).
It is not required, but it is suggested to run program on a [Virtual Machine](https://docs.microsoft.com/en-us/azure/virtual-machines/windows/quick-create-portal)
If a Virtual Machine is being used, it is required to also enable [Just-In-Time VM Access](https://docs.microsoft.com/en-us/azure/security-center/security-center-just-in-time?tabs=jit-config-asc%2Cjit-request-asc)

##Setup Requirements
Please navigate to the file Constants.cs and replace variable connectionString with your storage account connection string.
The values for the containerName and blobNameBase may be changed, but it is not required.

##How To Use
Open the file PerformanceSample.cs and run the program. The program will take a long while to run due to the size and number of 
blobs being uploaded. Running this program from inside of the Virtual Machine will speed up the runtime exponentially. Please
ensure that the location/region of your storage account and Virtual Machine are the same (i.e. East US, West US, etc). This will
also aid in the runtime.