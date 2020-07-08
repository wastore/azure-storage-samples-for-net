using System;
using System.Text;
using Azure.Core.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace LocalKeyClientEncryption
{
    //Sample implementation of IKeyEncryptionKey interface
    //Replace with customer's implementation used to encrypt data using Client Side Encryption
    //Must be same implementation used when first encrypting data
    class SampleKeyEncryptionKey : IKeyEncryptionKey
    {
        string IKeyEncryptionKey.KeyId { get; }
        private readonly byte[] keyEncryptionKey;

        public SampleKeyEncryptionKey(string key)
        {
            keyEncryptionKey = ASCIIEncoding.UTF8.GetBytes(key);
        }

        //sample WrapKey function, replace with customer's implementation
        public virtual byte[] WrapKey(
            string algorithm,
            ReadOnlyMemory<byte> key,
            CancellationToken cancellationToken = default)
        {
            if (algorithm == "ExampleAlgorithm")
            {
                return ExampleKeyWrapAlgorithm(key, keyEncryptionKey);
            }
            return key.ToArray();
        }

        //sample WrapKeyAsync function, replace with customer's implementation
        public virtual async Task<byte[]> WrapKeyAsync(
            string algorithm,
            ReadOnlyMemory<byte> key,
            CancellationToken cancellationToken = default)
        {
            if (algorithm == "ExampleAlgorithm")
            {
                return await ExampleKeyWrapAlgorithmAsync(key, keyEncryptionKey);
            }
            return key.ToArray();
        }

        //sample UnwrapKey function, replace with customer's implementation
        public virtual byte[] UnwrapKey(
            string algorithm,
            ReadOnlyMemory<byte> key,
            CancellationToken cancellationToken = default)
        {
            if (algorithm == "ExampleAlgorithm")
            {
                return ExampleKeyWrapAlgorithm(key, keyEncryptionKey);
            }
            return key.ToArray();
        }

        //sample UnrapKeyAsync function, replace with customer's implementation
        public virtual async Task<byte[]> UnwrapKeyAsync(
            string algorithm,
            ReadOnlyMemory<byte> key,
            CancellationToken cancellationToken = default)
        {
            if (algorithm == "ExampleAlgorithm")
            {
                return await ExampleKeyWrapAlgorithmAsync(key, keyEncryptionKey);
            }
            return key.ToArray();
        }

        //Sample KeyWrapAlgorithm, replace with customer's implementation
        private static byte[] ExampleKeyWrapAlgorithm(ReadOnlyMemory<byte> CEK, byte[] KEK)
        {
            return CEK.ToArray();
        }

        private static async Task<byte[]> ExampleKeyWrapAlgorithmAsync(ReadOnlyMemory<byte> CEK, byte[] KEK)
        {
            return await Task.FromResult(CEK.ToArray());
        }
    }
}
