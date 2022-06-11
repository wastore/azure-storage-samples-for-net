using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Storage.Blobs.EncryptionMigration
{
    internal class ManagedKeyUploader : IEncryptionUploader
    {
        private readonly string _encryptionScope;
        private readonly StorageTransferOptions _transferOptions;

        public ManagedKeyUploader(string encryptionScope, StorageTransferOptions? transferOptions = default)
        {
            _encryptionScope = encryptionScope;
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
            IProgress<long> progressHandler,
            CancellationToken cancellationToken)
        {
            await blob.WithEncryptionScope(_encryptionScope).UploadAsync(
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
