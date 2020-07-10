using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using Azure.Core;
using Azure.Security.KeyVault.Keys;
using System;
using System.IO;
using System.Diagnostics;

namespace localKeyClientSideToCustomerManagedServerSide
{
    class Setup
    {
        //Creates example container, client side encrypted blob, Key Vault key, and encryption scope for sample
        public static void SetupForExample(
            BlobServiceClient blobService,
            string containerName,
            string fileName,
            string filePath,
            string encryptionScopeName,
            ClientSideEncryptionOptions clientSideOption,
            string keyVaultName,
            string keyVaultKeyName,
            TokenCredential credential
            )
        {
            //Create example Container and .txt file, upload .txt file as client side encrypted blob
            BlobContainerClient containerClient = blobService.CreateBlobContainer(containerName);
            //Create BlobClient with Client Side Encryption Options to upload client side encrypted data
            BlobClient blobClient = containerClient.GetBlobClient(fileName).WithClientSideEncryptionOptions(clientSideOption);

            //Create text file in Data folder to upload
            File.WriteAllText(filePath, Constants.sampleFileContent);

            using FileStream uploadFileStream = File.OpenRead(filePath);
            blobClient.Upload(uploadFileStream, true);
            uploadFileStream.Close();
            Console.WriteLine("Uploaded to Blob storage as blob: \n\t {0}\n", blobClient.Uri);

            //Create key and use created key to create encryption scope
            KeyVaultKey keyVaultKey = Setup.CreateKeyVaultKey(keyVaultName, keyVaultKeyName, credential);
            Setup.CreateEncryptionScopeCMK(encryptionScopeName, keyVaultKey.Id);
        }

        //Creates a key in a specified Azure Key Vault to use for customer managed key server side encryption
        private static KeyVaultKey CreateKeyVaultKey(string keyVaultName, string keyVaultKeyName, TokenCredential credential)
        {            
            Uri keyVaultUri = new Uri("https://" + keyVaultName + ".vault.azure.net");
            KeyClient keyClient = new KeyClient(keyVaultUri, credential);
            KeyType keyType = new KeyType("RSA");
            keyClient.CreateKey(keyVaultKeyName, keyType);
            return keyClient.GetKey(keyVaultKeyName);
        }

        //Delete files in the Data folder    
        public static void CleanUp(string path)
        {        
            Directory.Delete(path, true);
        }

        //Runs Azure CLI script that creates an encryption scope that uses customer managed keys with the specified Azure Key Vault key
        private static void CreateEncryptionScopeCMK(string encryptionScopeName, Uri keyVaultKeyUri)
        {
            Process process = Process.Start("CMD.exe", "/C az storage account encryption-scope create --name " + encryptionScopeName
                + " -s Microsoft.KeyVault -u " + keyVaultKeyUri.ToString() + " --account-name " + Constants.storageAccount + " -g " + Constants.resourceGroup + " --subscription " + Constants.subscriptionId);
            process.WaitForExit();
        }
    }
}
