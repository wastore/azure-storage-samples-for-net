using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Storage.Blobs.EncryptionMigration.Models
{
    public enum BlobMigrationState
    {
        None = 0,
        Downloading = 1,
        Uploading = 2,
        Complete = 3
    }
}
