using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using System;
using System.IO;

namespace keyVaultClientSideToCustomerProvidedServerSide
{
    class Setup
    {
        //Creates example container and client side encrypted blob for sampple
        public static void SetupForExample(
            BlobServiceClient blobService,
            string containerName,
            string fileName,
            string filePath,
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
        }

        //Delete files in the Data folder      
        public static void CleanUp(string path)
        {                  
            Directory.Delete(path, true);
        }        
    }
}
