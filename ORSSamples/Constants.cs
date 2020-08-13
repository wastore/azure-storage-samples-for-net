using System.Configuration;

namespace ORS
{
    class Constants
    {
        // Parse config file
        public static string sourceConnectionString = ConfigurationManager.AppSettings["sourceConnectionString"];
        public static string destConnectionString = ConfigurationManager.AppSettings["destConnectionString"];
        public static string sourceContainerName = ConfigurationManager.AppSettings["sourceContainerName"];
        public static string destContainerName = ConfigurationManager.AppSettings["destContainerName"];
        public static string blobName = ConfigurationManager.AppSettings["blobName"];

        // Constants that can be changed
        public const string blobContent1 = "Hello World!";
        public const string blobContent2 = "Lorem ipsum";
        public const int numberBlobs = 1000;
        public const int timeInterval = 60000;
    }
}
