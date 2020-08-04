# Local Key Client Side Encryption to Customer Managed Key Server Side Encryption
Migration Sample From Local Key Client Side Encryption to Customer Managed Key Server Side Encryption

## General Info
This sample contains two programs: ExampleDataCreator and Migration. ExampleDataCreator is an optional setup process that uploads an example client side encrypted blob (blobExample.txt in samplesetup folder) to a newly created container in the provided storage account.
Migration then migrates the blob to server side encryption using a customer managed key from Azure Key Vault and uploads the server side encrypted blob in the same container. 

## Prerequisites
Requires installation of [.NET Core](https://dotnet.microsoft.com/download/dotnet-core) and [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest).
Requires an [Azure subscription](https://azure.microsoft.com/en-us/free/) and an 
[Azure storage account](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal).

## Setup Requirements
#### Requires users to enter the implementation of IKeyEncryptionKey interface used for client side encryption. 
This sample contains a SampleKeyEncryptionKey for the example, but users must include their implentation to migrate their data. Do not use SampleKeyEncryptionKey for user data.

### ExampleDataCreator
#### Requires users to enter the following into the App.config file:
* Subscription ID - subscriptionId
* Resource Group Name - resourceGroup
* Storage Account Name - storageAccount
* Storage Account Connection String- connectionString

#### The sample uses names for the following from Constants.cs, which may be edited as needed:
* Container Name
* Blob Name
* Encryption Scope Name 
* Key Vault Key Name
* Customer Provided Key for Client Side Encryption
* Key Wrap Algorithm

### Migration
#### Requires users to enter the following into the App.config file:
* Azure Active Directory Tenant ID - tenantId
* Service Principal Application ID - clientId
* Service Principal Password - clientSecret
* Storage Account Connection String - connectionString
* Key Wrap Algorithm - keyWrapAlgorithm
* Customer Provided Key for Client Side Encryption - clientSideCustomerProvidedKey
* Container Name - containerName
* Blob Name - blobName
* Blob Name After Migration (can be same as Blob Name) - blobNameAfterMigration
* Encryption Scope Name - encryptionScopeName

## How To Use
### With ExampleDataCreator
1. Navigate to ExampleDataCreator folder
2. Enter values to App.config file
3. Run 'dotnet build'
4. Run 'dotnet run'
5. Navigate to Migration folder
6. Enter values to App.config file (Must match values entered into ExampleDataCreator's App.config file)
7. Run 'dotnet build'
8. Run 'dotnet run'

### Without ExampleDataCreator
1. Navigate to Migration folder
2. Replace implementation of IKeyEncryptionKey interface in SampleKeyEncryptionKey.cs
3. Enter values to App.config folder
4. Run 'dotnet build'
5. Run 'dotnet run'
