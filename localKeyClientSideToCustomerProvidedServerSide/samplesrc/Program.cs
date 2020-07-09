using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using System;
using System.Text;
using System.IO;
using System.Configuration;

namespace localKeyClientSideToCustomerProvidedServerSide
{
    class Program
    {
        private static void CSEtoSSE(
            string connectionString,
            string containerNameString,
            string fileNameString,
            string filePathString,
            ClientSideEncryptionOptions clientSideOption,
            byte[] keyBytes)
        {
            //Download and decrypt Client Side Encrypted blob using BlobClient with Client Side Encryption Options
            string downloadFilePath = filePathString.Replace(".txt", "Download.txt");
            BlobClient blobClient = new BlobClient(
                connectionString,
                containerNameString,
                fileNameString).WithClientSideEncryptionOptions(clientSideOption);
            BlobDownloadInfo download = blobClient.Download();

            Console.WriteLine("\nDownloading blob to \n\t{0}\n", downloadFilePath);

            using (FileStream downloadFileStream = File.OpenWrite(downloadFilePath))
            {
                download.Content.CopyTo(downloadFileStream);
                downloadFileStream.Close();
            }

            //Set Blob Client Options with the given Customer Provided Key
            CustomerProvidedKey customerProvidedKey = new CustomerProvidedKey(keyBytes);
            BlobClientOptions blobClientOptions = new BlobClientOptions()
            {
                CustomerProvidedKey = customerProvidedKey,
            };

            //Reupload Blob with Server Side Encryption
            blobClient = new BlobClient(
                connectionString,
                containerNameString,
                fileNameString.Replace(".txt", "CPK.txt"),
                blobClientOptions);
            using FileStream uploadFileStream3 = File.OpenRead(downloadFilePath);
            blobClient.Upload(uploadFileStream3, true);
            uploadFileStream3.Close();
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
            var config = ConfigurationManager.AppSettings;

            //Get bytes for customer provided key
            byte[] localKeyBytes = ASCIIEncoding.UTF8.GetBytes(Constants.customerProvidedKey);

            //File Path for local file used to upload and reupload
            string localPath = "./data" + Guid.NewGuid().ToString() + "/";
            Directory.CreateDirectory(localPath);
            string localFilePath = Path.Combine(localPath, Constants.fileName);

            //Creating Key Encryption Key object for Client Side Encryption
            SampleKeyEncryptionKey keyEncryption = new SampleKeyEncryptionKey(config["clientSideCustomerProvidedKey"]);

            //Set up Client Side Encryption Options used for Client Side Encryption
            ClientSideEncryptionOptions clientSideOptions = new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V1_0)
            {
                KeyEncryptionKey = keyEncryption,
                KeyWrapAlgorithm = config["keyWrapAlgorithm"]
            };

            //Create Blob Service Client
            BlobServiceClient blobServiceClient = new BlobServiceClient(config["connectionString"]);

            //Run Setup Function that creates and example container and blob
            Setup.SetupForExample(
                blobServiceClient,
                Constants.containerName,
                Constants.fileName,
                localFilePath,
                clientSideOptions);

            //Convert Client Side Encryption Blob to Server Side Encrytion with Customer Provided Keys
            CSEtoSSE(
                config["connectionString"],
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