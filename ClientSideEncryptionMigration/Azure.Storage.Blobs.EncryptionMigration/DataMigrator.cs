using Azure.Storage.Blobs.EncryptionMigration.Models;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Storage.Blobs.EncryptionMigration
{
    public class DataMigrator<TUploadArgs>
    {
        private readonly ClientSideEncryptionDownloader _downloader;
        private readonly IEncryptionUploader<TUploadArgs> _uploader;

        public DataMigrator(ClientSideEncryptionDownloader downloader, IEncryptionUploader<TUploadArgs> uploader)
        {
            _downloader = downloader;
            _uploader = uploader;
        }

        /// <summary>
        /// Attempts to migrate a blob off client-side encryption v1, if it's already encrypted that way.
        /// </summary>
        /// <param name="blob">Blob to migrate if encrypted.</param>
        /// <param name="args">Arguments for upload, specific to the chosen new encryption solution.</param>
        /// <param name="progressHandler">Progress handler for blob transfer.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the blob was migrated. False if the blob does not exist or is not client-side encrypted.</returns>
        public async Task<bool> MigrateBlobIfClientsideV1Encrypted(BlobClient blob, TUploadArgs args, IProgress<(BlobMigrationState State, long BytesTransfered)> progressHandler, CancellationToken cancellationToken)
        {
            var plaintextHolder = new MemoryStream();
            (BlobProperties Properties, string KeyId, string KeyWrapAlgorithm) blobToMigrate = await _downloader.DownloadV1ClientSideEncryptedBlobOrDefaultAsync(
                blob,
                plaintextHolder,
                new Progress<long>(bytesDownloaded => progressHandler?.Report((BlobMigrationState.Downloading, bytesDownloaded))),
                cancellationToken);

            if (blobToMigrate == default)
            {
                return false;
            }

            // if any tags present, fetch separately
            IDictionary<string, string> tags = default;
            if (blobToMigrate.Properties.TagCount > 0)
            {
                tags = (await blob.GetTagsAsync()).Value.Tags;
            }

            // wipe old encryption metadata
            blobToMigrate.Properties.Metadata.Remove("encryptiondata");

            await _uploader.UploadBlobWithEncryptionAsync(
                blob,
                plaintextHolder,
                headers: default,
                blobToMigrate.Properties.Metadata,
                tags,
                blobToMigrate.KeyId,
                blobToMigrate.KeyWrapAlgorithm,
                args,
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
        /// <param name="container">Container to list from.</param>
        /// <param name="prefix">Blob prefix to list under.</param>
        /// <param name="delimiter">Delimiter to separate path segments by.</param>
        /// <param name="recursive">Whether to recursively enumerate more prefixes that are returned.</param>
        /// <param name="args">Arguments for upload, specific to the chosen new encryption solution.</param>
        /// <param name="progressHandler">Progress handler for prefix enumeration progress and blob transfer.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Total number of blobs migrated and total number ignored (not encrypted, not fount, etc.)</returns>
        public async Task<(long BlobsMigrated, long BlobsIgnored)> MigrateBlobPrefixAsync(
            BlobContainerClient container,
            string prefix,
            string delimiter,
            bool recursive,
            TUploadArgs args,
            IProgress<(long BlobsMigrated, long BlobsIgnored, BlobMigrationState CurrentBlobState, long CurrentBlobBytesTransferred)> progressHandler,
            CancellationToken cancellationToken)
        {
            long blobsMigrated = 0;
            long blobsIgnored = 0;

            // async enumeration requires C# 8 or greater
            await foreach (var item in container.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: delimiter))
            {
                if (item.IsBlob)
                {
                    if (await MigrateBlobIfClientsideV1Encrypted(
                        container.GetBlobClient(item.Blob.Name),
                        args,
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
                else if (recursive)
                {
                    (long subPrefixMigrated, long subPrefixIgnored) = await MigrateBlobPrefixAsync(
                        container,
                        prefix + item.Prefix,
                        delimiter,
                        recursive,
                        args,
                        new Progress<(long BlobsMigrated, long BlobsIgnored, BlobMigrationState CurrentBlobState, long CurrentBlobBytesTransferred)>(
                            report => progressHandler?.Report((blobsMigrated + report.BlobsMigrated, blobsIgnored + report.BlobsIgnored, report.CurrentBlobState, report.CurrentBlobBytesTransferred))),
                        cancellationToken);

                    blobsMigrated += subPrefixMigrated;
                    blobsIgnored += subPrefixIgnored;
                }
            }

            return (blobsMigrated, blobsIgnored);
        }

        /// <summary>
        /// Migrates all applicable blobs in a given container.
        /// </summary>
        /// <param name="container">Container to migrate.</param>
        /// <param name="args">Arguments for upload, specific to the chosen new encryption solution.</param>
        /// <param name="progressHandler">Progress handler for container enumeration progress and blob transfer.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Total number of blobs migrated.</returns>
        /// <returns></returns>
        public async Task<long> MigrateBlobContainerAsync(
            BlobContainerClient container,
            TUploadArgs args,
            IProgress<(long BlobsMigrated, long BlobsIgnored, BlobMigrationState CurrentBlobState, long CurrentBlobBytesTransferred)> progressHandler,
            CancellationToken cancellationToken)
        {
            long blobsMigrated = 0;
            long blobsIgnored = 0;

            // async enumeration requires C# 8 or greater
            await foreach (var item in container.GetBlobsAsync())
            {
                if (await MigrateBlobIfClientsideV1Encrypted(
                    container.GetBlobClient(item.Name),
                    args,
                    new Progress<(BlobMigrationState State, long BytesTransfered)>(result => progressHandler?.Report((blobsMigrated, blobsIgnored, result.State, result.BytesTransfered))),
                    cancellationToken))
                {
                    blobsMigrated++;
                }
                else
                {
                    blobsIgnored++;
                }
                progressHandler?.Report((blobsMigrated, blobsIgnored, default, default));
            }

            return blobsMigrated;
        }
    }
}
