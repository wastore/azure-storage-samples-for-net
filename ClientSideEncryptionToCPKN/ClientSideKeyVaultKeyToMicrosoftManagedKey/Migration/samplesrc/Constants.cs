using System.Configuration;

namespace keyVaultClientSideToMicrosoftManagedServerSide
{
    public class Constants
    {
        //Parse config file values
        public static string TenantId = ConfigurationManager.AppSettings["tenantId"];
        public static string ClientId = ConfigurationManager.AppSettings["clientId"];
        public static string ClientSecret = ConfigurationManager.AppSettings["clientSecret"];
        public static string SubscriptionId = ConfigurationManager.AppSettings["subscriptionId"];
        public static string ResourceGroup = ConfigurationManager.AppSettings["resourceGroup"];
        public static string StorageAccount = ConfigurationManager.AppSettings["storageAccount"];
        public static string ConnectionString = ConfigurationManager.AppSettings["connectionString"];
        public static string ClientSideKeyVaultKeyUri = ConfigurationManager.AppSettings["clientSideKeyVaultKeyUri"];
        public static string KeyWrapAlgorithm = ConfigurationManager.AppSettings["keyWrapAlgorithm"];
        public static string ContainerName = ConfigurationManager.AppSettings["containerName"];
        public static string BlobName = ConfigurationManager.AppSettings["blobName"];
        public static string BlobNameAfterMigration = ConfigurationManager.AppSettings["blobNameAfterMigration"];
        public static string EncryptionScopeName = ConfigurationManager.AppSettings["encryptionScopeName"];
    }
}
