using System.Configuration;

namespace keyVaultClientSideToMicrosoftManagedServerSide
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
        public static string keyVaultKeyUri = ConfigurationManager.AppSettings["keyVaultKeyUri"];
        public static string keyWrapAlgorithm = ConfigurationManager.AppSettings["keyWrapAlgorithm"];

        //Used only in SetupForExample method to get sample blob
        public const string samplePath = "./samplesrc/samplesetup/";

        //Edit the Following as Needed
        //Program Creates the Following using the Provided Names
        public const string containerName = "clientsidekeyvaultkeytomicrosoftmanagedkeysample";
        public const string fileName = "blobExample.txt";
        public const string encryptionScopeName = "myencryption";
    }
}
