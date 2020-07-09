using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using System;
using System.IO;
using System.Configuration;
using System.Diagnostics;

namespace localKeyClientSideToMicrosoftManagedServerSide
{
    class Setup
    {
        public static void SetupForExample(
            BlobServiceClient blobService,
            string containerNameString,
            string fileNameString,
            string filePathString,
            string encryptionScopeName,
            ClientSideEncryptionOptions clientSideOption
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
            
            Setup.CreateEncryptionScopeMMK(encryptionScopeName);
        }

        
        public static void CleanUp(string path)
        {
            //Delete files in the Data folder            
            Directory.Delete(path, true);
        }

        private static void CreateEncryptionScopeMMK(string encryptionScopeName)
        {
            //Runs Azure CLI script that creates an encryption scope that uses Microsoft managed keys
            var config = ConfigurationManager.AppSettings;
            Process process = Process.Start("CMD.exe", "/C az storage account encryption-scope create --name " + encryptionScopeName
                + "MMK -s Microsoft.Storage --account-name " + config["storageAccount"] + " -g " + config["resourceGroup"] + " --subscription " + config["subscriptionId"]);
            process.WaitForExit();
        }
    }
}
