using System.Configuration;

namespace ExampleSetup
{
    public class Constants
    {
        //Parse config file values
        public static string tenantId = ConfigurationManager.AppSettings["tenantId"];
        public static string clientId = ConfigurationManager.AppSettings["clientId"];
        public static string clientSecret = ConfigurationManager.AppSettings["clientSecret"];
        public static string connectionString = ConfigurationManager.AppSettings["connectionString"];
        public static string keyVaultKeyUri = ConfigurationManager.AppSettings["keyVaultKeyUri"];
        public static string keyWrapAlgorithm = ConfigurationManager.AppSettings["keyWrapAlgorithm"];

        //Used only in SetupForExample method to create sample blob
        public const string containerName = "clientsidelocalkeytocustomerprovidedkeysample";
        public const string blobName = "blobExample.txt";
        public const string samplePath = "./src/";
    }
}
