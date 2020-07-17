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
        //Creates example container, client side encrypted blob, and encryption scope for sample
        public static void SetupForExample(
            BlobServiceClient blobService,
            string containerName,
            string fileName,
            string encryptionScopeName,
            ClientSideEncryptionOptions clientSideOption,
            Uri keyVaultKeyUri)
        {
            //Create example Container and .txt file, upload .txt file as client side encrypted blob
            BlobContainerClient containerClient = blobService.CreateBlobContainer(containerName);
            //Create BlobClient with Client Side Encryption Options to upload client side encrypted data
            BlobClient blobClient = containerClient.GetBlobClient(fileName).WithClientSideEncryptionOptions(clientSideOption);

            //Upload blob with client side encryption            
            blobClient.Upload(Path.Combine(Constants.samplePath, Constants.blobName));
            Console.WriteLine("Uploaded to Blob storage as blob: \n\t {0}\n", blobClient.Uri);

            CreateEncryptionScopeCMK(encryptionScopeName, keyVaultKeyUri);
        }

        //Runs Azure CLI script that creates an encryption scope that uses customer managed keys with the specified Azure Key Vault key
        private static void CreateEncryptionScopeCMK(string encryptionScopeName, Uri keyVaultKeyUri)
        {
            Process process = Process.Start("CMD.exe", "/C az storage account encryption-scope create --name " + encryptionScopeName
                + " -s Microsoft.KeyVault -u " + keyVaultKeyUri.ToString() + " --account-name " + Constants.storageAccount + " -g " + Constants.resourceGroup + " --subscription " + Constants.subscriptionId);
            process.WaitForExit();
        }

        static void Main()
        {
            //Credentials of Service Principal
            TokenCredential credential =
                new ClientSecretCredential(
                    Constants.tenantId,
                    Constants.clientId,
                    Constants.clientSecret
                    );

            //Get Uri for Key Vault key
            Uri keyVaultKeyUri = new Uri(Constants.keyVaultKeyUri);
            //Create CryptographyClient using Key Vault Key
            CryptographyClient cryptographyClient = new CryptographyClient(keyVaultKeyUri, credential);
            //Set up Client Side Encryption Options used for Client Side Encryption
            ClientSideEncryptionOptions clientSideOptions = new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V1_0)
            {
                KeyEncryptionKey = cryptographyClient,
                KeyWrapAlgorithm = Constants.keyWrapAlgorithm
            };

            //Create Blob Service Client
            BlobServiceClient blobServiceClient = new BlobServiceClient(Constants.connectionString);

            //Run Setup Function that creates and example container and blob
            SetupForExample(
                blobServiceClient,
                Constants.containerName,
                Constants.blobName,
                Constants.encryptionScopeName,
                clientSideOptions,
                keyVaultKeyUri);

            Console.WriteLine("Completed creation of example container, blob, and encryption scope");
        }
    }
}
