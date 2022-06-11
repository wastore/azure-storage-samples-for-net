using Azure.Storage.Blobs.EncryptionMigration.Models;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Storage.Blobs.EncryptionMigration
{
    public class DataMigrator
    {
        private readonly ClientSideEncryptionDownloader _downloader;
        private readonly IEncryptionUploader _uploader;

        public DataMigrator(ClientSideEncryptionDownloader downloader, IEncryptionUploader uploader)
        {
            _downloader = downloader;
            _uploader = uploader;
        }

        /// <summary>
        /// Attempts to migrate a blob off client-side encryption v1, if it's already encrypted that way.
        /// </summary>
        /// <param name="blob">
        /// Blob to migrate if encrypted.
        /// Client should not be preconfigured with any encryption options.
        /// </param>
        /// <param name="args">
        /// Arguments for upload, specific to the chosen new encryption solution.
        /// </param>
        /// <param name="progressHandler">
        /// Progress handler for blob transfer.
        /// </param>
        /// <param name="cancellationToken">
        /// Cancellation token.
        /// </param>
        /// <returns>
        /// True if the blob was migrated. False otherwise.
        /// </returns>
        public async Task<bool> TryMigrateClientSideEncryptedV1Blob(
            BlobClient blob,
            IProgress<(BlobMigrationState State, long BytesTransfered)> progressHandler,
            CancellationToken cancellationToken)
        {
            BlobProperties properties = await blob.GetPropertiesAsync();

            string keyId, keyWrapAlgorithm;
            if (!_downloader.IsClientSideEncryptedV1(properties.Metadata, out keyId, out keyWrapAlgorithm))
            {
                return false;
            }

            // downloading to memory for sample purposes
            var plaintextHolder = new MemoryStream();
            await _downloader.DownloadV1ClientSideEncryptedBlobToStreamAsync(
                blob,
                plaintextHolder,
                new Progress<long>(bytesDownloaded => progressHandler?.Report((BlobMigrationState.Downloading, bytesDownloaded))),
                cancellationToken);

            // if any tags present, need to fetch separately
            IDictionary<string, string> tags = default;
            if (properties.TagCount > 0)
            {
                tags = (await blob.GetTagsAsync(cancellationToken: cancellationToken)).Value.Tags;
            }

            // important! wipe old encryption metadata
            properties.Metadata.Remove("encryptiondata");

            await _uploader.UploadBlobWithEncryptionAsync(
                blob,
                plaintextHolder,
                headers: new BlobHttpHeaders
                {
                    CacheControl = properties.CacheControl,
                    ContentDisposition = properties.ContentDisposition,
                    ContentEncoding = properties.ContentEncoding,
                    ContentHash = properties.ContentHash,
                    ContentLanguage = properties.ContentLanguage,
                    ContentType = properties.ContentType,
                },
                properties.Metadata,
                tags,
                keyId,
                keyWrapAlgorithm,
                new Progress<long>(bytesUploaded => progressHandler?.Report((BlobMigrationState.Uploading, bytesUploaded))),
                cancellationToken);

            progressHandler?.Report((BlobMigrationState.Complete, default));

            return true;
        }

        /// <summary>
        /// Migrates all applicable blobs listable by <see cref="BlobContainerClient.GetBlobsByHierarchyAsync"/>.
        /// 
        /// For more information on hierarchical listing, see <see cref="BlobContainerClient.GetBlobsByHierarchyAsync"/>.
        /// </summary>
        /// <param name="container">
        /// Container to list from.
        /// Client should not be preconfigured with any encryption options.
        /// </param>
        /// <param name="prefix">
        /// Optional prefix to list with. Otherwise, list whole container.
        /// </param>
        /// <param name="args">
        /// Arguments for upload, specific to the chosen new encryption solution.
        /// </param>
        /// <param name="progressHandler">
        /// Progress handler for prefix enumeration progress and blob transfer.
        /// </param>
        /// <param name="cancellationToken">
        /// Cancellation token.
        /// </param>
        /// <returns>
        /// Total number of blobs migrated and total number ignored (not clientside v1 encrypted, not found, etc.)
        /// </returns>
        public async Task<(long BlobsMigrated, long BlobsIgnored)> MigrateBlobsAsync(
            BlobContainerClient container,
            string prefix,
            IProgress<(long BlobsMigrated, long BlobsIgnored, BlobMigrationState CurrentBlobState, long CurrentBlobBytesTransferred)> progressHandler,
            CancellationToken cancellationToken)
        {
            long blobsMigrated = 0;
            long blobsIgnored = 0;

            // async enumeration requires C# 8 or greater
            await foreach (var blobItem in container.GetBlobsAsync(
                prefix: prefix,
                traits: BlobTraits.Metadata, // fetch blob metadata on list
                cancellationToken: cancellationToken))
            {
                // Check metadata from listing to see if blob should be migrated.
                // Blob may have been altered or moved since the list operation. Migrate function will recheck blob properties.
                if (_downloader.IsClientSideEncryptedV1(blobItem.Metadata) &&
                    await TryMigrateClientSideEncryptedV1Blob(
                        container.GetBlobClient(blobItem.Name),
                        new Progress<(BlobMigrationState State, long BytesTransfered)>(result => progressHandler?.Report((blobsMigrated, blobsIgnored, result.State, result.BytesTransfered))),
                        cancellationToken)) 
                {
                    blobsMigrated++;
                }
                else
                {
                    blobsIgnored++;
                }
            }

            return (blobsMigrated, blobsIgnored);
        }
    }
}
