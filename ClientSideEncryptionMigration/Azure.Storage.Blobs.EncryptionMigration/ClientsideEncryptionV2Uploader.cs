using Azure.Core.Cryptography;
using Azure.Storage.Blobs.Models;
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

        public ClientsideEncryptionV2Uploader(IKeyEncryptionKeyResolver keyResolver)
        {
            _keyResolver = keyResolver;
        }

        public Task UploadBlobWithEncryptionAsync(
            BlobClient blob, Stream plaintextToEncrypt,
            BlobHttpHeaders headers,
            IDictionary<string, string> metadata,
            IDictionary<string, string> tags,
            string previousKeyId,
            string previousKeyWrapAlgorithm,
            UploadOptions args,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
