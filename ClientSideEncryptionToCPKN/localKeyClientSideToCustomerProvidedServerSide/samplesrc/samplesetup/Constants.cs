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

        //Edit the Following as Needed
        //Program Creates the Following using the Provided Names
        public const string containerName = "example";
        public const string fileName = "example.txt";
        public const string sampleFileContent = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cras dolor purus, interdum in turpis ut, ultrices ornare augue. Donec mollis varius sem, et mattis ex gravida eget. Duis nibh magna, ultrices a nisi quis, pretium tristique ligula. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Vestibulum in dui arcu. Nunc at orci volutpat, elementum magna eget, pellentesque sem. Etiam id placerat nibh. Vestibulum varius at elit ut mattis.  Suspendisse ipsum sem, placerat id blandit ac, cursus eget purus. Vestibulum pretium ante eu augue aliquam, ultrices fermentum nibh condimentum. Pellentesque pulvinar feugiat augue vel accumsan. Nulla imperdiet viverra nibh quis rhoncus. Nunc tincidunt sollicitudin urna, eu efficitur elit gravida ut. Quisque eget urna convallis, commodo diam eu, pretium erat. Nullam quis magna a dolor ullamcorper malesuada. Donec bibendum sem lectus, sit amet faucibus nisi sodales eget. Integer lobortis lacus et volutpat dignissim. Suspendisse cras amet.";
        public const string customerProvidedKey = "dfD3Jb#6htqfpoj@gGpomDAv21035%21";   //Key used for Customer Provided Key Server Side Encryption
    }
}
