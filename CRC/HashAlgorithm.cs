#region CopyRight 2018
/*
    Copyright (c) 2003-2018 Andreas Rohleder (andreas@rohleder.cc)
    All rights reserved
*/
#endregion
#region License LGPL-3
/*
    This program/library/sourcecode is free software; you can redistribute it
    and/or modify it under the terms of the GNU Lesser General Public License
    version 3 as published by the Free Software Foundation subsequent called
    the License.

    You may not use this program/library/sourcecode except in compliance
    with the License. The License is included in the LICENSE file
    found at the installation directory or the distribution package.

    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the
    "Software"), to deal in the Software without restriction, including
    without limitation the rights to use, copy, modify, merge, publish,
    distribute, sublicense, and/or sell copies of the Software, and to
    permit persons to whom the Software is furnished to do so, subject to
    the following conditions:

    The above copyright notice and this permission notice shall be included
    in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
#region Authors & Contributors
/*
   Author:
     Andreas Rohleder <andreas@rohleder.cc>

   Contributors:
 */
#endregion

#if NETSTANDARD10

using System.IO;

namespace System.Security.Cryptography
{
    /// <summary>
    /// Provides the missing class for netstandard 1.0
    /// </summary>
    public abstract class HashAlgorithm : IDisposable
    {
        bool disposed = false;

        /// <summary>
        /// Current Hash Value
        /// </summary>
        protected internal byte[] HashValue;

        /// <summary>
        /// Current Hash State
        /// </summary>
        protected int State = 0;

        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        public abstract int HashSize { get; }

        /// <summary>
        /// Releases all resources used by the HashAlgorithm class.
        /// </summary>
        public void Clear() => Dispose();

        /// <summary>
        /// Gets the value of the computed hash code.
        /// </summary>
        public virtual byte[] Hash
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(null);
                }

                if (State != 0)
                {
                    throw new Exception("Hash Not Yet Finalized");
                }

                return (byte[])HashValue.Clone();
            }
        }

        /// <summary>
        /// Computes the hash value for the specified Stream object.
        /// </summary>
        /// <param name="inputStream">The input to compute the hash code for.</param>
        /// <returns>The computed hash code.</returns>
        public byte[] ComputeHash(Stream inputStream)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            // Default the buffer size to 4K.
            byte[] buffer = new byte[4096];
            int bytesRead;
            do
            {
                bytesRead = inputStream.Read(buffer, 0, 4096);
                if (bytesRead > 0)
                {
                    HashCore(buffer, 0, bytesRead);
                }
            }
            while (bytesRead > 0);

            HashValue = HashFinal();
            byte[] Tmp = (byte[])HashValue.Clone();
            Initialize();
            return (Tmp);
        }

        /// <summary>
        /// Computes the hash value for the specified byte array.
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <returns>The computed hash code.</returns>
        public byte[] ComputeHash(byte[] buffer)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            HashCore(buffer, 0, buffer.Length);
            HashValue = HashFinal();
            byte[] Tmp = (byte[])HashValue.Clone();
            Initialize();
            return (Tmp);
        }

        /// <summary>
        /// Computes the hash value for the specified region of the specified byte array.
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <param name="offset">The offset into the byte array from which to begin using data.</param>
        /// <param name="count">The number of bytes in the array to use as data.</param>
        /// <returns>The computed hash code.</returns>
        public byte[] ComputeHash(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "ArgumentOutOfRange_NeedNonNegNum");
            }

            if (count < 0 || (count > buffer.Length))
            {
                throw new ArgumentException("Argument_InvalidValue");
            }

            if ((buffer.Length - count) < offset)
            {
                throw new ArgumentException("Argument_InvalidOffLen");
            }

            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            HashCore(buffer, offset, count);
            HashValue = HashFinal();
            byte[] Tmp = (byte[])HashValue.Clone();
            Initialize();
            return (Tmp);
        }

        /// <summary>
        /// When overridden in a derived class, gets the input block size.
        /// </summary>
        public virtual int InputBlockSize => 1;

        /// <summary>
        /// When overridden in a derived class, gets the output block size.
        /// </summary>
        public virtual int OutputBlockSize => 1;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether multiple blocks can be transformed.
        /// </summary>
        public virtual bool CanTransformMultipleBlocks => true;

        /// <summary>
        /// Gets a value indicating whether the current transform can be reused.
        /// </summary>
        public virtual bool CanReuseTransform => true;

        /// <summary>
        /// Computes the hash value for the specified region of the input byte array and copies the specified region of the input byte array to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input to compute the hash code for.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">A copy of the part of the input array used to compute the hash code.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>The number of bytes written.</returns>
        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            // Do some validation, we let BlockCopy do the destination array validation
            if (inputBuffer == null)
            {
                throw new ArgumentNullException("inputBuffer");
            }

            if (inputOffset < 0)
            {
                throw new ArgumentOutOfRangeException("inputOffset", "ArgumentOutOfRange_NeedNonNegNum");
            }

            if (inputCount < 0 || (inputCount > inputBuffer.Length))
            {
                throw new ArgumentException("Argument_InvalidValue");
            }

            if ((inputBuffer.Length - inputCount) < inputOffset)
            {
                throw new ArgumentException("Argument_InvalidOffLen");
            }

            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            // Change the State value
            State = 1;
            HashCore(inputBuffer, inputOffset, inputCount);
            if ((outputBuffer != null) && ((inputBuffer != outputBuffer) || (inputOffset != outputOffset)))
            {
                Buffer.BlockCopy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
            }
            return inputCount;
        }

        /// <summary>
        /// Computes the hash value for the specified region of the specified byte array.
        /// </summary>
        /// <param name="inputBuffer">The input to compute the hash code for.</param>
        /// <param name="inputOffset">The offset into the byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the byte array to use as data.</param>
        /// <returns>An array that is a copy of the part of the input that is hashed.</returns>
        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            // Do some validation
            if (inputBuffer == null)
            {
                throw new ArgumentNullException("inputBuffer");
            }

            if (inputOffset < 0)
            {
                throw new ArgumentOutOfRangeException("inputOffset", "ArgumentOutOfRange_NeedNonNegNum");
            }

            if (inputCount < 0 || (inputCount > inputBuffer.Length))
            {
                throw new ArgumentException("Argument_InvalidValue");
            }

            if ((inputBuffer.Length - inputCount) < inputOffset)
            {
                throw new ArgumentException("Argument_InvalidOffLen");
            }

            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            HashCore(inputBuffer, inputOffset, inputCount);
            HashValue = HashFinal();
            byte[] outputBytes;
            if (inputCount != 0)
            {
                outputBytes = new byte[inputCount];
                Buffer.BlockCopy(inputBuffer, inputOffset, outputBytes, 0, inputCount);
            }
            else
            {
                outputBytes = new byte[0];
            }
            // reset the State value
            State = 0;
            return outputBytes;
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (HashValue != null)
                {
                    Array.Clear(HashValue, 0, HashValue.Length);
                }

                State = 0;
                HashValue = null;
                disposed = true;
            }
        }

        /// <summary>
        /// Initializes an implementation of the HashAlgorithm class.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// When overridden in a derived class, routes data written to the object into the hash algorithm for computing the hash.
        /// </summary>
        /// <remarks>
        /// This method is not called by application code.<br/>
        /// This abstract method performs the hash computation.Every write to the cryptographic stream object passes the data through this method.For each block of data, this method updates the state of the hash object so a correct hash value is returned at the end of the data stream.
        /// </remarks>
        /// <param name="array">The input to compute the hash code for.</param>
        /// <param name="ibStart">The offset into the byte array from which to begin using data.</param>
        /// <param name="cbSize">The number of bytes in the byte array to use as data.</param>
        protected abstract void HashCore(byte[] array, int ibStart, int cbSize);

        /// <summary>
        /// When overridden in a derived class, finalizes the hash computation after the last data is processed by the cryptographic stream object.
        /// </summary>
        /// <remarks>This method finalizes any partial computation and returns the correct hash value for the data stream.</remarks>
        /// <returns>The computed hash code.</returns>
        protected abstract byte[] HashFinal();
    }
}

#endif
