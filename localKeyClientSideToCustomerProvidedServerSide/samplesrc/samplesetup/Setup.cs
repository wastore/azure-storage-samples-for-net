using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using System;
using System.IO;

namespace localKeyClientSideToCustomerProvidedServerSide
{
    class Setup
    {
        public static void SetupForExample(
            BlobServiceClient blobService,
            string containerNameString,
            string fileNameString,
            string filePathString,
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
        }

        public static void CleanUp(string path)
        {
            //Delete files in the Data folder            
            Directory.Delete(path, true);
        }
    }
}
