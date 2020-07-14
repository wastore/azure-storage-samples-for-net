using System.Configuration;

namespace localKeyClientSideToCustomerProvidedServerSide
{
    class Constants
    {
        //Parse config file values
        public static string tenantId = ConfigurationManager.AppSettings["tenantId"];
        public static string clientId = ConfigurationManager.AppSettings["clientId"];
        public static string clientSecret = ConfigurationManager.AppSettings["clientSecret"];
        public static string subscriptionId = ConfigurationManager.AppSettings["subscriptionId"];
        public static string resourceGroup = ConfigurationManager.AppSettings["resourceGroup"];
        public static string storageAccount = ConfigurationManager.AppSettings["storageAccount"];
        public static string connectionString = ConfigurationManager.AppSettings["connectionString"];
        public static string keyVaultName = ConfigurationManager.AppSettings["keyVaultName"];
        public static string keyWrapAlgorithm = ConfigurationManager.AppSettings["keyWrapAlgorithm"];
        public static string clientSideCustomerProvidedKey = ConfigurationManager.AppSettings["clientSideCustomerProvidedKey"];

        //Used only in SetupForExample method to get sample blob
        public const string samplePath = "./samplesrc/samplesetup/";

        //Edit the Following as Needed
        //Program creates the following using the provided strings in SetupForExample method
        //Program gets and uses objects with the following names/values in EncryptWithCustomerProvidedKey method
        public const string containerName = "clientsidelocalkeytocustomerprovidedkeysample";
        public const string fileName = "blobExample.txt";
        public const string customerProvidedKey = "dfD3Jb#6htqfpoj@gGpomDAv21035%21";   //Key used for Customer Provided Key Server Side Encryption
    }
}
