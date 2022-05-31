using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Storage.Blobs.EncryptionMigration
{
    internal class CustomerProvidedKeyUploader : IEncryptionUploader<CustomerProvidedKey>
    {
        private readonly StorageTransferOptions _transferOptions;

        public CustomerProvidedKeyUploader(StorageTransferOptions? transferOptions = default)
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
            CustomerProvidedKey customerProvidedKey,
            IProgress<long> progressHandler,
            CancellationToken cancellation)
        {
            await blob.WithCustomerProvidedKey(customerProvidedKey).UploadAsync(
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
