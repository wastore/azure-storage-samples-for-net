using Azure.Storage.Blobs;
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
            string downloadFilePath = filePath + ".download";
            BlobClient blobClient = new BlobClient(
                connectionString,
                containerName,
                fileName).WithClientSideEncryptionOptions(clientSideOption);
            blobClient.DownloadTo(downloadFilePath);

            //Optional for encryption, change fileName to differentiate from original blob
            fileName = "MMK" + fileName;

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
            blobClient.Upload(downloadFilePath);
        }

        //Delete files in the Data folder  
        public static void CleanUp(string path)
        {
            Directory.Delete(path, true);
        }

        /*
        * Program migrates a client side encrypted blob to server side encryption using Microsoft managed keys
        *
        * NOTE: This program requires the following to be stored in the App.Config file:
        * Storage Account Connection String- connectionString
        * Key Wrap Algorithm for Client Side Encryption - keyWrapAlgorithm
        * Customer Provided Key for Client Side Encryption - clientSideCustomerProvidedKey
        * Container Name - containerName
        * Blob Name - blobName
        * Encryption Scope Name - encryptionScopeName
        */
        static void Main()
        {
            //File Path for local file used to download and reupload
            string localPath = "./data" + Guid.NewGuid().ToString() + "/";
            Directory.CreateDirectory(localPath);
            string localFilePath = Path.Combine(localPath, Constants.blobName);

            //Creating Key Encryption Key object for Client Side Encryption
            SampleKeyEncryptionKey keyEncryption = new SampleKeyEncryptionKey(Constants.clientSideCustomerProvidedKey);

            //Set up Client Side Encryption Options used for Client Side Encryption
            ClientSideEncryptionOptions clientSideOptions = new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V1_0)
            {
                KeyEncryptionKey = keyEncryption,
                KeyWrapAlgorithm = Constants.keyWrapAlgorithm
            };

            try
            {
                //Convert Client Side Encryption Blob to Server Side Encrytion with Microsoft Managed Keys
                EncryptWithMicrosoftManagedKey(
                    Constants.connectionString,
                    Constants.containerName,
                    Constants.blobName,
                    localFilePath,
                    clientSideOptions,
                    Constants.encryptionScopeName);
            }           
            finally
            {
                //Delete downloaded files
                CleanUp(localPath);
            }

            Console.WriteLine("Completed migration to Microsoft Managed Server Side Encryption");
        }
    }
}