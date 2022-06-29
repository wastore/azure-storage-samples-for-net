using Azure.Core.Cryptography;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Storage.Blobs.EncryptionMigration.Tests
{
    internal static class Clients
    {
        private const string KeyId = "MockKeyId";

        public static BlobServiceClient GetServiceClient()
        {
            string? connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            return new BlobServiceClient(connectionString);
        }

        public static Mock<IKeyEncryptionKey> GetMockIKeyEncryptionKey()
        {
            static byte[] Not(byte[] contents)
            {
                var result = new byte[contents.Length];
                // just bitflip the contents
                new System.Collections.BitArray(contents).Not().CopyTo(result, 0);

                return result;
            }

            var mock = new Mock<IKeyEncryptionKey>(MockBehavior.Strict);
            mock.SetupGet(m => m.KeyId).Returns(KeyId);
            mock.Setup(m => m.WrapKeyAsync(It.IsAny<string>(), It.IsNotNull<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()))
                .Returns<string, ReadOnlyMemory<byte>, CancellationToken>((algorithm, key, cancellationToken) =>
                {
                    return Task.FromResult(Not(key.ToArray()));
                });
            mock.Setup(m => m.UnwrapKeyAsync(It.IsAny<string>(), It.IsNotNull<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()))
                .Returns<string, ReadOnlyMemory<byte>, CancellationToken>((algorithm, wrappedKey, cancellationToken) =>
                {
                    return Task.FromResult(Not(wrappedKey.ToArray()));
                });

            return mock;
        }

        public static Mock<IKeyEncryptionKeyResolver> GetMockIKeyEncryptionKeyResolver(
            params IKeyEncryptionKey[] keys)
        {
            IKeyEncryptionKey Resolve(string keyId, CancellationToken cancellationToken)
                => keys.FirstOrDefault(k => k.KeyId == keyId) ?? throw new Exception("Mock resolver couldn't resolve key id.");

            var mock = new Mock<IKeyEncryptionKeyResolver>(MockBehavior.Strict);
            mock.Setup(r => r.ResolveAsync(It.IsNotNull<string>(), It.IsAny<CancellationToken>()))
                .Returns<string, CancellationToken>((keyId, cancellationToken) => Task.FromResult(Resolve(keyId, cancellationToken)));

            return mock;
        }
    }
}
