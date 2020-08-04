using System.Configuration;

namespace localKeyClientSideToCustomerProvidedServerSide
{
    class Constants
    {
        //Parse config file values
        public static string ConnectionString = ConfigurationManager.AppSettings["connectionString"];
        public static string KeyWrapAlgorithm = ConfigurationManager.AppSettings["keyWrapAlgorithm"];
        public static string ClientSideCustomerProvidedKey = ConfigurationManager.AppSettings["clientSideCustomerProvidedKey"];
        public static string ContainerName = ConfigurationManager.AppSettings["containerName"];
        public static string BlobName = ConfigurationManager.AppSettings["blobName"];
        public static string BlobNameAfterMigration = ConfigurationManager.AppSettings["blobNameAfterMigration"];
        public static string ServerSideCustomerProvidedKey = ConfigurationManager.AppSettings["serverSideCustomerProvidedKey"];
    }
}
