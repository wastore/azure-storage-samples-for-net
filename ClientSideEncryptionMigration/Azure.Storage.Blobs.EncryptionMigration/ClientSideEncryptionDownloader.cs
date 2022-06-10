using Azure.Core.Cryptography;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Storage.Blobs.EncryptionMigration
{
    public class ClientSideEncryptionDownloader
    {
        private readonly StorageTransferOptions _transferOptions;
        private readonly ClientSideEncryptionOptions _encryptionOptions;

        /// <summary>
        /// Downloads and decrypts blobs encrypted with client-side encryption v1.0.
        /// </summary>
        /// <param name="transferOptions">Storage transfer options for downloads.</param>
        /// <param name="keyEncryptionKeyResolver">Resolver for encryption keys on download.</param>
        public ClientSideEncryptionDownloader(StorageTransferOptions transferOptions, IKeyEncryptionKeyResolver keyEncryptionKeyResolver)
        {
            _transferOptions = transferOptions;
            _encryptionOptions = new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V1_0)
            {
                KeyResolver = keyEncryptionKeyResolver
            };
        }

        public async Task DownloadV1ClientSideEncryptedBlobToStreamAsync(
            BlobClient blob,
            Stream plaintextDestination,
            IProgress<long> progressHandler,
            CancellationToken cancellationToken)
        {
            await blob.WithClientSideEncryptionOptions(_encryptionOptions).DownloadToAsync(
                plaintextDestination,
                new BlobDownloadToOptions
                {
                    TransferOptions = _transferOptions,
                    ProgressHandler = progressHandler
                },
                cancellationToken);
        }

        public bool IsClientSideEncryptedV1(IDictionary<string, string> blobMetadata)
            => IsClientSideEncryptedV1(blobMetadata, out string _, out string _);

        public bool IsClientSideEncryptedV1(IDictionary<string, string> blobMetadata, out string keyId, out string keyWrapAlgorithm)
        {
            // if metadata key not present, clientside encryption not in use
            string encryptionMetadata;
            if (!blobMetadata.TryGetValue("encryptiondata", out encryptionMetadata))
            {
                keyId = null;
                keyWrapAlgorithm = null;
                return false;
            }

            // value must declare version 1.0 for clientside encryption v1
            string clientsideEncryptionVersion = JsonDocument.Parse(encryptionMetadata)
                .RootElement
                .GetProperty("EncryptionAgent")
                .GetProperty("Protocol").GetString();

            (keyId, keyWrapAlgorithm) = GetKeyWrappingInfo(encryptionMetadata);
            return clientsideEncryptionVersion == "1.0";
        }

        private (string KeyId, string KeyWrapAlgorithm) GetKeyWrappingInfo(string rawEncryptionMetadata)
        {
            JsonElement keywrappingInfo = JsonDocument.Parse(rawEncryptionMetadata)
                .RootElement
                .GetProperty("WrappedContentKey");

            return (keywrappingInfo.GetProperty("KeyId").GetString(), keywrappingInfo.GetProperty("Algorithm").GetString());
        }
    }
}
