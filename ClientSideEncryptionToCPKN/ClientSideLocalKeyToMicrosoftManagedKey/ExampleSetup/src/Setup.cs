using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
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
         * Key Vault Name - keyVaultName
         * 
         * Creates example objects using names from Constants.cs, which may be edited as needed
         */
        public static void SetupForExample(
            BlobServiceClient blobService,
            string containerName,
            string fileName,
            string encryptionScopeName,
            ClientSideEncryptionOptions clientSideOption
            )
        {
            //Create example Container and .txt file, upload .txt file as client side encrypted blob
            BlobContainerClient containerClient = blobService.CreateBlobContainer(containerName);
            //Create BlobClient with Client Side Encryption Options to upload client side encrypted data
            BlobClient blobClient = containerClient.GetBlobClient(fileName).WithClientSideEncryptionOptions(clientSideOption);
            blobClient.Upload(Path.Combine(Constants.samplePath, Constants.blobName));
            Console.WriteLine("Uploaded to Blob storage as blob: \n\t {0}\n", blobClient.Uri);

            Setup.CreateEncryptionScopeMMK(encryptionScopeName);
        }

        //Runs Azure CLI script that creates an encryption scope that uses Microsoft managed keys
        private static void CreateEncryptionScopeMMK(string encryptionScopeName)
        {
            Process process = Process.Start("CMD.exe", "/C az storage account encryption-scope create --name " + encryptionScopeName
                + "MMK -s Microsoft.Storage --account-name " + Constants.storageAccount + " -g " + Constants.resourceGroup + " --subscription " + Constants.subscriptionId);
            process.WaitForExit();
        }
        
        static void Main()
        {
            //Creating Key Encryption Key object for Client Side Encryption
            SampleKeyEncryptionKey keyEncryption = new SampleKeyEncryptionKey(Constants.clientSideCustomerProvidedKey);

            //Set up Client Side Encryption Options used for Client Side Encryption
            ClientSideEncryptionOptions clientSideOptions = new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V1_0)
            {
                KeyEncryptionKey = keyEncryption,
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
                clientSideOptions);

            Console.WriteLine("Completed creation of example container, blob, and encryption scope");
        }
    }
}
