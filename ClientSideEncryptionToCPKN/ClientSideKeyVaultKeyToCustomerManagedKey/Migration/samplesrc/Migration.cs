﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using Azure.Core;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Identity;
using System;
using System.IO;


namespace keyVaultClientSideToCustomerManagedServerSide
{
    class Migration
    {
        /*
         * Program migrates a client side encrypted blob to server side encryption using an encryption scope with a customer managed key    
         * 
         * NOTE: This program requires the following to be stored in the App.Config file:
         * Azure Active Directory Tenant ID - tenantId
         * Service Principal Application ID - clientId
         * Service Principal Password - clientSecret
         * Storage Account Connection String- connectionString
         * Client Side Key Vault Key Uri - clientSideKeyVaultKeyUri
         * Key Wrap Algorithm - keyWrapAlgorithm
         * Container Name - containerName
         * Blob Name - blobName
         * Blob Name After Migration - blobNameAfterMigration
         * Encryption Scope Name - encryptionScopeName 
         */
        static void Main()
        {
            //Credentials of Service Principal
            TokenCredential credential =
                new ClientSecretCredential(
                    Constants.TenantId,
                    Constants.ClientId,
                    Constants.ClientSecret
                    );

            //File Path for local file used to download and reupload
            string localPath = "./data" + Guid.NewGuid().ToString() + "/";
            Directory.CreateDirectory(localPath);
            string localFilePath = Path.Combine(localPath, Constants.BlobName);

            //Get Uri for Key Vault key
            Uri keyVaultKeyUri = new Uri(Constants.ClientSideKeyVaultKeyUri);
            //Create CryptographyClient using Key Vault Key
            CryptographyClient cryptographyClient = new CryptographyClient(keyVaultKeyUri, credential);
            //Set up Client Side Encryption Options used for Client Side Encryption
            ClientSideEncryptionOptions clientSideOptions = new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V1_0)
            {
                KeyEncryptionKey = cryptographyClient,
                KeyWrapAlgorithm = Constants.KeyWrapAlgorithm
            };

            try
            {
                //Convert Client Side Encryption Blob to Server Side Encrytion with Customer Managed Keys
                EncryptWithCustomerManagedKey(
                    Constants.ConnectionString,
                    Constants.ContainerName,
                    Constants.BlobName,
                    Constants.BlobNameAfterMigration,
                    localFilePath,
                    clientSideOptions,
                    Constants.EncryptionScopeName);
            }
            finally
            {
                //Delete downloaded files
                CleanUp(localPath);
                Console.WriteLine("Completed migration to Customer Managed Key Server Side Encryption");
            }
        }

        //Downloads and decrypts client side encrypted blob, then reuploads blob with server side encryption using an encryption scope with a customer managed key
        private static void EncryptWithCustomerManagedKey(
            string connectionString,
            string containerName,
            string blobName,
            string blobNameAfterMigration,
            string filePath,
            ClientSideEncryptionOptions clientSideOption,
            string encryptionScopeName)
        {
            //Download and decrypt Client Side Encrypted blob using BlobClient with Client Side Encryption Options
            string downloadFilePath = filePath + ".download";
            BlobClient blobClient = new BlobClient(
                connectionString,
                containerName,
                blobName).WithClientSideEncryptionOptions(clientSideOption);
            
            blobClient.DownloadTo(downloadFilePath);

            //Set Blob Client Options with the created Encryption Scope
            BlobClientOptions blobClientOptions = new BlobClientOptions()
            {
                EncryptionScope = encryptionScopeName
            };

            //Reupload Blob with Server Side Encryption using blob with Blob Client Options
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