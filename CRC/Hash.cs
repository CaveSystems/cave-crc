using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cave
{
    /// <summary>
    /// Provides thread safe hashing.
    /// </summary>
    public static class Hash
    {
        /// <summary>
        /// Available hash types.
        /// </summary>
        public enum Type
        {
            /// <summary>The none</summary>
            None,

            /// <summary>The crc32 hash algorithm</summary>
            CRC32,

            /// <summary>The crc64 hash algorithm</summary>
            CRC64,

            /// <summary>The md5 hash algorithm</summary>
            MD5,

            /// <summary>The sha1 hash algorithm</summary>
            SHA1,

            /// <summary>The sha256 hash algorithm</summary>
            SHA256,

            /// <summary>The sha384 hash algorithm</summary>
            SHA384,

            /// <summary>The sha512 hash algorithm</summary>
            SHA512,
        }

        /// <summary>Creates a hash of the specified type.</summary>
        /// <param name="type">The type.</param>
        /// <returns>Returns a new HashAlgorithm instance.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public static HashAlgorithm Create(Type type)
        {
            switch (type)
            {
                case Type.CRC32: return new CRC32();
                case Type.CRC64: return new CRC64();
                case Type.MD5: return MD5.Create();
                case Type.SHA1: return SHA1.Create();
                case Type.SHA256: return SHA256.Create();
                case Type.SHA384: return SHA384.Create();
                case Type.SHA512: return SHA512.Create();
                default: throw new NotImplementedException();
            }
        }

        /// <summary>Obtains the hash code for a specified data array.</summary>
        /// <param name="type">The type.</param>
        /// <param name="data">The bytes to hash.</param>
        /// <returns>A new byte[] containing the hash for the specified data.</returns>
        /// <exception cref="ArgumentNullException">data.</exception>
        public static byte[] FromArray(Type type, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            using (HashAlgorithm algorithm = Create(type))
            {
                algorithm.Initialize();
                return algorithm.ComputeHash(data);
            }
        }

        /// <summary>Obtains the hash code for a specified data array.</summary>
        /// <param name="type">The type.</param>
        /// <param name="data">The byte array to hash.</param>
        /// <param name="index">The start index.</param>
        /// <param name="count">The number of bytes to hash.</param>
        /// <returns>A new byte[] containing the hash for the specified data.</returns>
        /// <exception cref="ArgumentNullException">data.</exception>
        public static byte[] FromArray(Type type, byte[] data, int index, int count)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            using (HashAlgorithm algorithm = Create(type))
            {
                return algorithm.ComputeHash(data, index, count);
            }
        }

        /// <summary>
        /// Obtains the hash code for a specified data string (using UTF-8 encoding).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="data">The string to hash.</param>
        /// <param name="index">The start index.</param>
        /// <param name="count">The number of chars to hash.</param>
        /// <returns>A new byte[] containing the hash for the specified data.</returns>
        /// <exception cref="ArgumentNullException">data.</exception>
        public static byte[] FromString(Type type, string data, int index, int count)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return FromArray(type, Encoding.UTF8.GetBytes(data.Substring(index, count)));
        }

        /// <summary>Obtains the hash code for a specified data string (using UTF-8 encoding).</summary>
        /// <param name="type">The type.</param>
        /// <param name="data">The string to hash.</param>
        /// <returns>A new byte[] containing the hash for the specified data.</returns>
        /// <exception cref="ArgumentNullException">data.</exception>
        public static byte[] FromString(Type type, string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return FromArray(type, Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// Obtains the hash code for a specified <see cref="Stream" /> string at the current position and
        /// reading to the end of the stream.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="stream">The stream to hash.</param>
        /// <returns>A new byte[] containing the hash for the specified data.</returns>
        /// <exception cref="ArgumentNullException">stream.</exception>
        public static byte[] FromStream(Type type, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            using (HashAlgorithm algorithm = Create(type))
            {
                return algorithm.ComputeHash(stream);
            }
        }
    }
}
