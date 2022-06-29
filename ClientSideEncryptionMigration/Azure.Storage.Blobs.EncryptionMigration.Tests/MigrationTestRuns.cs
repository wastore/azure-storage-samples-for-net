using Azure.Core.Cryptography;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System.Text.Json;

namespace Azure.Storage.Blobs.EncryptionMigration.Tests
{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

    public class MigrationTestRuns
    {
        private const string _prefix = "foo";
        private const string _subPrefix = "bar";
        
        private readonly IEnumerable<string> _blobNames = new List<string>
        {
            $"encrypted-v1",
            $"encrypted-v2",
            $"plaintext",
            $"{_prefix}/encrypted-v1",
            $"{_prefix}/encrypted-v2",
            $"{_prefix}/plaintext",
            $"{_prefix}/{_subPrefix}/encrypted-v1",
            $"{_prefix}/{_subPrefix}/encrypted-v2",
            $"{_prefix}/{_subPrefix}/plaintext",
        };

        private readonly BlobServiceClient _serviceClient = Clients.GetServiceClient();
        private BlobContainerClient _containerClient = new(new Uri("https://example.com/container"));
        private IKeyEncryptionKey _keyClient = Clients.GetMockIKeyEncryptionKey().Object;

        private static void AssertPlaintext(BlobItem blobItem)
        {
            Assert.That(blobItem.Metadata, Is.Empty);
        }

        private static void AssertV1Encrypted(BlobItem blobItem)
        {
            Assert.That(blobItem.Metadata, Has.Count.EqualTo(1));
            Assert.That(blobItem.Metadata.TryGetValue("encryptiondata", out string? rawEncryptionData), Is.True);
            // was migrated
            string clientSideEncryptionVersion = JsonDocument.Parse(rawEncryptionData)
                .RootElement
                .GetProperty("EncryptionAgent")
                .GetProperty("Protocol").GetString();
            Assert.That(clientSideEncryptionVersion, Is.EqualTo("1.0"));
        }

        private static void AssertV2Encrypted(BlobItem blobItem)
        {
            Assert.That(blobItem.Metadata, Has.Count.EqualTo(1));
            Assert.That(blobItem.Metadata.TryGetValue("encryptiondata", out string? rawEncryptionData), Is.True);
            // was migrated
            string clientSideEncryptionVersion = JsonDocument.Parse(rawEncryptionData)
                .RootElement
                .GetProperty("EncryptionAgent")
                .GetProperty("Protocol").GetString();
            Assert.That(clientSideEncryptionVersion, Is.EqualTo("2.0"));
        }

        [SetUp]
        public async Task SetupAsync()
        {
            _containerClient = _serviceClient.GetBlobContainerClient(Guid.NewGuid().ToString());
            await _containerClient.CreateAsync();

            var random = new Random();
            foreach (string blobName in _blobNames)
            {
                var blobClient = _containerClient.GetBlobClient(blobName);
                if (blobName.Contains("encrypted-v1"))
                {
                    blobClient = blobClient.WithClientSideEncryptionOptions(
                        new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V1_0)
                        {
                            KeyEncryptionKey = _keyClient,
                            KeyWrapAlgorithm = "fizz"
                        });
                }
                else if (blobName.Contains("encrypted-v2"))
                {
                    blobClient = blobClient.WithClientSideEncryptionOptions(
                        new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V2_0)
                        {
                            KeyEncryptionKey = _keyClient,
                            KeyWrapAlgorithm = "fizz"
                        });
                }

                var data = new byte[1024];
                random.NextBytes(data);
                using Stream dataStream = new MemoryStream(data);
                await blobClient.UploadAsync(new MemoryStream(data));
            }
        }

        [TearDown]
        public async Task TearDownAsync()
        {
            await _containerClient.DeleteIfExistsAsync();
        }

        [Test]
        public async Task MigrateContainer()
        {
            // Arrange
            IKeyEncryptionKeyResolver resolver = Clients.GetMockIKeyEncryptionKeyResolver(_keyClient).Object;
            var migrator = new DataMigrator(
                new ClientSideEncryptionDownloader(resolver),
                new ClientsideEncryptionV2Uploader(resolver));

            // Act
            (long blobsMigrated, long blobsIgnored) = await migrator.MigrateBlobsAsync(
                _containerClient,
                prefix: default,
                progressHandler: default,
                cancellationToken: default);
            Assert.Multiple(() =>
            {
                Assert.That(blobsMigrated, Is.EqualTo(3));
                Assert.That(blobsIgnored, Is.EqualTo(6));
            });
            var blobItems = await _containerClient.GetBlobsAsync(BlobTraits.Metadata).ToListAsync();
            foreach (var blobItem in blobItems.Where(b => b.Name.Contains("plaintext")))
            {
                AssertPlaintext(blobItem);
            }
            foreach (var blobItem in blobItems.Where(b => b.Name.Contains("encrypted")))
            {
                AssertV2Encrypted(blobItem);
            }
        }

        [Test]
        public async Task MigratePrefix()
        {
            // Arrange
            IKeyEncryptionKeyResolver resolver = Clients.GetMockIKeyEncryptionKeyResolver(_keyClient).Object;
            var migrator = new DataMigrator(
                new ClientSideEncryptionDownloader(resolver),
                new ClientsideEncryptionV2Uploader(resolver));

            // Act
            (long blobsMigrated, long blobsIgnored) = await migrator.MigrateBlobsAsync(
                _containerClient,
                prefix: _prefix,
                progressHandler: default,
                cancellationToken: default);
            Assert.Multiple(() =>
            {
                Assert.That(blobsMigrated, Is.EqualTo(2));
                Assert.That(blobsIgnored, Is.EqualTo(4));
            });
            var prefixBlobItems = await _containerClient.GetBlobsAsync(BlobTraits.Metadata, prefix: _prefix).ToListAsync();
            Assert.That(prefixBlobItems, Has.Count.EqualTo(6));
            foreach (var blobItem in prefixBlobItems.Where(b => b.Name.Contains("plaintext")))
            {
                AssertPlaintext(blobItem);
            }
            foreach (var blobItem in prefixBlobItems.Where(b => b.Name.Contains("encrypted")))
            {
                AssertV2Encrypted(blobItem);
            }

            var otherBlobItems = await _containerClient.GetBlobsAsync(BlobTraits.Metadata)
                .Where(item => !prefixBlobItems.Select(b => b.Name).Contains(item.Name))
                .ToListAsync();
            Assert.That(otherBlobItems, Has.Count.EqualTo(3));
            foreach (var blobItem in otherBlobItems.Where(b => b.Name.Contains("plaintext")))
            {
                AssertPlaintext(blobItem);
            }
            foreach (var blobItem in otherBlobItems.Where(b => b.Name.Contains("encrypted-v1")))
            {
                AssertV1Encrypted(blobItem);
            }
            foreach (var blobItem in otherBlobItems.Where(b => b.Name.Contains("encrypted-v2")))
            {
                AssertV2Encrypted(blobItem);
            }
        }

        [Test]
        public async Task MigrateSubPrefix()
        {
            // Arrange
            IKeyEncryptionKeyResolver resolver = Clients.GetMockIKeyEncryptionKeyResolver(_keyClient).Object;
            var migrator = new DataMigrator(
                new ClientSideEncryptionDownloader(resolver),
                new ClientsideEncryptionV2Uploader(resolver));

            // Act
            (long blobsMigrated, long blobsIgnored) = await migrator.MigrateBlobsAsync(
                _containerClient,
                prefix: $"{_prefix}/{_subPrefix}",
                progressHandler: default,
                cancellationToken: default);
            Assert.Multiple(() =>
            {
                Assert.That(blobsMigrated, Is.EqualTo(1));
                Assert.That(blobsIgnored, Is.EqualTo(2));
            });
            var subPrefixBlobItems = await _containerClient.GetBlobsAsync(BlobTraits.Metadata, prefix: $"{_prefix}/{_subPrefix}").ToListAsync();
            Assert.That(subPrefixBlobItems, Has.Count.EqualTo(3));
            foreach (var blobItem in subPrefixBlobItems.Where(b => b.Name.Contains("plaintext")))
            {
                AssertPlaintext(blobItem);
            }
            foreach (var blobItem in subPrefixBlobItems.Where(b => b.Name.Contains("encrypted")))
            {
                AssertV2Encrypted(blobItem);
            }

            var otherBlobItems = await _containerClient.GetBlobsAsync(BlobTraits.Metadata)
                .Where(item => !subPrefixBlobItems.Select(b => b.Name).Contains(item.Name))
                .ToListAsync();
            Assert.That(otherBlobItems, Has.Count.EqualTo(6));
            foreach (var blobItem in otherBlobItems.Where(b => b.Name.Contains("plaintext")))
            {
                AssertPlaintext(blobItem);
            }
            foreach (var blobItem in otherBlobItems.Where(b => b.Name.Contains("encrypted-v1")))
            {
                AssertV1Encrypted(blobItem);
            }
            foreach (var blobItem in otherBlobItems.Where(b => b.Name.Contains("encrypted-v2")))
            {
                AssertV2Encrypted(blobItem);
            }
        }
    }
}