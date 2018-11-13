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

using System;
using System.Security.Cryptography;

namespace Cave
{
    /// <summary>
    /// Implements a fast implementation of the CRC-CCITT-16 algorithm for the polynomial 0x1021
    /// </summary>
    public class CRCCCITT16 : HashAlgorithm, IChecksum<ushort>
    {
        uint crc = ushort.MaxValue;

        /// <summary>
        /// Returns the checksum computed so far.
        /// </summary>
        public ushort Value => (ushort)(crc & 0xFFFF);

        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        public override int HashSize => 16;

        /// <summary>
        /// Initializes an implementation of the HashAlgorithm class.
        /// </summary>
        public override void Initialize() => crc = ushort.MaxValue;

        /// <summary>
        /// Resets the checksum to initialization state
        /// </summary>
        public void Reset() => Initialize();

        /// <summary>Adds one byte to the checksum.</summary>
        /// <param name="value">the byte to add. Only the lowest 8 bits will be used.</param>
        public void Update(int value)
        {
            byte v = (byte)(value &= 0xFF);
            uint x = crc >> 8;
            x ^= v;
            x ^= x >> 4;
            crc = (crc << 8) ^ (x << 12) ^ (x << 5) ^ x;
        }

        /// <summary>Updates the checksum with the specified byte array.</summary>
        /// <param name="buffer">buffer an array of bytes</param>
        public void Update(byte[] buffer)
        {
            for(int i = 0; i < buffer.Length; i++)
            {
                uint x = crc >> 8;
                x ^= buffer[i];
                x ^= x >> 4;
                crc = (crc << 8) ^ (x << 12) ^ (x << 5) ^ x;
            }
        }

        /// <summary>Updates the checksum with the specified byte array.</summary>
        /// <param name="buffer">The buffer containing the data</param>
        /// <param name="offset">The offset in the buffer where the data starts</param>
        /// <param name="count">the number of data bytes to add.</param>
        public void Update(byte[] buffer, int offset, int count)
        {
            int end = offset + count;
            for (int i = offset; i < end; i++)
            {
                uint x = crc >> 8;
                x ^= buffer[i];
                x ^= x >> 4;
                crc = (crc << 8) ^ (x << 12) ^ (x << 5) ^ x;
            }
        }

        /// <summary>
        /// Routes data written to the object into the hash algorithm for computing the hash.
        /// </summary>
        /// <remarks>
        /// This method is not called by application code.<br/>
        /// This abstract method performs the hash computation.Every write to the cryptographic stream object passes the data through this method.For each block of data, this method updates the state of the hash object so a correct hash value is returned at the end of the data stream.
        /// </remarks>
        /// <param name="array">The input to compute the hash code for.</param>
        /// <param name="ibStart">The offset into the byte array from which to begin using data.</param>
        /// <param name="cbSize">The number of bytes in the byte array to use as data.</param>
        protected override void HashCore(byte[] array, int ibStart, int cbSize) => Update(array, ibStart, cbSize);

        /// <summary>
        /// Finalizes the hash computation after the last data is processed by the cryptographic stream object.
        /// </summary>
        /// <remarks>This method finalizes any partial computation and returns the correct hash value for the data stream.</remarks>
        /// <returns>The computed hash code.</returns>
        protected override byte[] HashFinal() => Hash;

        /// <summary>
        /// Gets the value of the computed hash code.
        /// </summary>
#if !NETSTANDARD13 && !NETCOREAPP10
        override
#endif
        public byte[] Hash => BitConverter.GetBytes(Value);
    }
}
