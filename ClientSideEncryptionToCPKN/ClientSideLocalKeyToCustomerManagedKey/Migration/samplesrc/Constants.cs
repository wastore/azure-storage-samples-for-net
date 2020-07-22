using System.Configuration;

namespace localKeyClientSideToCustomerManagedServerSide
{
    class Constants
    {
        //Parse config file values
        public static string connectionString = ConfigurationManager.AppSettings["connectionString"];
        public static string keyWrapAlgorithm = ConfigurationManager.AppSettings["keyWrapAlgorithm"];
        public static string clientSideCustomerProvidedKey = ConfigurationManager.AppSettings["clientSideCustomerProvidedKey"];
        public static string containerName = ConfigurationManager.AppSettings["containerName"];
        public static string blobName = ConfigurationManager.AppSettings["blobName"];
        public static string blobNameAfterMigration = ConfigurationManager.AppSettings["blobNameAfterMigration"];
        public static string encryptionScopeName = ConfigurationManager.AppSettings["encryptionScopeName"];
    }
}
