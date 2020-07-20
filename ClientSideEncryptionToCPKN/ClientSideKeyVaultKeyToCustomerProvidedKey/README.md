# Key Vault Client Side Encryption to Customer Provided Key Server Side Encryption
Migration Sample From Key Vault Client Side Encryption to Customer Provided Key Server Side Encryption

## General Info
This sample contains two programs: ExampleSetup and Migration. ExampleSetup is an optional setup process that uploads an example client side encrypted blob (blobExample.txt in samplesetup folder) to a newly created container in the provided storage account.
Migration then migrates a specified blob to server side encryption using a customer provided key and uploads the server side encrypted blob in the same container. 

## Prerequisites
Requires installation of [.NET Core](https://dotnet.microsoft.com/download/dotnet-core).
Requires an [Azure subscription](https://azure.microsoft.com/en-us/free/) and an 
[Azure storage account](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal).

## Setup Requirements
### ExampleSetup
#### Requires users to enter the following into the App.config file:
* Azure Active Directory Tenant ID - tenantId
* Service Principal Application ID - clientId
* Service Principal Password - clientSecret
* Storage Account Connection String- connectionString
* Key Vault Key Uri - keyVaultKeyUri
* Key Wrap Algorithm - keyWrapAlgorithm

#### The sample uses names for the following from Constants.cs:
* Container Name
* Blob Name

### Migration
#### Requires users to enter the following into the App.config file:
* Azure Active Directory Tenant ID - tenantId
* Service Principal Application ID - clientId
* Service Principal Password - clientSecret
* Storage Account Connection String - connectionString
* Key Vault Key Uri - keyVaultKeyUri
* Key Wrap Algorithm - keyWrapAlgorithm
* Container Name - containerName
* Blob Name - blobName 
* Customer Provided Key for Server Side Encryption - serverSideCustomerProvidedKey

## How To Use
### With ExampleSetup
1. Navigate to ExampleSetup folder
2. Enter values to App.config file
3. Edit names in Constants.cs (Optional)
4. Run 'dotnet build'
5. Run 'dotnet run'
6. Navigate to Migration folder
7. Enter values to App.config file (Must match values entered into ExampleSetup's App.config file)
8. Run 'dotnet build'
9. Run 'dotnet run'

### Without ExampleSetup
1. Navigate to Migration folder
2. Enter values to App.config folder
3. Run 'dotnet build'
4. Run 'dotnet run'
