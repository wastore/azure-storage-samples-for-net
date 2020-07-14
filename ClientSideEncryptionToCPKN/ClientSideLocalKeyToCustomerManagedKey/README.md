# Local Key Client Side Encryption to Customer Managed Key Server Side Encryption
Migration Sample From Local Key Client Side Encryption to Customer Managed Key Server Side Encryption

## General Info
This sample has an optional setup process that uploads an example client side encrypted blob (blobExample.txt in samplesetup folder) and uploads it to a newly created container in the provided storage account.
The sample then migrates the blob to server side encryption using a customer managed key from Azure Key Vault and uploads the server side encrypted blob in the same container. 

## Prerequisites
Requires installation of .NET and [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest).
Requires an [Azure subscription](https://azure.microsoft.com/en-us/free/) and an 
[Azure storage account](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal).

## Setup Requirements
#### Requires users to enter the implementation of IKeyEncryptionKey interface used for client side encryption. 
This sample contains a SampleKeyEncryptionKey for the example, but users must include their implentation to migrate their data. Do not use SampleKeyEncryptionKey for user data.

#### Requires users to enter the following into the App.config file:
* Azure Active Directory Tenant ID - tenantId
* Service Principal Application ID - clientId
* Service Principal Password - clientSecret
* Azure Subscription ID - subscriptionId
* Resource Group Name - resourceGroup
* Storage Account Name - storageAccount
* Storage Account Connection String - connectionString
* Key Vault Name - keyVaultName
* Key Wrap Algorithm - keyWrapAlgorithm
* Client Side Customer Provided Key - clientSideCustomerProvidedKey

#### The sample uses names for the following from Constants.cs:
* Container Name
* File/Blob Name
* Key Vault Key Name
* Encryption Scope Name
