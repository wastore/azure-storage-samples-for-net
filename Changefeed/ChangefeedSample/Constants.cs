using System;
using System.Collections.Generic;
using System.Text;

namespace ChangefeedSample
{
    public static class Constants
    {
        public const string connectionString = "CONNECTION_STRING";
        public const string containerName = "test-changefeed-container";
        public const string eventType = "BlobCreated";
        public const string blobName = "exampleblob0";
    }
}
