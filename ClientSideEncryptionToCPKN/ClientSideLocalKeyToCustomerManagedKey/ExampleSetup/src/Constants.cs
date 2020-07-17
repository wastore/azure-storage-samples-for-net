using System.Configuration;

namespace ExampleSetup
{
    class Constants
    {
        //Parse config file values
        public static string tenantId = ConfigurationManager.AppSettings["tenantId"];
        public static string clientId = ConfigurationManager.AppSettings["clientId"];
        public static string clientSecret = ConfigurationManager.AppSettings["clientSecret"];
        public static string subscriptionId = ConfigurationManager.AppSettings["subscriptionId"];
        public static string resourceGroup = ConfigurationManager.AppSettings["resourceGroup"];
        public static string storageAccount = ConfigurationManager.AppSettings["storageAccount"];
        public static string connectionString = ConfigurationManager.AppSettings["connectionString"];
        public static string keyVaultName = ConfigurationManager.AppSettings["keyVaultName"];
        
        //Used only in SetupForExample method to create sample blob
        public const string containerName = "clientsidelocalkeytocustomermanagedkeysample";
        public const string blobName = "blobExample.txt";
        public const string encryptionScopeName = "exampleencryptionscope";
        public const string keyVaultKeyName = "exampleKey";
        public const string clientSideCustomerProvidedKey = "fEy$2HmYscaJfvS5@43hMzreFhY6juD2";
        public const string keyWrapAlgorithm = "ExampleAlgorithm";
        public const string samplePath = "./src/";
    }
}
