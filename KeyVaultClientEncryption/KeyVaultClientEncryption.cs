using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using Azure.Core;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Security.KeyVault.Keys;
using Azure.Identity;
using System;
using System.Text;
using System.IO;

using System.Configuration;
using System.Diagnostics;

namespace KeyClientEncryption
{
    class Program
    {

        static void Main()
        {
            var config = ConfigurationManager.AppSettings;

            //Credentials of Service Principal
            TokenCredential credential =
                new ClientSecretCredential(
                    config["tenantId"],
                    config["clientId"],
                    config["clientSecret"]
                    );
            //Used to create encryption scopes for Server Side Encryption
            string subscriptionId = config["subscriptionId"];
            string resourceGroup = config["resourceGroup"];
            string storageAccount = config["storageAccount"];

            //Connection String of Storage Account
            string connectionString = config["azureConnectionString"];

            //Name of Container to be created in setup or container of blob to be converted to Server Side Encryption
            string containerName = "example" + Guid.NewGuid().ToString();

            //Name of File to be created in setup or name of blob to be converted to Server Side Encryption
            string fileName = "example.txt";

            //Name of KeyVault and Key used for Client Side Encryption and to be used for Customer Managed Key Server Side Encryption
            string keyVaultName = config["keyVaultName"];
            string keyVaultKeyName = config["keyVaultKeyName"];

            //KeyWrap Algorithm used to wrap Content Encryption Key in Client Side Encryption
            string KeyWrapAlgorithm = config["keyWrapAlgorithm"];

            //Name of Encryption Scope to be created for Server Side Encryption
            string encryptionScopeName = config["encryptionScopeName"];

            //Key used for Customer Provided Key Server Side Encryption
            string customerProvidedKey = config["customerProvidedKey"];
            byte[] localKeyBytes = ASCIIEncoding.UTF8.GetBytes(customerProvidedKey);

            //File Path for local file used to upload and reupload
            string localPath = "./data/";
            Directory.CreateDirectory(localPath);
            string localFilePath = Path.Combine(localPath, fileName);

            //Getting Key from Azure Key Vault
            Uri keyVaultUri = new Uri("https://" + keyVaultName + ".vault.azure.net");
            KeyClient keyClient = new KeyClient(keyVaultUri, credential);
            KeyVaultKey keyVaultKey = keyClient.GetKey(keyVaultKeyName);

            //Create CryptographyClient using Key Vault Key
            CryptographyClient cryptographyClient = new CryptographyClient(keyVaultKey.Id, credential);
            KeyResolver keyResolve = new KeyResolver(credential);

            //Set up Client Side Encryption Options used for Client Side Encryption
            ClientSideEncryptionOptions clientSideOptions = new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V1_0)
            {
                KeyEncryptionKey = cryptographyClient,
                KeyResolver = keyResolve,
                KeyWrapAlgorithm = KeyWrapAlgorithm
            };

            //Create Blob Service Client
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            //Run Setup Function that creates and example container and blob
            SetupForExample(blobServiceClient, containerName, fileName, localFilePath, clientSideOptions);

            Console.Write("Press Enter to change to SSE");
            Console.ReadLine();

            //Convert Client Side Encryption Blob to Server Side Encrytion with Microsoft Managed Keys, Customer Managed Keys, and Customer Provided Keys
            CSEtoSSE(subscriptionId, resourceGroup, storageAccount, connectionString, containerName, fileName, localFilePath, clientSideOptions, encryptionScopeName, localKeyBytes, keyVaultKey);

            //CLEAN UP EXAMPLE CONTAINER AND FILES
            Console.Write("Press Enter to begin clean up");
            Console.ReadLine();
            //Delete downloaded files
            CleanUp();

            Console.WriteLine("Complete");

        }




        private static void SetupForExample(BlobServiceClient blobService, string containerNameString, string fileNameString, string filePathString, ClientSideEncryptionOptions clientSideOption)
        {
            //Create example Container and Blob, upload blob as a txt file containing "Hello World!"
            BlobContainerClient containerClient = blobService.CreateBlobContainer(containerNameString);

            BlobClient blobClient = containerClient.GetBlobClient(fileNameString).WithClientSideEncryptionOptions(clientSideOption);

            //Create text file in Data folder to upload
            File.WriteAllText(filePathString, "Hello World!");

            using FileStream uploadFileStream = File.OpenRead(filePathString);
            blobClient.Upload(uploadFileStream, true);
            uploadFileStream.Close();
            Console.WriteLine("Uploaded to Blob storage as blob: \n\t {0}\n", blobClient.Uri);
        }

        private static void CSEtoSSE(string subscriptionId, string resourceGroup, string storageAccount, string connectionString, string containerNameString, string fileNameString, string filePathString, ClientSideEncryptionOptions clientSideOption, string encryptionScopeNameString, byte[] keyBytes, KeyVaultKey keyVaultKey)
        {

            //Download and decrypt Client Side Encrypted blob
            string downloadFilePath = filePathString.Replace(".txt", "Download.txt");
            BlobClient blobClient = new BlobClient(connectionString, containerNameString, fileNameString).WithClientSideEncryptionOptions(clientSideOption);
            BlobProperties blobProperties = blobClient.GetProperties();

            Console.WriteLine("\nDownloading blob to \n\t{0}\n", downloadFilePath);

            BlobDownloadInfo download = blobClient.Download();
            using (FileStream downloadFileStream = File.OpenWrite(downloadFilePath))
            {
                download.Content.CopyTo(downloadFileStream);
                downloadFileStream.Close();
            }

            //MICROSOFT MANAGED KEY SERVER SIDE ENCRYPTION
            //Create Encryption Scope for Microsoft Managed Key Server Side Encryption
            string strCmdText;
            strCmdText = "/C az storage account encryption-scope create --name " + encryptionScopeNameString + "MMK -s Microsoft.Storage --account-name " + storageAccount + " -g " + resourceGroup + " --subscription " + subscriptionId;
            Process process = System.Diagnostics.Process.Start("CMD.exe", strCmdText);
            process.WaitForExit();

            //Set Blob Client Options with the created Encryption Scope
            BlobClientOptions blobClientOptions = new BlobClientOptions()
            {
                EncryptionScope = encryptionScopeNameString + "MMK"
            };

            //Reupload Blob with Server Side Encryption
            blobClient = new BlobClient(connectionString, containerNameString, fileNameString.Replace(".txt", "MMK.txt"), blobClientOptions);
            using FileStream uploadFileStream = File.OpenRead("./data/exampleDownload.txt");
            blobClient.Upload(uploadFileStream, true);
            uploadFileStream.Close();

            //CUSTOMER MANAGED KEY SERVER SIDE ENCRYPTION
            //Create Encryption Scope for Customer Managed Key Server Side Encryption using Key Vault Key
            strCmdText = "/C az storage account encryption-scope create --name " + encryptionScopeNameString + "CMK -s Microsoft.KeyVault -u " + keyVaultKey.Id.ToString() + " --account-name " + storageAccount + " -g " + resourceGroup + " --subscription " + subscriptionId;
            process = System.Diagnostics.Process.Start("CMD.exe", strCmdText);
            process.WaitForExit();

            //Set Blob Client Options with the created Encryption Scope
            blobClientOptions = new BlobClientOptions()
            {
                EncryptionScope = encryptionScopeNameString + "CMK"
            };

            //Reupload Blob with Server Side Encryption
            blobClient = new BlobClient(connectionString, containerNameString, fileNameString.Replace(".txt", "CMK.txt"), blobClientOptions);
            using FileStream uploadFileStream2 = File.OpenRead("./data/exampleDownload.txt");
            blobClient.Upload(uploadFileStream2, true);
            uploadFileStream2.Close();

            //CUSTOMER PROVIDED KEY SERVER SIDE ENCRYPTION
            //Set Blob Client Options with the given Customer Provided Key
            CustomerProvidedKey customerProvidedKey = new CustomerProvidedKey(keyBytes);
            blobClientOptions = new BlobClientOptions()
            {
                CustomerProvidedKey = customerProvidedKey,
            };

            //Reupload Blob with Server Side Encryption
            blobClient = new BlobClient(connectionString, containerNameString, fileNameString.Replace(".txt", "CPK.txt"), blobClientOptions);
            using FileStream uploadFileStream3 = File.OpenRead("./data/exampleDownload.txt");
            blobClient.Upload(uploadFileStream3, true);
            uploadFileStream3.Close();




        }


        private static void CleanUp()
        {
            //Delete files in the Data folder
            string[] filePaths = Directory.GetFiles("./data/");
            Console.WriteLine("Deleting local source and downloaded files");
            foreach (string filePathString in filePaths)
            {
                File.Delete(filePathString);
            }

            if (Directory.Exists("./data/"))
            {
                Directory.Delete("./data/");
            }


        }
    }

}