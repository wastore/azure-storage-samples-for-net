﻿using Azure.Storage.Blobs;
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
            string downloadFilePath = filePath + ".download";
            BlobClient blobClient = new BlobClient(
                connectionString,
                containerName,
                fileName).WithClientSideEncryptionOptions(clientSideOption);
            blobClient.DownloadTo(downloadFilePath);

            ////Optional for encryption, change fileName to differentiate from original blob
            fileName = "MMK" + fileName;

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
            blobClient.Upload(downloadFilePath);
        }

        //Delete files in the Data folder            
        public static void CleanUp(string path)
        {
            Directory.Delete(path, true);
        }

        /*
         * Program migrates a client side encrypted blob to server side encryption using an encryption scope  with Microsoft managed keys
         *
         * NOTE: This program requires the following to be stored in the App.Config file:
         * Azure Active Directory Tenant ID - tenantId
         * Service Principal Application ID - clientId
         * Service Principal Password - clientSecret
         * Storage Account Connection String- connectionString
         * Key Vault Key Uri - keyVaultKeyUri
         * Key Wrap Algorithm - keyWrapAlgorithm
         * Container Name - containerName
         * Blob Name - blobName
         * Encryption Scope Name - encryptionScopeName 
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

            //File Path for local file used to download and reupload
            string localPath = "./data" + Guid.NewGuid().ToString() + "/";
            Directory.CreateDirectory(localPath);
            string localFilePath = Path.Combine(localPath, Constants.blobName);

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
           
            Console.WriteLine("Completed migration to Microsoft Managed Key Server Side Encryption");
        }
    }
}