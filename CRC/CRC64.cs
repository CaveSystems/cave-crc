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
    /// Provides a managed implementation of the Cyclic Redundancy Checksum with 64 bits.
    /// </summary>
    public class CRC64 : HashAlgorithm, IChecksum<ulong>
    {
        /// <summary>
        /// Provides the default polynomial.
        /// </summary>
        public static readonly ulong DefaultPolynomial = 0x42f0e1eba9ea3693;

        /// <summary>
        /// Gets width=64 poly=0x42f0e1eba9ea3693 init=0xffffffffffffffff refin=true refout=true xorout=0xffffffffffffffff check=0x995dc9bbdf1939fa residue=0x49958c9abd7d353f name="CRC-64/XZ".
        /// </summary>
        public static CRC64 XZ => new CRC64(poly: CRC64.DefaultPolynomial, init: 0xffffffffffffffff, finalXor: 0xffffffffffffffff, reflectInput: true, reflectOutput: true, name: "CRC-64/XZ");

        /// <summary>
        /// Gets width=64 poly=0x42f0e1eba9ea3693 init=0xffffffffffffffff refin=false refout=false xorout=0xffffffffffffffff check=0x62ec59e3f1a4f00a residue=0xfcacbebd5931a992 name="CRC-64/WE".
        /// </summary>
        public static CRC64 WE => new CRC64(poly: CRC64.DefaultPolynomial, init: 0xffffffffffffffff, finalXor: 0xffffffffffffffff, reflectInput: false, reflectOutput: false, name: "CRC-64/WE");

        /// <summary>
        /// Gets width=64 poly=0x42f0e1eba9ea3693 init=0x0000000000000000 refin=false refout=false xorout=0x0000000000000000 check=0x6c40df5f0b497347 residue=0x0000000000000000 name="CRC-64".
        /// </summary>
        public static CRC64 ECMA182 => new CRC64(poly: CRC64.DefaultPolynomial, init: 0x0000000000000000, finalXor: 0x0000000000000000, reflectInput: false, reflectOutput: false, name: "CRC-64");

        /// <summary>Reflects 64 bits.</summary>
        /// <param name="x">The bits.</param>
        /// <returns>Returns a center reflection.</returns>
        public static ulong Reflect64(ulong x)
        {
            // move bits
            x = ((x & 0x5555555555555555) << 1) | ((x >> 1) & 0x5555555555555555);
            x = ((x & 0x3333333333333333) << 2) | ((x >> 2) & 0x3333333333333333);
            x = ((x & 0x0F0F0F0F0F0F0F0F) << 4) | ((x >> 4) & 0x0F0F0F0F0F0F0F0F);

            // move bytes
            x = (x << 56) | ((x & 0xFF00) << 40) | ((x & 0xFF0000) << 24) | ((x & 0xFF000000) << 8) | ((x >> 8) & 0xFF000000) | ((x >> 24) & 0xFF0000) | ((x >> 40) & 0xFF00) | (x >> 56);
            return x;
        }

        #region private funtionality
        ulong currentCRC;
        ulong[] table;

        /// <summary>Calculates the table.</summary>
        protected void CalculateTable()
        {
            ulong[] table = new ulong[256];
            for (ulong i = 0; i < 256; i++)
            {
                ulong value = i << 56;
                ulong crc = 0;
                for (ulong n = 0; n < 8; n++)
                {
                    unchecked
                    {
                        if ((long)(crc ^ value) < 0)
                        {
                            crc = (crc << 1) ^ Polynomial;
                        }
                        else
                        {
                            crc = crc << 1;
                        }
                        value <<= 1;
                    }
                }
                table[i] = crc;
            }
            this.table = table;
        }

        private void CalculateReflectedTable()
        {
            ulong poly = Reflect64(Polynomial);
            ulong[] table = new ulong[256];
            for (uint i = 0; i < 256; i++)
            {
                ulong crc = i;
                unchecked
                {
                    for (uint n = 0; n < 8; n++)
                    {
                        if ((crc & 1) != 0)
                        {
                            crc = (crc >> 1) ^ poly;
                        }
                        else
                        {
                            crc = crc >> 1;
                        }
                    }
                }
                table[i] = crc;
            }
            this.table = table;
        }
        #endregion

        /// <summary>The polynomial used to generate the table.</summary>
        public ulong Polynomial;

        /// <summary>The initializer value.</summary>
        public ulong Initializer;

        /// <summary>The final xor value.</summary>
        public ulong FinalXor;

        /// <summary>The reflect input flag.</summary>
        public bool ReflectInput;

        /// <summary>The reflect output flag.</summary>
        public bool ReflectOutput;

        /// <summary>Gets the name of the hash.</summary>
        public string Name { get; }

        /// <summary>Gets the lookup table.</summary>
        /// <value>The table.</value>
        public ulong[] Table => table;

        /// <summary>Gets or sets the checksum computed so far.</summary>
        public ulong Value
        {
            get
            {
                return currentCRC ^ FinalXor;
            }
            set
            {
                currentCRC = value;
            }
        }

        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        public override int HashSize => 32;

        /// <summary>
        /// Gets the value of the computed hash code.
        /// </summary>
#if !NETSTANDARD13 && !NETCOREAPP10
        public override byte[] Hash => BitConverter.GetBytes(Value);
#else
        public byte[] Hash => BitConverter.GetBytes(Value);
#endif

        /// <summary>Initializes a new instance of the <see cref="CRC64"/> class.</summary>
        /// <param name="blueprint">The blueprint to copy all properties from.</param>
        /// <exception cref="NotImplementedException">Throws an error if reflection is uneven.</exception>
        public CRC64(CRC64 blueprint)
        {
            Polynomial = blueprint.Polynomial;
            Initializer = blueprint.Initializer;
            FinalXor = blueprint.FinalXor;
            ReflectInput = blueprint.ReflectInput;
            ReflectOutput = blueprint.ReflectOutput;
            if (ReflectInput != ReflectOutput)
            {
                throw new NotImplementedException("ReflectInput has to match ReflectOutput. Uneven reflection is not implemented!");
            }

            Name = blueprint.Name;
            table = blueprint.table;
            currentCRC = Initializer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CRC64"/> class.
        /// Creates a new CRC64.XZ:
        /// width=64 poly=0x42f0e1eba9ea3693 init=0xffffffffffffffff refin=true refout=true xorout=0xffffffffffffffff check=0x995dc9bbdf1939fa residue=0x49958c9abd7d353f name="CRC-64/XZ".
        /// </summary>
        public CRC64()
            : this(poly: CRC64.DefaultPolynomial, init: 0xffffffffffffffff, finalXor: 0xffffffffffffffff, reflectInput: true, reflectOutput: true, name: "CRC-64/XZ")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CRC64"/> class.</summary>
        /// <param name="poly">The polynom.</param>
        /// <param name="init">The initialize value.</param>
        /// <param name="reflectInput">if set to <c>true</c> [reflect input value] first.</param>
        /// <param name="reflectOutput">if set to <c>true</c> [reflect output value] first.</param>
        /// <param name="finalXor">The final xor value.</param>
        /// <param name="name">The name of the checksum.</param>
        public CRC64(ulong poly, ulong init, bool reflectInput, bool reflectOutput, ulong finalXor, string name)
        {
            Polynomial = poly;
            Initializer = init;
            FinalXor = finalXor;
            ReflectInput = reflectInput;
            ReflectOutput = reflectOutput;
            Name = name;
            if (ReflectInput != ReflectOutput)
            {
                throw new NotImplementedException("ReflectInput has to match ReflectOutput. Uneven reflection is not implemented!");
            }

            if (ReflectInput)
            {
                CalculateReflectedTable();
            }
            else
            {
                CalculateTable();
            }
            currentCRC = Initializer;
        }

        /// <summary>
        /// (Re-)initializes the <see cref="CRC64"/>.
        /// </summary>
        public override void Initialize()
        {
            currentCRC = Initializer;
        }

        /// <summary>
        /// directly hashes one byte.
        /// </summary>
        /// <param name="b">The byte.</param>
        public void HashCore(byte b)
        {
            if (ReflectInput)
            {
                ulong i = (currentCRC ^ b) & 0xFF;
                currentCRC = (currentCRC >> 8) ^ table[i];
            }
            else
            {
                ulong i = ((currentCRC >> 56) ^ b) & 0xFF;
                currentCRC = unchecked((currentCRC << 8) ^ table[i]);
            }
        }

        /// <summary>
        /// Computes the hash for the specified data. The caller needs to <see cref="Initialize"/> the
        /// <see cref="CRC64"/> first and call <see cref="HashFinal"/> afterwards to obtain the
        /// full hash code.
        /// </summary>
        /// <param name="array">Array of bytes to hash.</param>
        /// <param name="ibStart">Start index of data.</param>
        /// <param name="cbSize">Size of data in bytes.</param>
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            for (int i = 0; i < cbSize; i++)
            {
                HashCore(array[ibStart++]);
            }
        }

        /// <summary>
        /// Finalizes the hash computation obtains the resulting hash code in the systems byte order.
        /// </summary>
        /// <returns>Byte array of the hash.</returns>
        protected override byte[] HashFinal()
        {
            return BitConverter.GetBytes(Value);
        }

        /// <summary>Resets the checksum to initialization state.</summary>
        public void Reset()
        {
            Initialize();
        }

        /// <summary>Adds one byte to the checksum.</summary>
        /// <param name="value">the byte to add. Only the lowest 8 bits will be used.</param>
        public void Update(int value)
        {
            HashCore((byte)(value & 0xFF));
        }

        /// <summary>Updates the checksum with the specified byte array.</summary>
        /// <param name="buffer">The buffer containing the data.</param>
        public void Update(byte[] buffer)
        {
            HashCore(buffer, 0, buffer.Length);
        }

        /// <summary>Updates the checksum with the specified byte array.</summary>
        /// <param name="buffer">The buffer containing the data.</param>
        /// <param name="offset">The offset in the buffer where the data starts.</param>
        /// <param name="count">the number of data bytes to add.</param>
        public void Update(byte[] buffer, int offset, int count)
        {
            HashCore(buffer, offset, count);
        }

        /// <summary>Returns a <see cref="string" /> that represents this instance.</summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Name + " width=32 poly=" + Polynomial + " init=" + Initializer + " refin=" + ReflectInput + " refout=" + ReflectOutput + " xorout=" + FinalXor;
        }

        /// <summary>Erstellt ein neues Objekt, das eine Kopie der aktuellen Instanz darstellt.</summary>
        /// <returns>Ein neues Objekt, das eine Kopie dieser Instanz darstellt.</returns>
        public object Clone()
        {
            return new CRC64(this);
        }
    }
}
