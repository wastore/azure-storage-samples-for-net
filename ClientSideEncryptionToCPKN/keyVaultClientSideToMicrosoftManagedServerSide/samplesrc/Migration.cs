using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using Azure.Core;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Identity;
using System;
using System.IO;

namespace keyVaultClientSideToMicrosoftManagedServerSide
{
    class Migration
    {
        //Downloads and decrypts client side encrypted blob, then reuploads blob with server side encryption using an encryption scope with Microsoft managed keys
        private static void EncryptWithMicrosoftManagedKey(
            string connectionString,
            string containerName,
            string fileName,
            string filePath,
            ClientSideEncryptionOptions clientSideOption,
            string encryptionScopeName)
        {
            //Download and decrypt Client Side Encrypted blob using BlobClient with Client Side Encryption Options
            string downloadFilePath = filePath.Replace(".txt", "Download.txt");
            BlobClient blobClient = new BlobClient(
                connectionString,
                containerName,
                fileName).WithClientSideEncryptionOptions(clientSideOption);
            BlobDownloadInfo download = blobClient.Download();

            Console.WriteLine("\nDownloading blob to \n\t{0}\n", downloadFilePath);

            using (FileStream downloadFileStream = File.OpenWrite(downloadFilePath))
            {
                download.Content.CopyTo(downloadFileStream);
                downloadFileStream.Close();
            }

            ////Optional for encryption, change fileName to differentiate from original blob
            fileName = fileName.Replace(".txt", "MMK.txt");

            //Set Blob Client Options with the created Encryption Scope
            BlobClientOptions blobClientOptions = new BlobClientOptions()
            {
                EncryptionScope = encryptionScopeName
            };

            //Reupload Blob with Server Side Encryption using blob with Blob Client Options
            blobClient = new BlobClient(
                connectionString,
                containerName,
                fileName,
                blobClientOptions);
            using FileStream uploadFileStream = File.OpenRead(downloadFilePath);
            blobClient.Upload(uploadFileStream, true);
            uploadFileStream.Close();
        }

        /*
         * Program uploads an example client side encrypted blob, and migrates it to server side encryption using an encryption scope  with Microsoft managed keys
         *
         * NOTE: This program requires the following to be stored in the App.Config file:
         * Azure Active Directory Tenant ID - tenantId
         * Service Principal Application ID - clientId
         * Service Principal Password - clientSecret
         * Azure Subscription ID - subscriptionId
         * Resource Group Name - resourceGroup
         * Storage Account Name - storageAccount
         * Storage Account Connection String- connectionString
         * Key Vault Key Uri - keyVaultKeyUri
         * Key Wrap Algorithm - keyWrapAlgorithm
         * 
         * NOTE: This program uses names from Constants.cs, which should be edited as needed
         * 
         */
        static void Main()
        {            
            //Credentials of Service Principal
            TokenCredential credential =
                new ClientSecretCredential(
                    Constants.tenantId,
                    Constants.clientId,
                    Constants.clientSecret
                    );

            //File Path for local file used to upload and reupload
            string localPath = "./data" + Guid.NewGuid().ToString() + "/";
            Directory.CreateDirectory(localPath);
            string localFilePath = Path.Combine(localPath, Constants.fileName);

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
            Setup.SetupForExample(
                blobServiceClient,
                Constants.containerName,
                Constants.fileName,
                localFilePath,
                Constants.encryptionScopeName,
                clientSideOptions);

            //Convert Client Side Encryption Blob to Server Side Encrytion with Microsoft Managed Keys
            EncryptWithMicrosoftManagedKey(
                Constants.connectionString,
                Constants.containerName,
                Constants.fileName,
                localFilePath,
                clientSideOptions,
                Constants.encryptionScopeName);

            //Delete downloaded files
            Setup.CleanUp(localPath);
            Console.WriteLine("Completed migration to Microsoft Managed Key Server Side Encryption");
        }
    }
}