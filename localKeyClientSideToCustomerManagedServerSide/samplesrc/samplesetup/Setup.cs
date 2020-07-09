using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using Azure.Core;
using Azure.Security.KeyVault.Keys;
using System;
using System.IO;
using System.Configuration;
using System.Diagnostics;

namespace localKeyClientSideToCustomerManagedServerSide
{
    class Setup
    {
        public static void SetupForExample(
            BlobServiceClient blobService,
            string containerNameString,
            string fileNameString,
            string filePathString,
            string encryptionScopeName,
            ClientSideEncryptionOptions clientSideOption,
            string keyVaultName,
            string keyVaultKeyName,
            TokenCredential credential
            )
        {
            //Create example Container and Blob, upload blob as a txt file containing "Hello World!"
            BlobContainerClient containerClient = blobService.CreateBlobContainer(containerNameString);
            //Create BlobClient with Client Side Encryption Options to upload client side encrypted data
            BlobClient blobClient = containerClient.GetBlobClient(fileNameString).WithClientSideEncryptionOptions(clientSideOption);

            //Create text file in Data folder to upload
            File.WriteAllText(filePathString, "Hello World!");

            using FileStream uploadFileStream = File.OpenRead(filePathString);
            blobClient.Upload(uploadFileStream, true);
            uploadFileStream.Close();
            Console.WriteLine("Uploaded to Blob storage as blob: \n\t {0}\n", blobClient.Uri);

            //Create key and use created key to create encryption scope
            KeyVaultKey keyVaultKey = Setup.CreateKeyVaultKey(keyVaultName, keyVaultKeyName, credential);
            Setup.CreateEncryptionScopeCMK(encryptionScopeName, keyVaultKey.Id);
        }

        private static KeyVaultKey CreateKeyVaultKey(string keyVaultName, string keyVaultKeyName, TokenCredential credential)
        {
            //Creates a key in a specified Azure Key Vault to use for customer managed key server side encryption
            Uri keyVaultUri = new Uri("https://" + keyVaultName + ".vault.azure.net");
            KeyClient keyClient = new KeyClient(keyVaultUri, credential);
            KeyType keyType = new KeyType("RSA");
            keyClient.CreateKey(keyVaultKeyName, keyType);
            return keyClient.GetKey(keyVaultKeyName);
        }

        public static void CleanUp(string path)
        {
            //Delete files in the Data folder            
            Directory.Delete(path, true);
        }

        private static void CreateEncryptionScopeCMK(string encryptionScopeName, Uri keyVaultKeyUri)
        {
            //Runs Azure CLI script that creates an encryption scope that uses customer managed keys with the specified Azure Key Vault key
            var config = ConfigurationManager.AppSettings;
            Process process = Process.Start("CMD.exe", "/C az storage account encryption-scope create --name " + encryptionScopeName
                + " -s Microsoft.KeyVault -u " + keyVaultKeyUri.ToString() + " --account-name " + config["storageAccount"] + " -g " + config["resourceGroup"] + " --subscription " + config["subscriptionId"]);
            process.WaitForExit();
        }
    }
}
