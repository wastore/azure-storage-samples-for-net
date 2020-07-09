﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using Azure.Core;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Identity;
using System;
using System.IO;
using System.Configuration;

namespace keyVaultClientSideToMicrosoftManagedServerSide
{
    class Program
    {
        private static void CSEtoSSE(
            string connectionString,
            string containerNameString,
            string fileNameString,
            string filePathString,
            ClientSideEncryptionOptions clientSideOption,
            string encryptionScopeNameString)
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

            //Set Blob Client Options with the created Encryption Scope
            BlobClientOptions blobClientOptions = new BlobClientOptions()
            {
                EncryptionScope = encryptionScopeNameString
            };

            //Reupload Blob with Server Side Encryption using blob with Blob Client Options
            blobClient = new BlobClient(
                connectionString,
                containerNameString,
                fileNameString.Replace(".txt", "MMK.txt"),
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
         * Key Vault Key Uri - keyVaultKeyUri
         * Key Wrap Algorithm - keyWrapAlgorithm
         * 
         * NOTE: This program uses names from Constants.cs, which should be edited as needed
         * 
         */
        static void Main()
        {
            var config = ConfigurationManager.AppSettings;

            //Credentials of Service Principal
            TokenCredential credential =
                new ClientSecretCredential(
                    config["tenantId"],
                    config["clientId"],
                    config["clientSecret"]
                    );

            //File Path for local file used to upload and reupload
            string localPath = "./data" + Guid.NewGuid().ToString() + "/";
            Directory.CreateDirectory(localPath);
            string localFilePath = Path.Combine(localPath, Constants.fileName);

            //Get Uri for Key Vault key
            Uri keyVaultKeyUri = new Uri(config["keyVaultKeyUri"]);
            //Create CryptographyClient using Key Vault Key
            CryptographyClient cryptographyClient = new CryptographyClient(keyVaultKeyUri, credential);
            //Set up Client Side Encryption Options used for Client Side Encryption
            ClientSideEncryptionOptions clientSideOptions = new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V1_0)
            {
                KeyEncryptionKey = cryptographyClient,
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
                Constants.encryptionScopeName,
                clientSideOptions);

            //Convert Client Side Encryption Blob to Server Side Encrytion with Microsoft Managed Keys
            CSEtoSSE(
                config["connectionString"],
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