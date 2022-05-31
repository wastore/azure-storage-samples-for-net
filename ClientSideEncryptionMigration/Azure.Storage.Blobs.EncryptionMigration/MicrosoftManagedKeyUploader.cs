using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Storage.Blobs.EncryptionMigration
{
    internal class MicrosoftManagedKeyUploader : IEncryptionUploader<string>
    {
        private readonly StorageTransferOptions _transferOptions;

        public MicrosoftManagedKeyUploader(StorageTransferOptions? transferOptions = default)
        {
            _transferOptions = transferOptions ?? new StorageTransferOptions
            {
                InitialTransferSize = 4 * Constants.MB,
                MaximumTransferSize = 4 * Constants.MB,
                MaximumConcurrency = 8
            };
        }

        public async Task UploadBlobWithEncryptionAsync(
            BlobClient blob,
            Stream plaintext,
            BlobHttpHeaders headers,
            IDictionary<string, string> metadata,
            IDictionary<string, string> tags,
            string previousKeyId,
            string previousKeyWrapAlgorithm,
            string encryptionScope,
            IProgress<long> progressHandler,
            CancellationToken cancellation)
        {
            await blob.WithEncryptionScope(encryptionScope).UploadAsync(
                plaintext,
                new BlobUploadOptions
                {
                    HttpHeaders = headers,
                    Metadata = metadata,
                    Tags = tags,
                    TransferOptions = _transferOptions,
                    ProgressHandler = progressHandler
                });
        }
    }
}
