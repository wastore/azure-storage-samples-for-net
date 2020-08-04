using System.Configuration;

namespace ExampleSetup
{
    class Constants
    {
        //Parse config file values
        public static string SubscriptionId = ConfigurationManager.AppSettings["subscriptionId"];
        public static string ResourceGroup = ConfigurationManager.AppSettings["resourceGroup"];
        public static string StorageAccount = ConfigurationManager.AppSettings["storageAccount"];
        public static string ConnectionString = ConfigurationManager.AppSettings["connectionString"];


        //Used only in SetupForExample method to create sample blob
        public const string ContainerName = "clientsidelocalkeytomicrosoftmanagedkeysample";
        public const string BlobName = "blobExample.txt";
        public const string EncryptionScopeName = "exampleencryptionscope";
        public const string ClientSideCustomerProvidedKey = "fEy$2HmYscaJfvS5@43hMzreFhY6juD2";
        public const string KeyWrapAlgorithm = "ExampleAlgorithm";
        public const string SamplePath = "./src/";
    }
}
