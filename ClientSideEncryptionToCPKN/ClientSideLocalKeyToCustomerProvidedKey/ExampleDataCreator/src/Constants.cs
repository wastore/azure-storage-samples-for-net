using System.Configuration;

namespace ExampleSetup
{
    class Constants
    {
        //Parse config file values
        public static string ConnectionString = ConfigurationManager.AppSettings["connectionString"];

        //Used only in SetupForExample method to create sample blob
        public const string ContainerName = "clientsidelocalkeytocustomerprovidedkeysample";
        public const string BlobName = "blobExample.txt";
        public const string ClientSideCustomerProvidedKey = "fEy$2HmYscaJfvS5@43hMzreFhY6juD2";
        public const string KeyWrapAlgorithm = "ExampleAlgorithm";
        public const string SamplePath = "./src/";
    }
}
