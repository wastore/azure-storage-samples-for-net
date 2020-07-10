using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using System;
using System.Text;
using System.IO;

namespace localKeyClientSideToCustomerProvidedServerSide
{
    class Migration
    {
        //Downloads and decrypts client side encrypted blob, then reuploads blob with server side encryption using a customer provided key
        private static void EncryptWithCustomerProvidedKey(
            string connectionString,
            string containerName,
            string fileName,
            string filePath,
            ClientSideEncryptionOptions clientSideOption,
            byte[] keyBytes)
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
            fileName = fileName.Replace(".txt", "CPK.txt");

            //Set Blob Client Options with the given Customer Provided Key
            CustomerProvidedKey customerProvidedKey = new CustomerProvidedKey(keyBytes);
            BlobClientOptions blobClientOptions = new BlobClientOptions()
            {
                CustomerProvidedKey = customerProvidedKey,
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
        * Program uploads an example client side encrypted blob, and migrates it to server side encryption using Customer Provided Keys
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
            //Get bytes for customer provided key
            byte[] localKeyBytes = ASCIIEncoding.UTF8.GetBytes(Constants.customerProvidedKey);

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
                clientSideOptions);

            //Convert Client Side Encryption Blob to Server Side Encrytion with Customer Provided Keys
            EncryptWithCustomerProvidedKey(
                Constants.connectionString,
                Constants.containerName,
                Constants.fileName,
                localFilePath,
                clientSideOptions,
                localKeyBytes);

            //Delete downloaded files
            Setup.CleanUp(localPath);

            Console.WriteLine("Completed Migration to Customer Provided Key Server Side Encryption");
        }
    }
}