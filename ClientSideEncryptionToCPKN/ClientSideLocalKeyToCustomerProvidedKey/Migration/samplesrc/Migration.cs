﻿using Azure.Storage.Blobs;
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
        /*
        * Program migrates a client side encrypted blob to server side encryption using customer provided keys
        *
        * NOTE: This program requires the following to be stored in the App.Config file:
        * Storage Account Connection String- connectionString
        * Key Wrap Algorithm for Client Side Encryption - keyWrapAlgorithm
        * Customer Provided Key for Client Side Encryption - clientSideCustomerProvidedKey     
        * Container Name - containerName
        * Blob Name - blobName
        * Blob Name After Migration - blobNameAfterMigration
        * Customer Provided Key for Server Side Encryption - serverSideCustomerProvidedKey
        */
        static void Main()
        {
            //Get bytes for customer provided key
            byte[] localKeyBytes = ASCIIEncoding.UTF8.GetBytes(Constants.ServerSideCustomerProvidedKey);

            //File Path for local file used to download and reupload
            string localPath = "./data" + Guid.NewGuid().ToString() + "/";
            Directory.CreateDirectory(localPath);
            string localFilePath = Path.Combine(localPath, Constants.BlobName);

            //Creating Key Encryption Key object for Client Side Encryption
            SampleKeyEncryptionKey keyEncryption = new SampleKeyEncryptionKey(Constants.ClientSideCustomerProvidedKey);

            //Set up Client Side Encryption Options used for Client Side Encryption
            ClientSideEncryptionOptions clientSideOptions = new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V1_0)
            {
                KeyEncryptionKey = keyEncryption,
                KeyWrapAlgorithm = Constants.KeyWrapAlgorithm
            };

            try
            {
                //Convert Client Side Encryption Blob to Server Side Encrytion with Customer Provided Keys
                EncryptWithCustomerProvidedKey(
                    Constants.ConnectionString,
                    Constants.ContainerName,
                    Constants.BlobName,
                    Constants.BlobNameAfterMigration,
                    localFilePath,
                    clientSideOptions,
                    localKeyBytes);
            }
            finally
            {
                //Delete downloaded files
                CleanUp(localPath);
            }

            Console.WriteLine("Completed Migration to Customer Provided Key Server Side Encryption");
        }

        //Downloads and decrypts client side encrypted blob, then reuploads blob with server side encryption using a customer provided key
        private static void EncryptWithCustomerProvidedKey(
            string connectionString,
            string containerName,
            string blobName,
            string blobNameAfterMigration,
            string filePath,
            ClientSideEncryptionOptions clientSideOption,
            byte[] keyBytes)
        {
            //Download and decrypt Client Side Encrypted blob using BlobClient with Client Side Encryption Options
            string downloadFilePath = filePath + "download";
            BlobClient blobClient = new BlobClient(
                connectionString,
                containerName,
                blobName).WithClientSideEncryptionOptions(clientSideOption);
            blobClient.DownloadTo(downloadFilePath);

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
                blobNameAfterMigration,
                blobClientOptions);
            blobClient.Upload(downloadFilePath, true);
        }

        //Delete files in the Data folder  
        public static void CleanUp(string path)
        {
            Directory.Delete(path, true);
        }        
    }
}