using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using System;
using System.IO;

namespace localKeyClientSideToMicrosoftManagedServerSide
{
    class Migration
    { 
        //Downloads and decrypts client side encrypted blob, then reuploads blob with server side encryption using an encryption scope with a Microsoft managed key
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

            //Optional for encryption, change fileName to differentiate from original blob
            fileName = fileName.Replace(".txt", "MMK.txt");

            //Set Blob Client Options with the created Encryption Scope
            BlobClientOptions blobClientOptions = new BlobClientOptions()
            {
                EncryptionScope = encryptionScopeName
            };

            //Reupload Blob with Server Side Encryption
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
        * Program uploads an example client side encrypted blob, and migrates it to server side encryption using Microsoft Managed Keys
        *
        * NOTE: This program requires the following to be stored in the App.Config file:
        * Azure Active Directory Tenant ID - tenantId
        * Service Principal Application ID - clientId
        * Service Principal Password - clientSecret
        * Azure Subscription ID - subscriptionId
        * Resource Group Name - resourceGroup
        * Storage Account Name - storageAccount
        * Storage Account Connection String- connectionString
        * Key Vault Name - keyVaultName
        * Key Wrap Algorithm for Client Side Encryption - keyWrapAlgorithm
        * Customer Provided Key for Client Side Encryption - clientSideCustomerProvidedKey        
        *  
        *  NOTE: This program uses names from Constants.cs, which should be edited as needed
        */
        static void Main()
        {
            //File Path for local file used to upload and reupload
            string localPath = "./data" + Guid.NewGuid().ToString() + "/";
            Directory.CreateDirectory(localPath);
            string localFilePath = Path.Combine(localPath, Constants.fileName);

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

            Console.WriteLine("Completed migration to Microsoft Managed Server Side Encryption");
        }
    }
}