using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage;
using Azure.Core;
using Azure.Security.KeyVault.Keys;
using Azure.Identity;
using System;
using System.Text;
using System.IO;
using System.Configuration;
using System.Diagnostics;

namespace LocalKeyClientEncryption
{
    class Program
    {
        private static void SetupForExample(
            BlobServiceClient blobService, 
            string containerNameString, 
            string fileNameString, 
            string filePathString, 
            ClientSideEncryptionOptions clientSideOption)
        {
            //Create example Container and Blob, upload blob as a txt file containing "Hello World!"
            BlobContainerClient containerClient = blobService.CreateBlobContainer(containerNameString);

            //Create BlobClient with Client Side Encryption Options to upload client side encrypted data
            BlobClient blobClient = containerClient.GetBlobClient(fileNameString).WithClientSideEncryptionOptions(clientSideOption);
            File.WriteAllText(filePathString, "Hello World!");

            using FileStream uploadFileStream = File.OpenRead(filePathString);
            blobClient.Upload(uploadFileStream, true);
            uploadFileStream.Close();
            Console.WriteLine("Uploaded to Blob storage as blob: \n\t {0}\n", blobClient.Uri);
        }

        private static void CreateEncryptionScopeMMK(string encryptionScopeName)
        {
            //Runs Azure CLI script that creates an encryption scope that uses Microsoft managed keys
            var config = ConfigurationManager.AppSettings;
            Process process = Process.Start("CMD.exe", "/C az storage account encryption-scope create --name " + encryptionScopeName
                + "MMK -s Microsoft.Storage --account-name " + config["storageAccount"] + " -g " + config["resourceGroup"] + " --subscription " + config["subscriptionId"]);
            process.WaitForExit();
        }
        private static void CreateEncryptionScopeCMK(string encryptionScopeName, KeyVaultKey keyVaultKey)
        {
            //Runs Azure CLI script that creates an encryption scope that uses customer managed keys with the specified Azure Key Vault key
            var config = ConfigurationManager.AppSettings;
            Process process = Process.Start("CMD.exe", "/C az storage account encryption-scope create --name "
                + encryptionScopeName + "CMK -s Microsoft.KeyVault -u " + keyVaultKey.Id.ToString() + " --account-name " + config["storageAccount"] + " -g " + config["resourceGroup"] + " --subscription " + config["subscriptionId"]);
            process.WaitForExit();
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

        private static void CSEtoSSE(
            string connectionString, 
            string containerNameString, 
            string fileNameString, 
            string filePathString, 
            ClientSideEncryptionOptions clientSideOption, 
            string encryptionScopeNameString, 
            string keyVaultName, 
            string keyVaultKeyName,
            TokenCredential credential,
            byte[] keyBytes)
        {
            //Download and decrypt Client Side Encrypted blob using BlobClient with Client Side Encryption Options
            string downloadFilePath = filePathString.Replace(".txt", "Download.txt");
            BlobClient blobClient = new BlobClient(
                connectionString, 
                containerNameString, 
                fileNameString).WithClientSideEncryptionOptions(clientSideOption);
            BlobDownloadInfo download = blobClient.Download();

            Console.WriteLine("\nDownloading blob to \n\t{0}\n", downloadFilePath);

            using (FileStream downloadFileStream = File.OpenWrite(downloadFilePath))
            {
                download.Content.CopyTo(downloadFileStream);
                downloadFileStream.Close();
            }

            //MICROSOFT MANAGED KEY SERVER SIDE ENCRYPTION
            //Create Encryption Scope for Microsoft Managed Key Server Side Encryption
            CreateEncryptionScopeMMK(encryptionScopeNameString);

            //Set Blob Client Options with the created Encryption Scope
            BlobClientOptions blobClientOptions = new BlobClientOptions()
            {
                EncryptionScope = encryptionScopeNameString + "MMK"
            };

            //Reupload Blob with Server Side Encryption
            blobClient = new BlobClient(
                connectionString, 
                containerNameString, 
                fileNameString.Replace(".txt", "MMK.txt"), 
                blobClientOptions);
            using FileStream uploadFileStream = File.OpenRead(downloadFilePath);
            blobClient.Upload(uploadFileStream, true);
            uploadFileStream.Close();

            //CUSTOMER MANAGED KEY SERVER SIDE ENCRYPTION
            //Create Key in Azure Key Vault for Customer Managed Key Server Side Encryption             
            KeyVaultKey keyVaultKey = CreateKeyVaultKey(keyVaultName, keyVaultKeyName, credential);

            //Create Encryption Scope for Customer Managed Key Server Side Encryption
            CreateEncryptionScopeCMK(encryptionScopeNameString, keyVaultKey);

            //Set Blob Client Options with the created Encryption Scope
            blobClientOptions = new BlobClientOptions()
            {
                EncryptionScope = encryptionScopeNameString + "CMK"
            };

            //Reupload Blob with Server Side Encryption
            blobClient = new BlobClient(
                connectionString, 
                containerNameString, 
                fileNameString.Replace(".txt", "CMK.txt"), 
                blobClientOptions);
            using FileStream uploadFileStream2 = File.OpenRead(downloadFilePath);
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
            blobClient = new BlobClient(
                connectionString, 
                containerNameString, 
                fileNameString.Replace(".txt", "CPK.txt"), 
                blobClientOptions);
            using FileStream uploadFileStream3 = File.OpenRead(downloadFilePath);
            blobClient.Upload(uploadFileStream3, true);
            uploadFileStream3.Close();
        }

        private static void CleanUp(string path)
        {
            //Delete files in the Data folder          
            Directory.Delete(path,true);
        }

        /*
        * Program uploads an example client side encrypted blob, and migrates it to server side encryption using
        * Microsoft Managed Keys, Customer Managed Keys, and Customer Provided Keys
        *
        * NOTE: This program requires the following to be stored in the App.Config file:
        * Azure Active Directory Tenant ID - tenantId
        * Service Principal Application ID - clientId
        * Service Principal Password - clientSecret
        * Azure Subscription ID - subscriptionId
        * Resource Group Name - resourceGroup
        * Storage Account Name - storageAccount
        * Storage Account Connection String- connectionString
        * Key Vault Name - keyVaultName
        * Key Wrap Algorithm for Client Side Encryption - keyWrapAlgorithm
        * Customer Provided Key for Client Side Encryption - clientSideCustomerProvidedKey        
        *  
        *  NOTE: This program uses names from Constants.cs, which should be edited as needed
        */
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
            //Get bytes for customer provided key
            byte[] localKeyBytes = ASCIIEncoding.UTF8.GetBytes(Constants.customerProvidedKey);

            //File Path for local file used to upload and reupload
            string localPath = "./data" + Guid.NewGuid().ToString() + "/";
            Directory.CreateDirectory(localPath);
            string localFilePath = Path.Combine(localPath, Constants.fileName);

            //Creating Key Encryption Key object for Client Side Encryption
            SampleKeyEncryptionKey keyEncryption = new SampleKeyEncryptionKey(config["clientSideCustomerProvidedKey"]);

            //Set up Client Side Encryption Options used for Client Side Encryption
            ClientSideEncryptionOptions clientSideOptions = new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V1_0)
            {
                KeyEncryptionKey = keyEncryption,
                KeyWrapAlgorithm = config["keyWrapAlgorithm"]
            };

            //Create Blob Service Client
            BlobServiceClient blobServiceClient = new BlobServiceClient(config["connectionString"]);

            //Run Setup Function that creates and example container and blob
            SetupForExample(
                blobServiceClient, 
                Constants.containerName, 
                Constants.fileName, 
                localFilePath, 
                clientSideOptions);

            //Convert Client Side Encryption Blob to Server Side Encrytion with Microsoft Managed Keys, Customer Managed Keys, and Customer Provided Keys
            CSEtoSSE(
                config["connectionString"],
                Constants.containerName,
                Constants.fileName, 
                localFilePath, 
                clientSideOptions,
                Constants.encryptionScopeName, 
                config["keyVaultName"],
                Constants.keyVaultKeyName, 
                credential, 
                localKeyBytes);

            //Delete downloaded files
            CleanUp(localPath);

            Console.WriteLine("Completed each type of encryption");
        }
    }
}