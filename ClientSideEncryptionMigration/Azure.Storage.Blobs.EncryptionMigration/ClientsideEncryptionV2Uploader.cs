using Azure.Core.Cryptography;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Storage.Blobs.EncryptionMigration
{
    public class ClientsideEncryptionV2Uploader : IEncryptionUploader
    {
        private readonly IKeyEncryptionKeyResolver _keyResolver;
        private readonly StorageTransferOptions _transferOptions;

        public ClientsideEncryptionV2Uploader(IKeyEncryptionKeyResolver keyResolver, StorageTransferOptions transferOptions = default)
        {
            _keyResolver = keyResolver;
            _transferOptions = transferOptions;
        }

        public async Task UploadBlobWithEncryptionAsync(
            BlobClient blob,
            Stream plaintextToEncrypt,
            BlobHttpHeaders headers,
            IDictionary<string, string> metadata,
            IDictionary<string, string> tags,
            string previousKeyId,
            string previousKeyWrapAlgorithm,
            IProgress<long> progressHandler,
            CancellationToken cancellationToken)
        {
            await blob.WithClientSideEncryptionOptions(new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V2_0)
            {
                KeyEncryptionKey = await _keyResolver.ResolveAsync(previousKeyId),
                KeyWrapAlgorithm = previousKeyWrapAlgorithm
            }).UploadAsync(
                plaintextToEncrypt,
                new BlobUploadOptions
                {
                    HttpHeaders = headers,
                    Metadata = metadata,
                    Tags = tags,
                    TransferOptions = _transferOptions,
                    ProgressHandler = progressHandler
                },
                cancellationToken);
        }
    }
}
