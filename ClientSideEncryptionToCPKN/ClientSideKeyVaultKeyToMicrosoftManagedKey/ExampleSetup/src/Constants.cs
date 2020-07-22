using System.Configuration;

namespace ExampleSetup
{
    public class Constants
    {
        //Parse config file values
        public static string tenantId = ConfigurationManager.AppSettings["tenantId"];
        public static string clientId = ConfigurationManager.AppSettings["clientId"];
        public static string clientSecret = ConfigurationManager.AppSettings["clientSecret"];
        public static string subscriptionId = ConfigurationManager.AppSettings["subscriptionId"];
        public static string resourceGroup = ConfigurationManager.AppSettings["resourceGroup"];
        public static string storageAccount = ConfigurationManager.AppSettings["storageAccount"];
        public static string connectionString = ConfigurationManager.AppSettings["connectionString"];
        public static string clientSideKeyVaultKeyUri = ConfigurationManager.AppSettings["clientSideKeyVaultKeyUri"];
        public static string keyWrapAlgorithm = ConfigurationManager.AppSettings["keyWrapAlgorithm"];

        //Used only in SetupForExample method to create sample blob
        public const string containerName = "clientsidelocalkeytocustomerprovidedkeysample";
        public const string blobName = "blobExample.txt";
        public const string encrpytionScopeName = "exampleencryptionscope";
        public const string samplePath = "./src/";
    }
}
