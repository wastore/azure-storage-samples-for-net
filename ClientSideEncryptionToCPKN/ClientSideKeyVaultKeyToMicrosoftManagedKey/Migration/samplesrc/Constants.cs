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
        public static string containerName = ConfigurationManager.AppSettings["containerName"];
        public static string blobName = ConfigurationManager.AppSettings["blobName"];
        public static string encryptionScopeName = ConfigurationManager.AppSettings["encryptionScopeName"];
    }
}
