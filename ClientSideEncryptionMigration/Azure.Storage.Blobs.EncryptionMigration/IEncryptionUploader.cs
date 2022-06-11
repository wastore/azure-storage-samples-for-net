using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Storage.Blobs.EncryptionMigration
{
    public interface IEncryptionUploader
    {
        Task UploadBlobWithEncryptionAsync(
            BlobClient blob,
            Stream plaintext,
            BlobHttpHeaders headers,
            IDictionary<string, string> metadata,
            IDictionary<string, string> tags,
            string previousKeyId,
            string previousKeyWrapAlgorithm,
            IProgress<long> progressHandler,
            CancellationToken cancellationToken);
    }
}
