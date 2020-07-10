using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using System;
using System.IO;
using System.Diagnostics;

namespace keyVaultClientSideToMicrosoftManagedServerSide
{
    class Setup
    {
        //Creates example container, client side encrypted blob, and encryption scope for sample
        public static void SetupForExample(
            BlobServiceClient blobService,
            string containerName,
            string fileName,
            string filePath,
            string encryptionScopeName,
            ClientSideEncryptionOptions clientSideOption)
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

            CreateEncryptionScopeMMK(encryptionScopeName);
        }

        //Delete files in the Data folder            
        public static void CleanUp(string path)
        {
            Directory.Delete(path, true);
        }

        //Runs Azure CLI script that creates an encryption scope that uses Microsoft managed keys
        private static void CreateEncryptionScopeMMK(string encryptionScopeName)
        {
            Process process = Process.Start("CMD.exe", "/C az storage account encryption-scope create --name " + encryptionScopeName
                + " -s Microsoft.Storage --account-name " + Constants.storageAccount + " -g " + Constants.resourceGroup + " --subscription " + Constants.subscriptionId);
            process.WaitForExit();
        }
    }
}
