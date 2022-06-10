using Azure.Core.Cryptography;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Storage.Blobs.EncryptionMigration
{
    public class ClientsideEncryptionV2Uploader : IEncryptionUploader<ClientsideEncryptionV2Uploader.UploadOptions>
    {
        public class UploadOptions
        {
            public IKeyEncryptionKey KeyEncryptionKeyOverride { get; set; }
            public string KeyWrapAlgorithmOverride { get; set; }
        }

        private readonly IKeyEncryptionKeyResolver _keyResolver;
        private readonly StorageTransferOptions _transferOptions;

        public ClientsideEncryptionV2Uploader(StorageTransferOptions transferOptions, IKeyEncryptionKeyResolver keyResolver)
        {
            _transferOptions = transferOptions;
            _keyResolver = keyResolver;
        }

        public async Task UploadBlobWithEncryptionAsync(
            BlobClient blob,
            Stream plaintextToEncrypt,
            BlobHttpHeaders headers,
            IDictionary<string, string> metadata,
            IDictionary<string, string> tags,
            string previousKeyId,
            string previousKeyWrapAlgorithm,
            UploadOptions args,
            IProgress<long> progressHandler,
            CancellationToken cancellationToken)
        {
            await blob.WithClientSideEncryptionOptions(new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V2_0)
            {
                KeyEncryptionKey = args.KeyEncryptionKeyOverride ?? await _keyResolver.ResolveAsync(previousKeyId),
                KeyWrapAlgorithm = args.KeyWrapAlgorithmOverride ?? previousKeyWrapAlgorithm
            }).UploadAsync(
                plaintextToEncrypt,
                new BlobUploadOptions
                {
                    TransferOptions = _transferOptions,
                    HttpHeaders = headers,
                    Metadata = metadata,
                    Tags = tags,
                    ProgressHandler = progressHandler,
                },
                cancellationToken);
        }
    }
}
