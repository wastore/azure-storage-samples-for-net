using System.Configuration;

namespace keyVaultClientSideToCustomerManagedServerSide
{
    public class Constants
    {
        //Parse config file values
        public static string tenantId = ConfigurationManager.AppSettings["tenantId"];
        public static string clientId = ConfigurationManager.AppSettings["clientId"];
        public static string clientSecret = ConfigurationManager.AppSettings["clientSecret"];
        public static string connectionString = ConfigurationManager.AppSettings["connectionString"];
        public static string clientSideKeyVaultKeyUri = ConfigurationManager.AppSettings["clientSideKeyVaultKeyUri"];
        public static string keyWrapAlgorithm = ConfigurationManager.AppSettings["keyWrapAlgorithm"];
        public static string containerName = ConfigurationManager.AppSettings["containerName"];
        public static string blobName = ConfigurationManager.AppSettings["blobName"];
        public static string blobNameAfterMigration = ConfigurationManager.AppSettings["blobNameAfterMigration"];
        public static string encryptionScopeName = ConfigurationManager.AppSettings["encryptionScopeName"];
    }
}
