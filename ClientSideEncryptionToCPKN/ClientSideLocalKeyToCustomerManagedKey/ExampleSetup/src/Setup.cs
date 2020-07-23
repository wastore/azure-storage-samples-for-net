using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using Azure.Core;
using Azure.Security.KeyVault.Keys;
using Azure.Identity;
using System;
using System.IO;
using System.Diagnostics;

namespace ExampleSetup
{
    class Setup
    {
        /*Creates example container, client side encrypted blob, Key Vault key, and encryption scope for sample
         * 
         * NOTE: This program requires the following to be stored in the App.Config file:
         * Subscription ID - subscriptionId
         * Resource Group Name - resourceGroup
         * Storage Account Name - storageAccount
         * Storage Account Connection String- connectionString
         * 
         * Creates example objects using names from Constants.cs, which may be edited as needed
         */
        public static void SetupForExample(
            BlobServiceClient blobService,
            string containerName,
            string fileName,
            string encryptionScopeName,
            ClientSideEncryptionOptions clientSideOption,
            string keyVaultName,
            string keyVaultKeyName,
            TokenCredential credential
            )
        {
            //Create example Container and .txt file, upload .txt file as client side encrypted blob
            BlobContainerClient containerClient = blobService.CreateBlobContainer(containerName);
            //Create BlobClient with Client Side Encryption Options to upload client side encrypted data
            BlobClient blobClient = containerClient.GetBlobClient(fileName).WithClientSideEncryptionOptions(clientSideOption);

            //Upload blob with client side encryption
            blobClient.Upload(Path.Combine(Constants.SamplePath, Constants.BlobName));
            Console.WriteLine("Uploaded to Blob storage as blob: \n\t {0}\n", blobClient.Uri);

            //Create key and use created key to create encryption scope
            KeyVaultKey keyVaultKey = Setup.CreateKeyVaultKey(keyVaultName, keyVaultKeyName, credential);
            Setup.CreateEncryptionScopeCMK(encryptionScopeName, keyVaultKey.Id);
        }

        //Creates a key in a specified Azure Key Vault to use for customer managed key server side encryption
        private static KeyVaultKey CreateKeyVaultKey(string keyVaultName, string keyVaultKeyName, TokenCredential credential)
        {
            Uri keyVaultUri = new Uri("https://" + keyVaultName + ".vault.azure.net");
            KeyClient keyClient = new KeyClient(keyVaultUri, credential);
            KeyType keyType = new KeyType("RSA");
            keyClient.CreateKey(keyVaultKeyName, keyType);
            return keyClient.GetKey(keyVaultKeyName);
        }

        //Runs Azure CLI script that creates an encryption scope that uses customer managed keys with the specified Azure Key Vault key
        private static void CreateEncryptionScopeCMK(string encryptionScopeName, Uri keyVaultKeyUri)
        {
            Process process = Process.Start("CMD.exe", "/C az storage account encryption-scope create --name " + encryptionScopeName
                + " -s Microsoft.KeyVault -u " + keyVaultKeyUri.ToString() + " --account-name " + Constants.StorageAccount + " -g " + Constants.ResourceGroup + " --subscription " + Constants.SubscriptionId);
            process.WaitForExit();
        }
        
        static void Main()
        {
            //Credentials of Service Principal
            TokenCredential credential =
                new ClientSecretCredential(
                    Constants.TenantId,
                    Constants.ClientId,
                    Constants.ClientSecret
                    );
            //Creating Key Encryption Key object for Client Side Encryption
            SampleKeyEncryptionKey keyEncryptionKey = new SampleKeyEncryptionKey(Constants.ClientSideCustomerProvidedKey);

            //Set up Client Side Encryption Options used for Client Side Encryption
            ClientSideEncryptionOptions clientSideOptions = new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V1_0)
            {
                KeyEncryptionKey = keyEncryptionKey,
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
                Constants.KeyVaultName,
                Constants.KeyVaultKeyName,
                credential);

            Console.WriteLine("Completed creation of example container, blob, Key Vault key, and encryption scope");
        }
    }
}
