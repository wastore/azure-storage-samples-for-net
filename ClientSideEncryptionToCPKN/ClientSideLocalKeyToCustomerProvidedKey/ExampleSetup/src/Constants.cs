using System.Configuration;

namespace ExampleSetup
{
    class Constants
    {
        //Parse config file values
        public static string connectionString = ConfigurationManager.AppSettings["connectionString"];
        public static string keyWrapAlgorithm = ConfigurationManager.AppSettings["keyWrapAlgorithm"];

        //Used only in SetupForExample method to create sample blob
        public const string containerName = "clientsidelocalkeytocustomerprovidedkeysample";
        public const string blobName = "blobExample.txt";
        public const string clientSideCustomerProvidedKey = "fEy$2HmYscaJfvS5@43hMzreFhY6juD2";
        public const string samplePath = "./src/";
    }
}
