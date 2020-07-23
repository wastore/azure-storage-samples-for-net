using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Identity;
using Azure.Core;
using System;
using System.IO;
using System.Diagnostics;

namespace ExampleSetup
{
    class Setup
    {
        /*Creates example container, client side encrypted blob, and encryption scope for sample
        * 
        * NOTE: This program requires the following to be stored in the App.Config file:
        * Azure Active Directory Tenant ID - tenantId
        * Service Principal Application ID - clientId
        * Service Principal Password - clientSecret
        * Subscription ID - subscriptionId
        * Resource Group Name - resourceGroup
        * Storage Account Name - storageAccount
        * Storage Account Connection String- connectionString
        * Client Side Key Vault Key Uri - clientSideKeyVaultKeyUri
        * Server Side Key Vault Key Uri - serverSideKeyVaultKeyUri
        * Key Wrap Algorithm - keyWrapAlgorithm
        * 
        * Creates example objects using names from Constants.cs, which may be edited as needed
        */
        static void Main()
        {
            //Credentials of Service Principal
            TokenCredential credential =
                new ClientSecretCredential(
                    Constants.TenantId,
                    Constants.ClientId,
                    Constants.ClientSecret
                    );

            //Get Uri for Key Vault key
            Uri clientSideKeyVaultKeyUri = new Uri(Constants.ClientSideKeyVaultKeyUri);
            Uri serverSideKeyVaultKeyUri = new Uri(Constants.ServerSideKeyVaultKeyUri);
            //Create CryptographyClient using Key Vault Key
            CryptographyClient cryptographyClient = new CryptographyClient(clientSideKeyVaultKeyUri, credential);
            //Set up Client Side Encryption Options used for Client Side Encryption
            ClientSideEncryptionOptions clientSideOptions = new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V1_0)
            {
                KeyEncryptionKey = cryptographyClient,
                KeyWrapAlgorithm = Constants.KeyWrapAlgorithm
            };

            //Create Blob Service Client
            BlobServiceClient blobServiceClient = new BlobServiceClient(Constants.ConnectionString);

            //Run Setup Function that creates and example container and blob
            SetupForExample(
                blobServiceClient,
                Constants.ContainerName,
                Constants.BlobName,
                Constants.EncryptionScopeName,
                clientSideOptions,
                serverSideKeyVaultKeyUri);

            Console.WriteLine("Completed creation of example container, blob, and encryption scope");
        }

        //Creates example container, client side encrypted blob, and encryption scope for sample         
        public static void SetupForExample(
            BlobServiceClient blobService,
            string containerName,
            string fileName,
            string encryptionScopeName,
            ClientSideEncryptionOptions clientSideOption,
            Uri serverSideKeyVaultKeyUri)
        {
            //Create example Container and .txt file, upload .txt file as client side encrypted blob
            BlobContainerClient containerClient = blobService.CreateBlobContainer(containerName);
            //Create BlobClient with Client Side Encryption Options to upload client side encrypted data
            BlobClient blobClient = containerClient.GetBlobClient(fileName).WithClientSideEncryptionOptions(clientSideOption);

            //Upload blob with client side encryption            
            blobClient.Upload(Path.Combine(Constants.SamplePath, Constants.BlobName));
            Console.WriteLine("Uploaded to Blob storage as blob: \n\t {0}\n", blobClient.Uri);

            CreateEncryptionScopeCMK(encryptionScopeName, serverSideKeyVaultKeyUri);
        }

        //Runs Azure CLI script that creates an encryption scope that uses customer managed keys with the specified Azure Key Vault key
        private static void CreateEncryptionScopeCMK(string encryptionScopeName, Uri keyVaultKeyUri)
        {
            Process process = Process.Start("CMD.exe", "/C az storage account encryption-scope create --name " + encryptionScopeName
                + " -s Microsoft.KeyVault -u " + keyVaultKeyUri.ToString() + " --account-name " + Constants.StorageAccount + " -g " + Constants.ResourceGroup + " --subscription " + Constants.SubscriptionId);
            process.WaitForExit();
        }        
    }
}
