using System.Configuration;

namespace ExampleSetup
{
    class Constants
    {
        //Parse config file values
        public static string subscriptionId = ConfigurationManager.AppSettings["subscriptionId"];
        public static string resourceGroup = ConfigurationManager.AppSettings["resourceGroup"];
        public static string storageAccount = ConfigurationManager.AppSettings["storageAccount"];
        public static string connectionString = ConfigurationManager.AppSettings["connectionString"];


        //Used only in SetupForExample method to create sample blob
        public const string containerName = "clientsidelocalkeytomicrosoftmanagedkeysample";
        public const string blobName = "blobExample.txt";
        public const string encryptionScopeName = "exampleencryptionscope";
        public const string clientSideCustomerProvidedKey = "fEy$2HmYscaJfvS5@43hMzreFhY6juD2";
        public const string keyWrapAlgorithm = "ExampleAlgorithm";
        public const string samplePath = "./src/";
    }
}
