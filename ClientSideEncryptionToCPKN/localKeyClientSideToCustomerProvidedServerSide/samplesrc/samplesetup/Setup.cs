using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using System;
using System.IO;

namespace localKeyClientSideToCustomerProvidedServerSide
{
    class Setup
    {
        //Creates example container and client side encrypted blob for sample
        public static void SetupForExample(
            BlobServiceClient blobService,
            string containerName,
            string fileName,
            ClientSideEncryptionOptions clientSideOption
            )
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
        }

        //Delete files in the Data folder  
        public static void CleanUp(string path)
        {          
            Directory.Delete(path, true);
        }
    }
}
