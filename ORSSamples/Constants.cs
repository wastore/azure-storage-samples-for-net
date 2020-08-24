using System.Configuration;

namespace ORS
{
    class Constants
    {
        // Parse config file
        public static string SourceConnectionString = ConfigurationManager.AppSettings["SourceConnectionString"];
        public static string DestConnectionString = ConfigurationManager.AppSettings["DestConnectionString"];
        public static string SourceContainerName = ConfigurationManager.AppSettings["SourceContainerName"];
        public static string DestContainerName = ConfigurationManager.AppSettings["DestContainerName"];
        public static string BlobName = ConfigurationManager.AppSettings["BlobName"];

        // Constants that can be changed
        public const string BlobContent1 = "Hello World!";
        public const string BlobContent2 = "Lorem ipsum";
        public const int NumberBlobs = 1000;
        public const int TimeInterval = 60000;
    }
}
