using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using System;
using System.IO;
using System.Diagnostics;

namespace keyVaultClientSideToCustomerManagedServerSide
{
    class Setup
    {
        //Creates example container, client side encrypted blob, and encryption scope for sample
        public static void SetupForExample(
            BlobServiceClient blobService,
            string containerName,
            string fileName,
            string encryptionScopeName,
            ClientSideEncryptionOptions clientSideOption,
            Uri keyVaultKeyUri)
        {
            //Create example Container and .txt file, upload .txt file as client side encrypted blob
            BlobContainerClient containerClient = blobService.CreateBlobContainer(containerName);
            //Create BlobClient with Client Side Encryption Options to upload client side encrypted data
            BlobClient blobClient = containerClient.GetBlobClient(fileName).WithClientSideEncryptionOptions(clientSideOption);

            //Upload blob with client side encryption
            using FileStream uploadFileStream = File.OpenRead(Path.Combine(Constants.samplePath, Constants.fileName));
            blobClient.Upload(uploadFileStream, true);
            uploadFileStream.Close();
            Console.WriteLine("Uploaded to Blob storage as blob: \n\t {0}\n", blobClient.Uri);

            CreateEncryptionScopeCMK(encryptionScopeName, keyVaultKeyUri);
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
